using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PreferenceMap", menuName = "Animals/Preference Map", order = 1)]
public class PreferenceMap : ScriptableObject {

    public List<WeightedType> weightedTypes;

    public float GetWeight(SourceType source) {
        return weightedTypes.Where(s => s.source == source).Select(s => s.weight).FirstOrDefault();
    }
}

[Serializable]
public class WeightedType {
    public SourceType source;
    public float weight;
}