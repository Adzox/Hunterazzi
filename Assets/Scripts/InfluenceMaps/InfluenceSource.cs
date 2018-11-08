using UnityEngine;

public abstract class InfluenceSource : MonoBehaviour {

    public float sourceValue;
    public int range;
    public InfluenceMap parentMap;
    public Vector3 sourceDirection { get; protected set; }

    protected virtual void Start() {
        if (parentMap != null)
            parentMap.AddInfluenceSource(this);
        else {
            Debug.LogWarning("No Influence map for influence source, source is destroyed!");
            Destroy(this);
        }
    }

    protected virtual void OnEnable() {
        if (parentMap != null)
            parentMap.AddInfluenceSource(this);
    }

    protected virtual void OnDisable() {
        if (parentMap != null)
            parentMap.RemoveInfluenceSource(this);
    }

    protected virtual void OnDestroy() {
        if (parentMap != null)
            parentMap.RemoveInfluenceSource(this);
    }

    public abstract float GetValue(float distance, float sourceValue, float maxDistance);
}
