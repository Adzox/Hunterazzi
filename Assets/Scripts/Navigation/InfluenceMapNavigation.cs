using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfluenceMapNavigation {

    public static List<Vector2Int> FindMax(List<AIMovement.WeightedMap> maps, Vector2Int start, float searchDist,
                                           float minInfluence = 0f) {
        return FindMax(maps, start, searchDist, new Vector2Int(-1, -1), 0, minInfluence);
    }

    // More TODO: Take a SharedGrid as argument or extract from one of the maps.
    // Use it for functions.

    public static List<Vector2Int> FindMax(List<AIMovement.WeightedMap> maps, Vector2Int start, float searchDist,
                                           Vector2Int flockPoint, float flockPointInfluence, float minInfluence = 0f) {
        var frontier = new Queue<Vector2Int>() { start };
        var discovered = new HashSet<Vector2Int>() { start };
        var distanceTo = new Dictionary<Vector2Int, float>() { { start, 0 } };

        var prev = new Dictionary<Vector2Int, Vector2Int>();
        var res = new List<Vector2Int>();

        Vector2Int best = start;
        float bestInfluence = 0f;
        if (flockPoint == best) {
            bestInfluence += flockPointInfluence;
        }
        foreach (var map in maps) {
            bestInfluence += map.map.GetInfluence(best.x, best.y) * map.weight;
        }

        while (frontier.Count != 0) {
            var pos = frontier.Dequeue();
            float influence = 0f;
            if (pos == flockPoint) {
                influence += flockPointInfluence;
            }
            foreach (var map in maps) {
                influence += map.map.GetInfluence(pos.x, pos.y) * map.weight;
            }
            // If new best found, store it
            if (influence > bestInfluence) {
                best = pos;
                bestInfluence = influence;
            }

            float currentDistance = distanceTo[pos];
            if (searchDist > currentDistance) {
                bool foundImprovement = false;
                Vector2Int leastBadNode = pos; // This is used if all neighbours have influence lower than minInfluence
                float leastBadInfluence = Mathf.NegativeInfinity;

                foreach (var n in SharedGrid.GetNeighbors4(pos)) {
                    if (maps[0].map.grid.InBounds(n)) {
                        float distToN = currentDistance + Vector2Int.Distance(pos, n);
                        float avgInfluence = 0f;
                        int count = maps.Count;
                        if (n == flockPoint) {
                            avgInfluence += flockPointInfluence;
                            count++;
                        }
                        foreach (var map in maps) {
                            avgInfluence += map.map.GetInfluence(n.x, n.y) * map.weight;
                        }
                        avgInfluence /= count;
                        if (avgInfluence > leastBadInfluence) {
                            leastBadNode = n;
                            leastBadInfluence = avgInfluence;
                        }
                        if (avgInfluence >= minInfluence &&
                            Mathf.Approximately(maps[0].map.obstacleHeights.GetHeight(n), 0) &&
                            ((distanceTo.ContainsKey(n) && distToN < distanceTo[n]) || !discovered.Contains(n))) {

                            discovered.Add(n);
                            frontier.Enqueue(n);
                            distanceTo[n] = distToN;
                            prev[n] = pos;
                            foundImprovement = true;
                        }
                    }
                }
                float distToLBN = currentDistance + Vector2Int.Distance(pos, leastBadNode);
                if (!foundImprovement &&
                    Mathf.Approximately(maps[0].map.obstacleHeights.GetHeight(leastBadNode), 0) &&
                    ((distanceTo.ContainsKey(leastBadNode) && distToLBN < distanceTo[leastBadNode]) || !discovered.Contains(leastBadNode))) {

                    discovered.Add(leastBadNode);
                    frontier.Enqueue(leastBadNode);
                    distanceTo[leastBadNode] = distToLBN;
                    prev[leastBadNode] = pos;
                }
            }
        }

        // Reconstruct path
        var c = best;
        
        if (flockPoint == best) {
            return new List<Vector2Int>() { flockPoint };
        } else {
            while (prev.ContainsKey(c)) {
                res.Add(c);
                c = prev[c];
            }
            res.Reverse();
            return res;
        }
    }

}
