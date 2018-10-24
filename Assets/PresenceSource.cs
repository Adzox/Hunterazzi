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
}
