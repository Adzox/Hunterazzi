using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InfluenceMap : MonoBehaviour {

    [Tooltip("The parent GameObject for all obstacles.")]
    // Replace with Navmesh!
    public GameObject obstacles;
    public float cellSize;
    public Vector2 dimensions;
    private Vector3 origo;

    private const float decay = 2;

    private List<InfluenceSource> sources = new List<InfluenceSource>();

    [Range(1, 5)]
    public int layers;
    public float[] layerInfluences; 
    private int currentLayer;

    int width;
    int height;
    private float[][,] mapz;

    Texture2D tex;

    private float[] defaultNeighborDiminish = { 1, 1, 1, 1, 1, 1, 1, 1 };

    public float updateFrequency = 30;
    private float updateTime;

    private void OnValidate() {
        if (layerInfluences.Length != layers) {
            Array.Resize(ref layerInfluences, layers);
        }
    }

    void Start () {
        origo = transform.position - new Vector3(dimensions.x * transform.lossyScale.x, 0, dimensions.y * transform.lossyScale.z) / 2;
        width = Mathf.FloorToInt(dimensions.x / cellSize);
        height = Mathf.FloorToInt(dimensions.y / cellSize);
        mapz = new float[layers][,];
        currentLayer = 0;
        mapz[currentLayer] = new float[width, height];
        updateTime = 1 / updateFrequency;

        tex = new Texture2D(width, height);
        GetComponent<Renderer>().material.mainTexture = tex;
        tex.filterMode = FilterMode.Point;
        StartCoroutine("UpdateMap");
    }

    private void OnDrawGizmosSelected() {
        origo = transform.position - new Vector3(dimensions.x * transform.lossyScale.x, 0, dimensions.y * transform.lossyScale.z) / 2;
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(origo, 0.1f);
        Gizmos.color = GetComponent<MeshRenderer>() != null ? GetComponent<MeshRenderer>().sharedMaterial.color : Color.white;
        if (sources != null) {
            foreach (InfluenceSource source in sources) {
                Gizmos.DrawSphere(source.transform.position, 0.1f);
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
        if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height) {
            mapz[currentLayer][pos.x, pos.y] = influence;
        }
    }

    void AddInfluence(float influence, Vector2Int pos) {
        if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height) {
            mapz[currentLayer][pos.x, pos.y] += influence;
        }
    }

    public bool InBounds(Vector2Int pos) {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public Vector2Int WorldToGrid(Vector3 point) {
        var pos = point - origo;        
        int x = Mathf.FloorToInt(pos.x / cellSize);
        int y = Mathf.FloorToInt(pos.z / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(Vector2Int pos) {
        return new Vector3(pos.x * cellSize, 0, pos.y * cellSize) + origo;
    }

    public float GetInfluence(Vector3 point) {
        Vector2Int pos = WorldToGrid(point);
        if (!InBounds(pos))
            return 0;
        return GetInfluence(pos.x, pos.y);
    }

    private float GetInfluence(int x, int y) {
        float total = 0;
        for (int i = 0; i < layers; i++) {
            int l = (i + currentLayer) % layers;
            total += mapz[l][x, y] * layerInfluences[i];
        }
        return total;
    }

    public void Display() {
        for (int i = 0; i < width; ++i) {
            for (int j = 0; j < height; ++j) {
                float c = GetInfluence(i, j);
                tex.SetPixel(i, j, new Color(c, c, c, 0.5f));
            }
        }
        tex.Apply();
    }

    public IEnumerator UpdateMap() {
        while (true) {
            currentLayer++;
            if (currentLayer >= layers)
                currentLayer = 0;
            // Clear current layer
            mapz[currentLayer] = new float[width, height];

            // Add values from InfluenceSources
            foreach (InfluenceSource source in sources) {
                InsertNewValues(source, defaultNeighborDiminish);
            }
            Display();
            yield return new WaitForSeconds(updateTime);
        }
    }
    
    void InsertNewValues(InfluenceSource source, float[] neighborDiminish) {
        if (InBounds(WorldToGrid(source.transform.position))) {
            var pos = WorldToGrid(source.transform.position);
            var visited = new HashSet<Vector2Int>();
            DFS(source, pos, pos, visited);
        }
    }

    float PositionShapeValue(float distance, float sourceValue, float maxDistance) {
        return -(distance - maxDistance) * (distance + maxDistance) / (maxDistance * maxDistance / sourceValue);
    }

    void DFS(InfluenceSource source, Vector2Int startPos, Vector2Int pos, HashSet<Vector2Int> visited) {
        float distance = Vector2Int.Distance(startPos, pos);
        if (distance >= source.range) {
            return;
        } else {
            visited.Add(pos);
            mapz[currentLayer][pos.x, pos.y] += PositionShapeValue(distance, source.sourceValue, source.range);
            foreach (var neighbor in GetNeighbors(pos)) {
                if (InBounds(neighbor) && !visited.Contains(neighbor)) {
                    DFS(source, startPos, neighbor, visited);
                }
            }
        }
    }
}
