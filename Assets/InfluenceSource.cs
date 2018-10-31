using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class InfluenceSource : MonoBehaviour {

    public float sourceValue;
    public int range;
    public InfluenceMap parentMap;
    public Vector3 sourceDirection { get; protected set; }

    public abstract float GetValue(float distance, float sourceValue, float maxDistance);
}
