using ProtoTurtle.BitmapDrawing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        StartCoroutine("UpdateMap");
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

    IEnumerable<Vector2Int> GetNeighbors(Vector2Int pos) {
        return new List<Vector2Int>() {
            new Vector2Int(pos.x - 1, pos.y + 1), new Vector2Int(pos.x, pos.y + 1), new Vector2Int(pos.x + 1, pos.y + 1),
            new Vector2Int(pos.x - 1, pos.y),                                       new Vector2Int(pos.x + 1, pos.y),
            new Vector2Int(pos.x - 1, pos.y - 1), new Vector2Int(pos.x, pos.y - 1), new Vector2Int(pos.x + 1, pos.y - 1),
        };
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

    private float GetInfluence(int x, int y) {
        return decayMap[x, y];
    }

    public void Display() {
        grid.ForEachCell((x, y) => {
            float c = GetInfluence(x, y);
            tex.SetPixel(x, y, new Color(c / visualizedMaxValue, c / visualizedMaxValue, c / visualizedMaxValue, 0.5f));
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
                InsertNewValues(source, defaultNeighborDiminish);
            }

            Display();
            yield return new WaitForSeconds(updateTime);
        }
    }
    
    void InsertNewValues(InfluenceSource source, float[] neighborDiminish) {
        grid.BFS(grid.ProjectGridPos(source.GetComponentInChildren<Renderer>().bounds), source.range, (pos, it) => {
            addMap[pos.x, pos.y] += source.GetValue(it, source.sourceValue, source.range);
            decayMap[pos.x, pos.y] = Mathf.Max(addMap[pos.x, pos.y], decayMap[pos.x, pos.y]);
        }, (neighbor) => {
            return Mathf.Approximately(obstacleHeights.GetHeight(neighbor), 0);
        });
    }
}
