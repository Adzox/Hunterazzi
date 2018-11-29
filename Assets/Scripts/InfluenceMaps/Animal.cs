using System.Linq;
using UnityEngine;

public class Animal : MonoBehaviour {

    public SightScript sight;
    public AIMovement movement;
    public PreferenceMap preferenceMap;

    private SourceType type;

    void Start() {
        bool first = true;
        foreach (var source in GetComponentsInChildren<InfluenceSource>()) {
            if (first) {
                type = source.type;
                first = false;
            } else if (type != source.type) {
                Debug.LogError("Mismatching source types in/on '" + gameObject.name + "'! Make sure they are all the same!");
            }
        }

        if (movement == null)
            Debug.Log("Movement script is missing!");
        if (preferenceMap == null)
            Debug.Log("Preference Map is missing!");

        foreach (var type in preferenceMap.weightedTypes.Select(wt => wt.source)) {
            foreach (var im in gameObject.FindComponentsWithTag<InfluenceMap>(type.GetMapTag())) {
                if (!im.isPresenceMap) {
                    var wm = new AIMovement.WeightedMap();
                    wm.weight = preferenceMap.GetWeight(type);
                    wm.map = im;
                    movement.maps.Add(wm);
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
            if (source.parentMap.isPresenceMap) {
                var wMap = new AIMovement.WeightedMap();
                wMap.map = source.parentMap;
                wMap.weight = preferenceMap.GetWeight(source.type);
                movement.maps.Add(wMap);
            }
        }
    }

    public void ExitSight(GameObject gameObject) {
        foreach (InfluenceSource source in gameObject.GetComponentsFromParentInChildren<InfluenceSource>()) {
            if (source.parentMap.isPresenceMap) {
                movement.maps.RemoveAll(wMap => wMap.map == source.parentMap && wMap.weight == preferenceMap.GetWeight(source.type));
            }
        }
    }

    public SourceType GetSourceType() {
        return type;
    }
}
