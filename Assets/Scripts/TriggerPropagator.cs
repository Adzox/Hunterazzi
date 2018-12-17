using UnityEngine;

public class TriggerPropagator : MonoBehaviour {

    public ObjectRoot objectRoot;

    // Message arrival - resend to object parent!

    // objectRoot.SendMessage("OnTriggerEnter", collision);

    private void OnTriggerEnter(Collider other) {
        objectRoot.onTriggerEnter.Invoke(other);
    }

    private void OnTriggerStay(Collider other) {
        objectRoot.onTriggerStay.Invoke(other);
    }

    private void OnTriggerExit(Collider other) {
        objectRoot.onTriggerExit.Invoke(other);
    }
}
