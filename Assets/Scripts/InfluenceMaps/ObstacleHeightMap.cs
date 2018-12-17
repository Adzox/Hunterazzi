using UnityEngine;

public class ObstacleHeightMap : MonoBehaviour {

    public SharedGrid grid;
    public float visualizedMaxHeight;
    private float[,] heightObstacleMap;

    void Start() {
        if (grid == null)
            Debug.LogError("Missing SharedGrid instance!");
        heightObstacleMap = new float[grid.GetWidth(), grid.GetHeight()];

        foreach (Transform child in transform) {
            foreach (var gridPos in grid.ProjectGridPos(child.GetComponent<Renderer>().bounds)) {
                heightObstacleMap[gridPos.x, gridPos.y] = child.GetComponent<Renderer>().bounds.size.y;
            }
        }
    }

    private void OnValidate() {
        visualizedMaxHeight = Mathf.Clamp(visualizedMaxHeight, 0, float.MaxValue);
    }

    private void OnDrawGizmosSelected() {
        if (grid == null)
            return;
        var baseColor = Color.white;
        if (heightObstacleMap != null) {
            grid.ForEachCell((x, y) => {
                if (heightObstacleMap[x, y] > 0) {
                    var c = heightObstacleMap[x, y] / visualizedMaxHeight;
                    Gizmos.color = Color.Lerp(new Color(c, c, c), baseColor, 0.5f);
                    Gizmos.DrawWireSphere(grid.GridToWorld(new Vector2Int(x, y)), 0.031f);
                } 
            });
        }
    }

    public void AddHeight(int x, int y, float height) {
        if (grid.InBounds(x, y)) {
            heightObstacleMap[x, y] = heightObstacleMap[x, y] + height < 0 ? 
                                        heightObstacleMap[x, y] = 0 : 
                                        heightObstacleMap[x, y] + height;  
        }
    }

    public void RemoveHeight(int x, int y, float height) {
        if (grid.InBounds(x, y)) {
            heightObstacleMap[x, y] = heightObstacleMap[x, y] - height < 0 ?
                                        heightObstacleMap[x, y] = 0 :
                                        heightObstacleMap[x, y] - height;
        }
    }

    public float GetHeight(int x, int y) {
        if (grid.InBounds(x, y))
            return heightObstacleMap[x, y];
        return 0;
    }

    public float GetHeight(Vector2Int pos) {
        if (grid.InBounds(pos))
            return heightObstacleMap[pos.x, pos.y];
        return 0;
    }
}
