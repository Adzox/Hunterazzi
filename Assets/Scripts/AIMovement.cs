using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour {

    public List<WeightedMap> maps;
    List<Vector2Int> path;
    Vector2Int currentPos;
    Vector2Int nextPos;
    public SharedGrid grid;

    public float speed;
    public float searchDistance;

	void Start () {
        path = new List<Vector2Int>();
        StartCoroutine(FindPath());
	}

    void Update() {
        Debug.Log(path.Count);
        if (path.Count > 0) {
            nextPos = path[0];
            if (currentPos == nextPos) path.Remove(path[0]);
        } else {
            nextPos = currentPos;
        }
        Vector3 newPos = Vector3.MoveTowards(transform.position, grid.GridToCenterWorld(nextPos), speed * Time.deltaTime);
        newPos.y = transform.position.y;
        transform.position = newPos;
        transform.LookAt(newPos, Vector3.up);
    }

    [System.Serializable]
    public class WeightedMap {
        public InfluenceMap map;
        public float weight;
		
	}

    public Vector3 GetVelocity() {
        return (transform.position + grid.GridToWorld(nextPos)).normalized * speed;
    }

    IEnumerator FindPath() {
        while (true) {
            currentPos = grid.WorldToGrid(transform.position);
            path = InfluenceMapNavigation.FindMax(maps, currentPos, searchDistance, 1f);
            yield return new WaitForSeconds(1 / maps[0].map.updateFrequency);
        }
    }
}
