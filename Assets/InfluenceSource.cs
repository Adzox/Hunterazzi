using UnityEngine;
using System.Collections;

public abstract class InfluenceSource : MonoBehaviour {

    public float sourceValue;
    public int range;
    public InfluenceMap parentMap;

    public abstract float GetDecayValue(int maxIter, int currentIter, float startVal);
}
