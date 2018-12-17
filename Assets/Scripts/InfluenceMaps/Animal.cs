using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Animal : AIMovement {

    [Header("Animal")]

    public DetectorScript detection;
    public PreferenceMap preferenceMap;

    private SourceType type;

    [Header("Flocking")]

    [SerializeField]
    private float flockingInfluence = 3;

    [SerializeField]
    private float cohesionFactor = 1;
    [SerializeField]
    private float alignmentFactor = 1;
    [SerializeField]
    [Range(0.01f, 1.0f)]
    private float alignmentDistancePercentageOfRadius = 1;
    [SerializeField]
    private float separationFactor = 1;

    private bool flocking = false;
    private Vector3? flockPoint;
    private float detectionRadius;

    protected override void Start() {
        bool first = true;
        foreach (var source in GetComponentsInChildren<InfluenceSource>()) {
            if (first) {
                type = source.type;
                first = false;
            } else if (type != source.type) {
                Debug.LogError("Mismatching source types in/on '" + gameObject.name + "'! Make sure they are all the same!");
            }
        }

        if (preferenceMap == null)
            Debug.Log("Preference Map is missing!");

        foreach (var type in preferenceMap.weightedTypes.Select(wt => wt.source)) {
            foreach (var im in gameObject.FindComponentsWithTag<InfluenceMap>(type.GetMapTag())) {
                if (!im.isPresenceMap) {
                    var wm = new AIMovement.WeightedMap();
                    wm.weight = preferenceMap.GetWeight(type);
                    wm.map = im;
                    maps.Add(wm);
                }
            }
        }

        if (detection != null) {
            detectionRadius = detection.GetDetectionSize().x;
            detection.OnDetectEnter += EnterSight;
            detection.OnDetectExit += ExitSight;
        } else {
            Debug.Log("Sight script missing!");
        }

        currentPath = new List<Vector2Int>();
        StartCoroutine(FindFlockingPath());
    }

    protected override void Update() {
        flockPoint = Flocking();
        base.Update();
    }

    private void OnDrawGizmosSelected() {
        if (flockPoint.HasValue) {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(flockPoint.Value, 0.1f);
        }
    }

    public void EnterSight(GameObject gameObject) {
        foreach (InfluenceSource source in gameObject.GetComponentsFromParentInChildren<InfluenceSource>()) {
            if (source.parentMap.isPresenceMap && source.type != type) {
                var wMap = new WeightedMap();
                wMap.map = source.parentMap;
                wMap.weight = preferenceMap.GetWeight(source.type);
                maps.Add(wMap);
            }
        }
    }

    public void ExitSight(GameObject gameObject) {
        foreach (InfluenceSource source in gameObject.GetComponentsFromParentInChildren<InfluenceSource>()) {
            if (source.parentMap.isPresenceMap && source.type != type) {
                maps.RemoveAll(wMap => wMap.map == source.parentMap && wMap.weight == preferenceMap.GetWeight(source.type));
            }
        }
        if (detection.GetDetected().Count == 0)
            flocking = false;
    }

    public SourceType GetSourceType() {
        return type;
    }

    public Vector3? Flocking(float lookaheadTime = 1) {
        var avgPos = Vector3.zero;
        var avgSpd = Vector3.zero;
        var avoid = Vector3.zero;
        var count = detection.GetDetected().Count;
        foreach (var boid in detection.GetDetected()) {
            var animal = boid.GetComponentFromParentInChildren<Animal>();
            if (animal == null || animal.type != type)
                continue;
            avgPos += boid.transform.position;
            avgSpd += GetComponent<AIMovement>().GetVelocity() - animal.GetComponent<AIMovement>().GetVelocity();
            avoid += CalculateAvoidance(boid.transform.position);
        }
        if (count != 0) {
            avgPos /= count;
            avgSpd /= count;
            avoid /= count;

            var speed = (avgPos - transform.position) * cohesionFactor + avgSpd * alignmentFactor + avoid * separationFactor;
            // A vision!
            return transform.position + speed * lookaheadTime;
        }
        return null;
    }

    public Vector3 CalculateAvoidance(Vector3 otherPos) {
        if ((otherPos - transform.position).magnitude <= detectionRadius * alignmentDistancePercentageOfRadius) {
            float invMagnitude = (detectionRadius - (otherPos - transform.position).magnitude);

            return (transform.position - otherPos).normalized * invMagnitude;
        }
        return Vector3.zero;
    }

    IEnumerator FindFlockingPath() {
        float updateTime = 1 / maps[0].map.updateFrequency;
        var path = new List<Vector2Int>();
        while (true) {
            currentPos = grid.WorldToGrid(transform.position);
            //var pos = Flocking(); // Use updateTime to scale next position? Use current-time calculation?
            if (flockPoint.HasValue) {
                var gridPos = grid.WorldToGrid(flockPoint.Value);
                path = InfluenceMapNavigation.FindMax(maps, currentPos, searchDistance, gridPos, flockingInfluence);
                if (path.Count > 0 && path[path.Count - 1] == gridPos) {
                    flocking = true;
                } else {
                    flocking = false;
                }
            } else {
                path = InfluenceMapNavigation.FindMax(maps, currentPos, searchDistance);
                flocking = false;
            }
            newPath = path;
            yield return new WaitForSeconds(updateTime);
        }
    }
}
