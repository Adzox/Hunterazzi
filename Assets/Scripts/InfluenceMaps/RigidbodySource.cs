using UnityEngine;

public class RigidbodySource : SimpleSource {

    public Rigidbody velocityBearer;
    
    public new Vector3 sourceDirection { get {
            if (velocityBearer != null)
                return velocityBearer.velocity;
            else
                return Vector3.zero;
        } }
}
