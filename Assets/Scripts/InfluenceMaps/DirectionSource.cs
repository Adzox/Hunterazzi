using UnityEngine;

public class DirectionSource : SimpleSource {

    public DirectionScript directionScript;

    public override Vector3 sourceDirection {
        get {
            return directionScript.direction;
        }
    }
}
