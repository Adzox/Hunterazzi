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

    // (x - c)(x + c)/(c*c/d), x is current, c is max, d is height
    // Consider using exp here for point-based data (positions?)
    // Sound kinda like pos, but bigger stretch (radius)
    // Smell like below
    // Velocity is sudden increase near direct neighbors in direction of velocity, and quick decline from there!
    public override float GetDecayValue(int maxIterations, int currentIteration, float startValue) {
        return -(currentIteration - maxIterations) * (currentIteration + maxIterations) / (maxIterations * maxIterations / startValue);
    }
}
