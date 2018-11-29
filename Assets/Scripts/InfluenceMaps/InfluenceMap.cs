using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class InfluenceMap : MonoBehaviour {

    public SharedGrid grid;
    public ObstacleHeightMap obstacleHeights;
    public float decayPerSecond = 1;
    public float visualizedMaxValue;
    public float updateFrequency = 30;

    public float directionModifier;

    [Space]
    public bool isPresenceMap = false;

    private float[,] addMap;
    private float[,] decayMap;
    private List<InfluenceSource> sources = new List<InfluenceSource>();

    internal Texture2D tex;

    private float[] defaultNeighborDiminish = { 1, 1, 1, 1, 1, 1, 1, 1 };

    private float updateTime;

    private const float zeroThreshold = 0.1f;

    public delegate void UpdateDelegate();
    public UpdateDelegate UpdateMapDelegates;

    internal bool standalone = true;

    void Start () {
        if (grid == null)
            Debug.LogError("Missing SharedGrid instance!");

        addMap = new float[grid.GetWidth(), grid.GetHeight()];
        decayMap = new float[grid.GetWidth(), grid.GetHeight()];
        updateTime = 1 / updateFrequency;

        tex = new Texture2D(grid.GetWidth(), grid.GetHeight());
        if (standalone)
            GetComponent<Renderer>().material.mainTexture = tex;
        tex.filterMode = FilterMode.Point;
        if (standalone)
            StartCoroutine("ParallelUpdateMap"); // ACTUALLY BETTER TO RUN IN PARALLEL!
    }

    private void OnValidate() {
        updateFrequency = Mathf.Clamp(updateFrequency, 0.01f, float.MaxValue);
        decayPerSecond = Mathf.Clamp(decayPerSecond, 0, float.MaxValue);
        visualizedMaxValue = Mathf.Clamp(visualizedMaxValue, 0, float.MaxValue);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = GetComponent<MeshRenderer>() != null ? GetComponent<MeshRenderer>().sharedMaterial.color : Color.white;
        if (sources != null) {
            foreach (InfluenceSource source in sources) {
                Gizmos.DrawSphere(source.transform.position, 0.2f);
            }
        }
    }

    public void AddInfluenceSource(InfluenceSource source) {
        if (!sources.Contains(source)) {
            sources.Add(source);
        }
    }

    public void RemoveInfluenceSource(InfluenceSource source) {
        sources.Remove(source);
    }

    public bool InBounds(Vector2Int pos) {
        return grid.InBounds(pos);
    }

    public void AddInfluence(float influence, Vector2Int pos) {
        if (grid.InBounds(pos)) {
            addMap[pos.x, pos.y] += influence;
        }
    }

    public float GetInfluence(Vector3 point) {
        Vector2Int pos = grid.WorldToGrid(point);
        if (!grid.InBounds(pos))
            return 0;
        return GetInfluence(pos.x, pos.y);
    }

    public float GetInfluence(int x, int y) {
        return decayMap[x, y];
    }

    public void Display() {
        grid.ForEachCell((x, y) => {
            float c = GetInfluence(x, y);
            float a = Mathf.Approximately(c, 0) ? 0 : 1f;
            tex.SetPixel(x, y, new Color(c / visualizedMaxValue, c / visualizedMaxValue, c / visualizedMaxValue, a));
        });
        tex.Apply();
    }

    #region Parallel

    private int completed;

    private class ThreadData {
        public Vector3 position;
        public Vector2Int gridPos;
        public Vector3 direction;
        public InfluenceSource source;
        public Bounds bounds;
    }

    public IEnumerator UpdateInfluencesParallel() {
        grid.ForEachCell((x, y) => {
            if (!Mathf.Approximately(decayMap[x, y], 0)) {
                decayMap[x, y] -= Mathf.Sign(decayMap[x, y]) * decayPerSecond * updateTime;
                if (Mathf.Abs(decayMap[x, y]) <= zeroThreshold) {
                    decayMap[x, y] = 0;
                }
            }
        });

        // Clear addition layer
        addMap = new float[grid.GetWidth(), grid.GetHeight()];
        // Add values from InfluenceSources

        completed = 0;
        foreach (var source in sources) {
            ThreadData data = new ThreadData();
            data.position = source.gameObject.transform.position;
            data.gridPos = grid.WorldToGrid(source.gameObject.transform.position);
            data.direction = source.sourceDirection;
            data.source = source;
            if (source.spreadBounds != null)
                data.bounds = source.spreadBounds.bounds;
            else if (source.GetComponentInChildren<Renderer>() != null)
                data.bounds = source.GetComponentInChildren<Renderer>().bounds;
            else
                data.bounds = source.transform.parent.GetComponentInChildren<Renderer>().bounds;

            try {
                ThreadPool.QueueUserWorkItem(new WaitCallback(InsertNewValuesParallel), data);
            } catch (NotSupportedException e) {
                Debug.LogError(e);
            }
        }

        yield return new WaitUntil(() => completed >= sources.Count);
    }

    public IEnumerator ParallelUpdateMap() {
        while (true) {

            yield return UpdateInfluencesParallel();

            Display();
            if (UpdateMapDelegates != null)
                UpdateMapDelegates();
            yield return new WaitForSeconds(updateTime);
        }
    }

    static float Add(ref float location1, float value) {
        float newCurrentValue = location1; // non-volatile read, so may be stale
        while (true) {
            float currentValue = newCurrentValue;
            float newValue = currentValue + value;
            newCurrentValue = Interlocked.CompareExchange(ref location1, newValue, currentValue);
            if (newCurrentValue == currentValue)
                return newValue;
        }
    }

    void InsertNewValuesParallel(object obj) {
        try {
            ThreadData data = obj as ThreadData;
            InfluenceSource source = data.source;

            // Get middle point for angle calculation
            var middle = new Vector2Int();
            int count = 0;
            foreach (var pos in grid.ProjectGridPos(data.bounds)) {
                middle += pos;
                count++;
            }
            if (count == 0) {
                middle = data.gridPos;
            } else {
                middle.x /= count;
                middle.y /= count;
            }

            float angleCalc = 1;
            var dir = new Vector2(data.direction.x, data.direction.z);

            grid.BFS(grid.ProjectGridPos(data.bounds), source.range, (pos, it) => {
                if (dir != Vector2.zero) {
                    float dot = (Vector2.Dot(dir.normalized, new Vector2(pos.x - middle.x, pos.y - middle.y).normalized) + 1) / 2;
                    angleCalc = dot;
                }
                var addVal = source.GetValue(it, source.sourceValue, source.range) * angleCalc;
                Add(ref addMap[pos.x, pos.y], addVal);
                Interlocked.Exchange(ref decayMap[pos.x, pos.y], Mathf.Max(addMap[pos.x, pos.y], decayMap[pos.x, pos.y]));
            }, (neighbor) => {
                return Mathf.Approximately(obstacleHeights.GetHeight(neighbor), 0);
            }, (neighbor) => {
                return dir != Vector2.zero ? angleCalc * directionModifier * data.direction.magnitude : 0;
            });
            Interlocked.Increment(ref completed);
        } catch (Exception e) {
            Debug.LogException(e);
        }
    }

    #endregion Parallel
}
