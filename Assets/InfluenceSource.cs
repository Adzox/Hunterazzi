using UnityEngine;
using System.Collections;

public abstract class InfluenceSource : MonoBehaviour {

    public float sourceValue;
    public int range;

    protected abstract float GetDecay(float oldValue);
}
