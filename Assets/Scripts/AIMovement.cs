using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour {

    public InfluenceMap map;
    public List<WeightedMap> maps;
    List<Vector2Int> path;
    Vector2Int currentPos;
    Vector2Int nextPos;

    public float speed;
    public float searchDistance;

	void Start () {
        path = new List<Vector2Int>();
        map.UpdateMapDelegates += Search;
	}

    void Update() {
        if (path.Count > 0) {
            nextPos = path[0];
            if (currentPos == nextPos) path.Remove(path[0]);
        } else {
            nextPos = currentPos;
        }
        Vector3 newPos = Vector3.MoveTowards(transform.position, map.grid.GridToCenterWorld(nextPos), speed * Time.deltaTime);
        newPos.y = transform.position.y;
        transform.position = newPos;
    }

    void Search() {
        currentPos = map.grid.WorldToGrid(transform.position);
        path = InfluenceMapNavigation.FindMax(map, currentPos, searchDistance, 1f);

    }

    [System.Serializable]
    public class WeightedMap {
        public InfluenceMap map;
        public float weight;
    }
}
