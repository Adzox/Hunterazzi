using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfluenceMapNavigation {

    /// <summary>
    /// Returns a path from start to the cell with the highest influence
    /// (excluding start, including end). The algorithm considers all cells
    /// with influence lower than minInfluence to be obstacles. 
    /// </summary>
    public static List<Vector2Int> FindMax(InfluenceMap map, Vector2Int start, int searchDist,
                                    float minInfluence = 0f) {
        var frontier = new Queue<Vector2Int>() { start };
        var discovered = new HashSet<Vector2Int>() { start };
        var distanceTo = new Dictionary<Vector2Int, float>() { { start, 0 } };

        var prev = new Dictionary<Vector2Int, Vector2Int>();
        var res = new List<Vector2Int>();

        Vector2Int best = start;

        while (frontier.Count != 0) {
            var pos = frontier.Dequeue();
            if (map.GetInfluence(pos.x, pos.y) > map.GetInfluence(best.x, best.y))
                best = pos;

            float currentDistance = distanceTo[pos];

            if (searchDist > currentDistance) {
                foreach (var n in SharedGrid.GetNeighbors8(pos)) {
                    if (map.InBounds(n)) {
                        float distToN = currentDistance + Vector2Int.Distance(pos, n);
                        if (map.GetInfluence(n.x, n.y) >= minInfluence &&
                            Mathf.Approximately(map.obstacleHeights.GetHeight(n), 0) &&
                            ((distanceTo.ContainsKey(n) && distToN < distanceTo[n]) || !discovered.Contains(n))) {

                            discovered.Add(n);
                            frontier.Enqueue(n);
                            distanceTo[n] = distToN;
                            prev[n] = pos;
                        }

                    }
                }
            }
        }

        // Reconstruct path
        var c = best;
        while (prev.ContainsKey(c)) {
            res.Add(c);
            c = prev[c];
        }
        res.Reverse();
        return res;
    }

}
