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

    int width;
    int height;
    private float[,] map;

    // Use this for initialization
    void Start () {
        origo = transform.position - new Vector3(dimensions.x, 0, dimensions.y) / 2;
        width = Mathf.FloorToInt(dimensions.x / cellSize);
        height = Mathf.FloorToInt(dimensions.y / cellSize);
        map = new float[width, height];
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Vector3 origo = transform.position - new Vector3(dimensions.x, 0, dimensions.y) / 2;
        Gizmos.DrawSphere(transform.position - new Vector3(dimensions.x, 0, dimensions.y) / 2, 0.1f);
    }

    IEnumerable<Vector2Int> GetNeighbors(Vector2Int pos) {
        return new List<Vector2Int>() {
            new Vector2Int(pos.x - 1, pos.y + 1), new Vector2Int(pos.x, pos.y + 1), new Vector2Int(pos.x + 1, pos.y + 1),
            new Vector2Int(pos.x - 1, pos.y),                                       new Vector2Int(pos.x + 1, pos.y),
            new Vector2Int(pos.x - 1, pos.y - 1), new Vector2Int(pos.x, pos.y - 1), new Vector2Int(pos.x + 1, pos.y - 1),
        }.Where(p => InBounds(p));
    }

    void SetInfluence(float influence, Vector2Int pos) {
        if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height) {
            map[pos.x, pos.y] = influence;
        }
    }

    void AddInfluence(float influence, Vector2Int pos) {
        if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height) {
            map[pos.x, pos.y] += influence;
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
        if (InBounds(pos)) {
            return map[pos.x, pos.y];
        }
        return 0;
    }
}
