using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour {

    public InfluenceMap map;
    List<Vector2Int> path;
    Vector2Int currentPos;
    Vector2Int nextPos;

    public float speed;

	void Start () {
        path = new List<Vector2Int>();
		
	}
	
	void Update () {
        currentPos = map.grid.WorldToGrid(transform.position);
        path = InfluenceMapNavigation.FindMax(map, currentPos, 10, 0);
        if (path.Count > 0) {
            nextPos = path[0];
            if (currentPos == nextPos) path.Remove(path[0]);
        } else {
            nextPos = currentPos;
        }
        transform.position = Vector3.MoveTowards(transform.position, map.grid.GridToWorld(nextPos), speed * Time.deltaTime);
		
	}
}
