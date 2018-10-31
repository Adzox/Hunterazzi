using UnityEngine;
using System.Collections;

public class MultiSource : MonoBehaviour {

    public MultiInfluenceMap parentMap;

    [Space()]
    public float presenceValue;
    public int presenceRange;

    [Space()]
    public float smellValue;
    public int smellRange;

    [Space()]
    public float soundValue;
    public int soundRange;

    internal SimpleSource presence;
    internal SimpleSource smell;
    internal SimpleSource sound;

    private void Awake() {
        var sources = new GameObject("Sources");
        sources.transform.SetParent(gameObject.transform);

        presence = sources.AddComponent<SimpleSource>();
        presence.sourceValue = presenceValue;
        presence.range = presenceRange;

        smell = sources.AddComponent<SimpleSource>();
        smell.sourceValue = smellValue;
        smell.range = smellRange;

        sound = sources.AddComponent<SimpleSource>();
        sound.sourceValue = soundValue;
        sound.range = soundRange;
    }

    protected virtual void Start() {
        if (parentMap != null)
            parentMap.AddMultiSource(this);
        else {
            Debug.LogWarning("No Influence map for influence source, source is destroyed!");
            Destroy(this);
        }
    }

    protected virtual void OnEnable() {
        if (parentMap != null)
            parentMap.AddMultiSource(this);
    }

    protected virtual void OnDisable() {
        if (parentMap != null)
            parentMap.RemoveMultiSource(this);
    }

    protected virtual void OnDestroy() {
        if (parentMap != null)
            parentMap.RemoveMultiSource(this);
    }

    public float GetValue(float distance, float sourceValue, float maxDistance) {
        return (1 / (maxDistance * maxDistance)) * sourceValue * (distance - maxDistance) * (distance - maxDistance);
    }
}
