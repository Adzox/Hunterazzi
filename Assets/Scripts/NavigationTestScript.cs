using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationTestScript : MonoBehaviour {

    public InfluenceMap map;
    public int searchDistance = 10;
	
	void Update () {
        var path = InfluenceMapNavigation.FindMax(map, new Vector2Int(0, 0), searchDistance);
        foreach (var v in path) {
            Debug.Log(v);
        }
	}
}
