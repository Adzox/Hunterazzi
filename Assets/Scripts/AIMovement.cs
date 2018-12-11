using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour {

    public List<WeightedMap> maps;
    protected List<Vector2Int> newPath;
    protected List<Vector2Int> currentPath;
    public Vector2Int currentPos { get; protected set; }
    public Vector2Int nextPos { get; private set; }
    public SharedGrid grid;
    public ObstacleHeightMap obstacleHeightMap; 
    public Transform modelTransform;

    public float speed;
    public float searchDistance;
    public float minRandomWaitTime = 1;
    public float maxRandomWaitTime = 5;
    public float maxRandomWalkDistance = 1;
    [Range(0, 1)]
    public float randomWalkAnyway = 0.1f;

    bool isRandomWalking;
    bool waiting;
    bool forceRandom;

	protected virtual void Start () {
        currentPath = new List<Vector2Int>();
        StartCoroutine(FindPath());
	}

    private void OnDrawGizmos() {
        if (grid != null) {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireCube(transform.position, new Vector3(grid.GetCellSize() * searchDistance, 0.1f, grid.GetCellSize() * searchDistance));

            if (currentPath != null) {
                Gizmos.color = Color.magenta;
                Vector2Int prev = new Vector2Int(-1, -1);
                foreach (var p in currentPath) {
                    Gizmos.DrawWireSphere(grid.GridToCenterWorld(p), 0.3f);
                    if (prev.x != -1 && prev.y != -1) {
                        Gizmos.DrawLine(grid.GridToCenterWorld(prev), grid.GridToCenterWorld(p));
                    }
                    prev = p;
                }

                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(grid.GridToCenterWorld(currentPos), 0.2f);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(grid.GridToCenterWorld(nextPos), 0.1f);

                Gizmos.DrawLine(grid.GridToCenterWorld(currentPos), grid.GridToCenterWorld(nextPos));
            }
        }
    }

    protected virtual void Update() {
        // Waiting and no new path to use instead?
        bool shouldRandomWalk = Random.value < randomWalkAnyway;
        
        // Waiting if waiting, have no new path or should ignore new path
        if (waiting && newPath.Count == 0) {
            Debug.Log("Waiting...");
            return;
        } else if (forceRandom && waiting) {
            Debug.Log("Force Waiting...");
            return;
        }

        if (shouldRandomWalk && !isRandomWalking) {
            // Random walk anyways!
            currentPath.Clear();
            currentPath.Add(RandomWalk());
            isRandomWalking = true;
            forceRandom = true;
            Debug.Log("Force Random");
        } else if (newPath.Count > 0 && !shouldRandomWalk && !forceRandom) {
            // If a new path exists!
            currentPath.Clear();
            currentPath.AddRange(newPath);
            newPath.Clear();
            StopCoroutine("WaitForRandomWalk");
            waiting = false;
            isRandomWalking = false;
            Debug.Log("New Path");
        } else if (currentPath.Count == 0 && !isRandomWalking && !forceRandom) {
            // Random walk, set path
            currentPath.Clear();
            currentPath.Add(RandomWalk());
            isRandomWalking = true;
            Debug.Log("Random");
        }

        // If still no path after this, do nothing!
        if (currentPath.Count == 0) {
            Debug.Log("No path");
            return;
        }

        nextPos = currentPath[0];
        Move(nextPos);
        if (currentPos == nextPos) {
            currentPath.RemoveAt(0);
            if (isRandomWalking) { // Random walking always sets 1 position into currentPath, so whole path cleared if here!
                waiting = true;
                StopCoroutine("WaitForRandomWalk");
                StartCoroutine("WaitForRandomWalk");
            }
        }
    }
    
    protected Vector2Int RandomWalk() {
        Vector2Int randomPos = new Vector2Int(0, 0);
        // Loop until a traversable straight line is found
        // It must not pass an obstacle and it must not be out of bounds.
        // This might cause problems if an agent is surrounded by obstacles, for instance
        // In that case: do something more intelligent.

        while (true) {
            Vector2 randomWalk = Random.insideUnitCircle * Random.Range(0, maxRandomWalkDistance);
            randomPos = grid.WorldToGrid(transform.position + new Vector3(randomWalk.x, 0, randomWalk.y));
            // Ignore out of bound positions
            if (!grid.InBounds(randomPos)) {
                continue;
            }

            // Ignore obstacles
            RaycastHit hit;        
            if (Physics.Raycast(transform.position, new Vector3(randomWalk.x, 0, randomWalk.y), out hit, randomWalk.magnitude, LayerMask.GetMask("Obstacles"))) {
                continue;
            } else if (obstacleHeightMap.GetHeight(randomPos.x, randomPos.y) > 0) {
                continue;
            }
            break;
        }
        return randomPos;
    }

    protected void Move(Vector2Int targetPos) {
        Vector3 newPos = Vector3.MoveTowards(transform.position, grid.GridToCenterWorld(targetPos), speed * Time.deltaTime);
        newPos.y = transform.position.y;
        transform.LookAt(newPos, Vector3.up);
        transform.position = newPos;
    }

    [System.Serializable]
    public class WeightedMap {
        public InfluenceMap map;
        public float weight;
	}

    public Vector3 GetVelocity() {
        return (transform.position + grid.GridToWorld(nextPos)).normalized * speed;
    }

    protected IEnumerator FindPath() {
        while (true) {
            currentPos = grid.WorldToGrid(transform.position);
            var maxPath = InfluenceMapNavigation.FindMax(maps, currentPos, searchDistance);
            if (maxPath != null && maxPath.Count > 0) {
                newPath = maxPath;
            }
            yield return new WaitForSeconds(1 / maps[0].map.updateFrequency);
        }
    }

    protected IEnumerator WaitForRandomWalk() {
        Debug.Log("Start Wait");
        yield return new WaitForSeconds(Random.Range(minRandomWaitTime, maxRandomWaitTime));
        isRandomWalking = false;
        waiting = false;
        forceRandom = false;
        Debug.Log("End Wait");
    }
}
