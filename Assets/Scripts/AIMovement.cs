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

    bool isRandomWalking;
    bool isWaiting;

	void Start () {
        path = new List<Vector2Int>();
        StartCoroutine(FindPath());
	}

    void Update() {
        if (path.Count > 0) {
            nextPos = path[0];
            if (currentPos == nextPos) path.Remove(path[0]);
            isRandomWalking = false;
        } else {
            if (!isRandomWalking && !isWaiting) {
                bool foundValidPos = false;
                // Loop until a traversable straight line is found
                // It must not pass an obstacle and it must not be out of bounds.
                // This might cause problems if an agent is surrounded by obstacles, for instance
                // In that case: do something more intelligent.
                while (!foundValidPos) {
                    Vector2 randomWalk = Random.insideUnitCircle * Random.Range(0, 10);
                    nextPos = grid.WorldToGrid(transform.position + new Vector3(randomWalk.x, 0, randomWalk.y));
                    if (!grid.InBounds(nextPos)) {
                        continue;
                    }
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, new Vector3(randomWalk.x, 0, randomWalk.y), out hit, randomWalk.magnitude)) {
                        if (hit.transform.GetComponent<ObstacleHeightMap>() == null) {
                            continue;
                        }
                    }
                    foundValidPos = true;
                    isRandomWalking = true;

                }
            }
        }
        Vector3 newPos = Vector3.MoveTowards(transform.position, grid.GridToCenterWorld(nextPos), speed * Time.deltaTime);
        newPos.y = transform.position.y;
        transform.position = newPos;
        transform.LookAt(newPos, Vector3.up);
        if (isRandomWalking && currentPos == nextPos) {
            isRandomWalking = false;
            isWaiting = true;
            StartCoroutine(WaitForRandomWalk());
        }
    }

    private void OnDrawGizmosSelected() {
        if (grid != null) {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, grid.GetCellSize() * searchDistance);
        }
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

    IEnumerator WaitForRandomWalk() {
        yield return new WaitForSeconds(Random.Range(1, 10));
        isWaiting = false;
    }
}
