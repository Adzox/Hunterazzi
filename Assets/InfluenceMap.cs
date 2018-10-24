﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InfluenceMap : MonoBehaviour {

    [Tooltip("The parent GameObject for all obstacles.")]
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

    private float[] defaultNeighborDiminish = { 1, 1, 1, 1, 1, 1, 1, 1 };

    public float updateFrequency = 30;
    public float updateTime;

    private void OnValidate() {
        if (layerInfluences.Length != layers) {
            Array.Resize(ref layerInfluences, layers);
        }
    }

    // Use this for initialization
    void Start () {
        origo = transform.position - new Vector3(dimensions.x, 0, dimensions.y) / 2;
        width = Mathf.FloorToInt(dimensions.x / cellSize);
        height = Mathf.FloorToInt(dimensions.y / cellSize);
        mapz = new float[layers][,];
        currentLayer = 0;
        mapz[currentLayer] = new float[width, height];
        updateTime = 1 / updateFrequency;
        StartCoroutine("UpdateMap");
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position - new Vector3(dimensions.x, 0, dimensions.y) / 2, 0.1f);
        Gizmos.color = Color.blue;
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
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.z);
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
            total += mapz[l][x, y] * (1 - i / layers);
        }
        return 0;
    }

    public IEnumerator UpdateMap() {
        while (true) {
            Debug.Log("Current layer: " + currentLayer);
            currentLayer++;
            if (currentLayer >= layers)
                currentLayer = 0;
            // Clear current layer
            mapz[currentLayer] = new float[width, height];

            // Add values from InfluenceSources
            foreach (InfluenceSource source in sources) {
                InsertNewValues(source, defaultNeighborDiminish);
            }
            yield return new WaitForSeconds(updateTime);
        }
    }
    
    void InsertNewValues(InfluenceSource source, float[] neighborDiminish) {
        if (InBounds(WorldToGrid(source.transform.position))) {
            DFSAdd(source.sourceValue, 1, WorldToGrid(source.transform.position), new HashSet<Vector2Int>(), neighborDiminish, 0, source.range);
        }
    }

    void DFSAdd(float startValue, float modifier, Vector2Int pos, HashSet<Vector2Int> visited, float[] neighborDiminish, int iteration, int maxIterations) {
        if (iteration >= maxIterations) {
            return;
        } else {
            float value = GetValue(maxIterations, iteration, startValue);
            mapz[currentLayer][pos.x, pos.y] += value * modifier;
            foreach (var pair in Extensions.Zip(GetNeighbors(pos), neighborDiminish)) {
                if (InBounds(pair.Key) && !visited.Contains(pair.Key)) {
                    visited.Add(pair.Key);
                    DFSAdd(startValue, pair.Value, pair.Key, visited, neighborDiminish, iteration + 1, maxIterations);
                }
            }
        }
    }

    // (x - c)(x + c)/d, x is current, c is max, d is height
    // Consider using exp here for point-based data (positions?)
    float GetValue(int maxIterations, int currentIteration, float startValue) {
        return (currentIteration - maxIterations) * (currentIteration + maxIterations) / startValue;
    }
}
