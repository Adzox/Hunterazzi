using UnityEngine;

public class DirectionSource : SimpleSource {

    public DirectionScript directionScript;

    public new Vector3 sourceDirection {
        get {
            return directionScript.direction;
        }
    }
}
