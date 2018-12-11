using UnityEngine;

public class CollisionPropagator : MonoBehaviour {

    public ObjectRoot objectRoot;

    // Message arrival - resend to object parent!

    // objectRoot.SendMessage("OnCollisionEnter", collision);

    private void OnCollisionEnter(Collision collision) {
        objectRoot.onCollisionEnter.Invoke(collision);
    }

    private void OnCollisionStay(Collision collision) {
        objectRoot.onCollisionStay.Invoke(collision);
    }

    private void OnCollisionExit(Collision collision) {
        objectRoot.onCollisionExit.Invoke(collision);
    }
}
