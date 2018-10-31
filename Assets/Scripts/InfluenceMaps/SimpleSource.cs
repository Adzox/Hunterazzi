using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSource : InfluenceSource {

    public override float GetValue(float distance, float sourceValue, float maxDistance) {
        return (1 / (maxDistance * maxDistance)) * sourceValue * (distance - maxDistance) * (distance - maxDistance);
        //return Mathf.Exp(-0.5f*distance + Mathf.Log(sourceValue));
        //return -(distance - maxDistance) * (distance + maxDistance) / (maxDistance * maxDistance / sourceValue);
    }
}
