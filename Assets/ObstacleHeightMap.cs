using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleHeightMap : MonoBehaviour {

    public float cellSize;
    public Vector2 dimensions;
    private Vector3 origo;

    int width;
    int height;
    private float[,] heightObstacleMap;

    void Start() {
        origo = transform.position - new Vector3(dimensions.x * transform.lossyScale.x, 0, dimensions.y * transform.lossyScale.z) / 2;
        width = Mathf.FloorToInt(dimensions.x / cellSize);
        height = Mathf.FloorToInt(dimensions.y / cellSize);
        heightObstacleMap = new float[width, height];

        foreach (Transform child in transform) {
            var bounds = child.GetComponent<Renderer>().bounds;
            // Clamp bounds x and z value to match grid.
            var min = bounds.min;
            min.x = Mathf.FloorToInt(min.x / cellSize) * cellSize;
            min.z = Mathf.FloorToInt(min.z / cellSize) * cellSize;
            var max = bounds.max;
            max.x = Mathf.FloorToInt(max.x / cellSize) * cellSize;
            max.z = Mathf.FloorToInt(max.z / cellSize) * cellSize;
            bounds.SetMinMax(min, max);

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    if (bounds.Contains(GridToWorld(x, y)) && bounds.Contains(GridToWorld(x + 1, y + 1))) {
                        heightObstacleMap[x, y] = bounds.size.y;
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected() {
        origo = transform.position - new Vector3(dimensions.x * transform.lossyScale.x, 0, dimensions.y * transform.lossyScale.z) / 2;

        width = Mathf.FloorToInt(dimensions.x / cellSize);
        height = Mathf.FloorToInt(dimensions.y / cellSize);

        Gizmos.color = Color.black;
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Gizmos.DrawSphere(GridToWorld(new Vector2Int(x, y)), 0.03f);
            }
        }

        Gizmos.color = Color.cyan;
        if (heightObstacleMap != null) {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    if (heightObstacleMap[x, y] >= 0.1f)
                        Gizmos.DrawWireSphere(GridToWorld(new Vector2Int(x, y)), 0.031f);
                }
            }
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(origo, 0.1f);
    }

    public float GetHeight(int x, int y) {
        return heightObstacleMap[x, y];
    }

    public float GetHeight(Vector2Int pos) {
        return heightObstacleMap[pos.x, pos.y];
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

    public Vector3 GridToWorld(int x, int y) {
        return new Vector3(x * cellSize, 0, y * cellSize) + origo;
    }
}
