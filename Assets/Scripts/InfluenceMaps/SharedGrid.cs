using System;
using System.Collections.Generic;
using UnityEngine;

public class SharedGrid : MonoBehaviour {

    [SerializeField]
    private float cellSize;
    [SerializeField]
    private Vector2 dimensions;
    private Vector3 origo;

    int width;
    int height;

    void Start() {
        origo = transform.position - new Vector3(dimensions.x * transform.lossyScale.x, 0, dimensions.y * transform.lossyScale.z) / 2;
        width = Mathf.FloorToInt(dimensions.x * transform.lossyScale.x / cellSize);
        height = Mathf.FloorToInt(dimensions.y * transform.lossyScale.z / cellSize);
    }

    private void OnDrawGizmosSelected() {
        origo = transform.position - new Vector3(dimensions.x * transform.lossyScale.x, 0, dimensions.y * transform.lossyScale.z) / 2;

        width = Mathf.FloorToInt(dimensions.x * transform.lossyScale.x / cellSize);
        height = Mathf.FloorToInt(dimensions.y * transform.lossyScale.x / cellSize);

        Gizmos.color = Color.black;
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Gizmos.DrawSphere(GridToWorld(new Vector2Int(x, y)), 0.03f);
            }
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(origo, 0.06f);
    }

    public bool InBounds(int x, int y) {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public bool InBounds(Vector2Int pos) {
        return InBounds(pos.x, pos.y);
    }

    public Vector2Int WorldToGrid(Vector3 point) {
        var pos = point - origo;
        int x = Mathf.FloorToInt(pos.x / cellSize);
        int y = Mathf.FloorToInt(pos.z / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(int x, int y) {
        return new Vector3(x * cellSize, 0, y * cellSize) + origo;
    }

    public Vector3 GridToWorld(Vector2Int pos) {
        return GridToWorld(pos.x, pos.y);
    }

    public Vector3 GridToCenterWorld(int x, int y) {
        return new Vector3(x * cellSize + cellSize / 2, 0, y * cellSize + cellSize / 2) + origo;
    }

    public Vector3 GridToCenterWorld(Vector2Int pos) {
        return GridToCenterWorld(pos.x, pos.y);
    }

    public float GetCellSize() {
        return cellSize;
    }

    public int GetWidth() {
        return width;
    }

    public int GetHeight() {
        return height;
    }

    public void ForEachYRow(Action<int> action) {
        for (int y = 0; y < height; y++) {
            action.Invoke(y);
        }
    }

    public void ForEachXColumn(Action<int> action) {
        for (int x = 0; x < width; x++) {
            action.Invoke(x);
        }
    }

    public void ForEachCell(Action<int, int> action) {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                action.Invoke(x, y);
            }
        }
    }

    public Bounds Project(Bounds bounds) {
        // Clamp bounds x and z value to match grid.
        var minX = Mathf.FloorToInt(bounds.min.x / cellSize) * cellSize;
        var minZ = Mathf.FloorToInt(bounds.min.z / cellSize) * cellSize;
        var maxX = Mathf.FloorToInt(bounds.max.x / cellSize) * cellSize;
        var maxZ = Mathf.FloorToInt(bounds.max.z / cellSize) * cellSize;
        var newBounds = new Bounds();
        newBounds.SetMinMax(new Vector3(minX, bounds.min.y, minZ), new Vector3(maxX, bounds.max.y, maxZ));
        return newBounds;
    }

    public IEnumerable<Vector2Int> ProjectGridPos(Bounds bounds) {
        Vector2Int min = WorldToGrid(bounds.min);
        Vector2Int max = WorldToGrid(bounds.max);
        for (int x = min.x; x < max.x; x++) {
            for (int y = min.y; y < max.y; y++) {
                if (InBounds(x, y))
                    yield return new Vector2Int(x, y);
            }
        }
    }

    public static IEnumerable<Vector2Int> GetNeighbors8(Vector2Int pos) {
        return new List<Vector2Int>() {
            new Vector2Int(pos.x - 1, pos.y + 1), new Vector2Int(pos.x, pos.y + 1), new Vector2Int(pos.x + 1, pos.y + 1),
            new Vector2Int(pos.x - 1, pos.y),                                       new Vector2Int(pos.x + 1, pos.y),
            new Vector2Int(pos.x - 1, pos.y - 1), new Vector2Int(pos.x, pos.y - 1), new Vector2Int(pos.x + 1, pos.y - 1),
        };
    }

    public static IEnumerable<Vector2Int> GetNeighbors4(Vector2Int pos) {
        return new List<Vector2Int>() {
                                                new Vector2Int(pos.x, pos.y + 1),
            new Vector2Int(pos.x - 1, pos.y),                                       new Vector2Int(pos.x + 1, pos.y),
                                                new Vector2Int(pos.x, pos.y - 1),
        };
    }

    public Dictionary<Vector2Int, int> BFS(int maxIterations, Action<Vector2Int, int> action, Predicate<Vector2Int> validNeighbor, params Vector2Int[] startPositions) {
        return BFS(startPositions, maxIterations, action, validNeighbor);
    }

    public Dictionary<Vector2Int, int> BFS(IEnumerable<Vector2Int> startPositions, int maxIterations, Action<Vector2Int, int> action, Predicate<Vector2Int> validNeighbor) {
        var frontier = new Queue<Vector2Int>();
        var discovered = new HashSet<Vector2Int>();
        var distanceTo = new Dictionary<Vector2Int, int>();
        foreach (Vector2Int pos in startPositions) {
            frontier.Enqueue(pos);
            discovered.Add(pos);
            distanceTo.Add(pos, 0);
        }

        while (frontier.Count != 0) {
            var pos = frontier.Dequeue();
            int iteration = distanceTo[pos];

            if (maxIterations > iteration) {

                action.Invoke(pos, iteration);

                foreach (var neighbor in GetNeighbors8(pos)) {
                    if (InBounds(neighbor) && validNeighbor.Invoke(neighbor) && ((distanceTo.ContainsKey(neighbor) && iteration + 1 < distanceTo[neighbor]) || !discovered.Contains(neighbor))) {
                        discovered.Add(neighbor);
                        frontier.Enqueue(neighbor);
                        distanceTo[neighbor] = iteration + 1;
                    }
                }
            }
        }
        return distanceTo;
    }
}
