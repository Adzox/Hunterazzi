using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class Animal : MonoBehaviour {

    public SightScript sight;
    public AIMovement movement;
    public PreferenceMap preferenceMap;

    private SourceType? type;

    void Start() {
        foreach (var source in GetComponentsInChildren<InfluenceSource>()) {
            if (type == null) {
                type = source.type;
            } else if (type != source.type) {
                Debug.LogError("Mismatching source types in/on '" + gameObject.name + "'! Make sure they are all the same!");
            }
        }

        if (movement == null)
            Debug.Log("Movement script is missing!");
        if (sight != null) {
            sight.EnterSight += EnterSight;
            sight.ExitSight += ExitSight;
        } else {
            Debug.Log("Sight script missing!");
        }
        if (preferenceMap == null)
            Debug.Log("Preference Map is missing!");
    }

    public void EnterSight(GameObject gameObject) {
        foreach (InfluenceSource source in gameObject.GetComponentsInChildren<InfluenceSource>()) {
            if (source.parentMap.isPresenceMap) {
                var wMap = new AIMovement.WeightedMap();
                wMap.map = source.parentMap;
                wMap.weight = preferenceMap.GetWeight(source.type);
                movement.maps.Add(wMap);
            }
        }
    }

    public void ExitSight(GameObject gameObject) {
        foreach (InfluenceSource source in gameObject.GetComponentsInChildren<InfluenceSource>()) {
            if (source.parentMap.isPresenceMap) {
                movement.maps.RemoveAll(wMap => wMap.map == source.parentMap && wMap.weight == preferenceMap.GetWeight(source.type));
            }
        }
    }

    public SourceType? GetSourceType() {
        return type;
    }
}
