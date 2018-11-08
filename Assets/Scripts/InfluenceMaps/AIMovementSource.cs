using UnityEngine;

public class AIMovementSource : SimpleSource {

    public AIMovement aIMovement;
    
    public new Vector3 sourceDirection { get {
            if (aIMovement != null)
                return aIMovement.GetVelocity();
            else
                return Vector3.zero;
        } }
}
