using ProtoTurtle.BitmapDrawing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class InfluenceMap : MonoBehaviour {

    public SharedGrid grid;
    public ObstacleHeightMap obstacleHeights;
    public float decayPerSecond = 1;
    public float visualizedMaxValue;
    public float updateFrequency = 30;

    private float[,] addMap;
    private float[,] decayMap;
    private List<InfluenceSource> sources = new List<InfluenceSource>();

    Texture2D tex;

    private float[] defaultNeighborDiminish = { 1, 1, 1, 1, 1, 1, 1, 1 };

    private float updateTime;

    private const float zeroThreshold = 0.1f;

    void Start () {
        if (grid == null)
            Debug.LogError("Missing SharedGrid instance!");
        addMap = new float[grid.GetWidth(), grid.GetHeight()];
        decayMap = new float[grid.GetWidth(), grid.GetHeight()];
        updateTime = 1 / updateFrequency;

        tex = new Texture2D(grid.GetWidth(), grid.GetHeight());
        GetComponent<Renderer>().material.mainTexture = tex;
        tex.filterMode = FilterMode.Point;
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

    void SetInfluence(float influence, Vector2Int pos) {
        if (grid.InBounds(pos)) {
            addMap[pos.x, pos.y] = influence;
        }
    }

    void AddInfluence(float influence, Vector2Int pos) {
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
            float a = Mathf.Approximately(c, 0) ? 0 : 0.5f;
            tex.SetPixel(x, y, new Color(c / visualizedMaxValue, c / visualizedMaxValue, c / visualizedMaxValue, a));
        });
        tex.Apply();
    }

    public IEnumerator UpdateMap() {
        while (true) {
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

            foreach (InfluenceSource source in sources) {
                InsertNewValues(source);
            }

            Display();
            yield return new WaitForSeconds(updateTime);
        }
    }
    
    void InsertNewValues(InfluenceSource source) {
        grid.BFS(grid.ProjectGridPos(source.GetComponentInChildren<Renderer>().bounds), source.range, (pos, it) => {
            addMap[pos.x, pos.y] += source.GetValue(it, source.sourceValue, source.range);
            decayMap[pos.x, pos.y] = Mathf.Max(addMap[pos.x, pos.y], decayMap[pos.x, pos.y]);
        }, (neighbor) => {
            return Mathf.Approximately(obstacleHeights.GetHeight(neighbor), 0);
        });
    }

    #region Parallel

    private int completed;

    private class ThreadData {
        public InfluenceSource source;
        public Bounds bounds;
    }

    public IEnumerator ParallelUpdateMap() {
        while (true) {
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
                data.source = source;
                data.bounds = source.GetComponentInChildren<Renderer>().bounds;
                ThreadPool.QueueUserWorkItem(new WaitCallback(InsertNewValuesParallel), data);
            }

            yield return new WaitUntil(() => completed >= sources.Count);

            Display();
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
        ThreadData data = obj as ThreadData;
        InfluenceSource source = data.source;
        grid.BFS(grid.ProjectGridPos(data.bounds), source.range, (pos, it) => {
            Add(ref addMap[pos.x, pos.y], source.GetValue(it, source.sourceValue, source.range));
            Interlocked.Exchange(ref decayMap[pos.x, pos.y], Mathf.Max(addMap[pos.x, pos.y], decayMap[pos.x, pos.y]));
        }, (neighbor) => {
            return Mathf.Approximately(obstacleHeights.GetHeight(neighbor), 0);
        });
        Interlocked.Increment(ref completed);
    }

    #endregion Parallel
}
