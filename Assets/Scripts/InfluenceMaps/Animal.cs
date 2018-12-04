using System.Collections;
using System.Linq;
using UnityEngine;

public class Animal : AIMovement {

    public SightScript sight;
    public PreferenceMap preferenceMap;

    private SourceType type;

    [SerializeField]
    private float cohesionFactor = 1;
    [SerializeField]
    private float alignmentFactor = 1;
    [SerializeField]
    private float separationFactor = 1;

    protected override void Start() {
        base.Start();
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

        if (sight != null) {
            sight.EnterSight += EnterSight;
            sight.ExitSight += ExitSight;
        } else {
            Debug.Log("Sight script missing!");
        }
    }

    public void EnterSight(GameObject gameObject) {
        foreach (InfluenceSource source in gameObject.GetComponentsFromParentInChildren<InfluenceSource>()) {
            if (source.parentMap.isPresenceMap && source.type != type) {
                var wMap = new AIMovement.WeightedMap();
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
    }

    public SourceType GetSourceType() {
        return type;
    }

    public Vector3 Flocking(float dt) {
        var position = transform.position;
        var speed = GetComponent<AIMovement>().GetVelocity();
        var avoid = Vector3.zero;
        var count = sight.GetSeen().Count;
        foreach (var boid in sight.GetSeen()) {
            var animal = boid.GetComponentFromParentInChildren<Animal>();
            if (animal == null || animal.type != type)
                continue;
            position += boid.transform.position;
            speed += animal.GetComponent<AIMovement>().GetVelocity();
            avoid += boid.transform.position - transform.position;
        }
        if (count != 0) {
            position /= count;
            speed /= count;
            avoid /= count;

            var dir = (position - transform.position) * cohesionFactor + speed * alignmentFactor + avoid * separationFactor;
            return transform.position + dir * dt;
        }
        return Vector3.zero;
    }
}
