using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PresenceSource : InfluenceSource {

    private void Start() {
        if (parentMap != null)
            parentMap.AddInfluenceSource(this);
        else {
            Debug.LogWarning("No Influence map for influence source, source is destroyed!");
            Destroy(this);
        }
    }

    private void OnEnable() {
        if (parentMap != null)
            parentMap.AddInfluenceSource(this);
    }

    private void OnDisable() {
        if (parentMap != null)
            parentMap.RemoveInfluenceSource(this);
    }

    private void OnDestroy() {
        if (parentMap != null)
            parentMap.RemoveInfluenceSource(this);
    }

    public override float GetValue(float distance, float sourceValue, float maxDistance) {
        return (1 / (maxDistance * maxDistance)) * sourceValue * (distance - maxDistance) * (distance - maxDistance);
        //return Mathf.Exp(-0.5f*distance + Mathf.Log(sourceValue));
        //return -(distance - maxDistance) * (distance + maxDistance) / (maxDistance * maxDistance / sourceValue);
    }
}
