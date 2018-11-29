using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnEat : MonoBehaviour {

    private void OnTriggerEnter(Collider other) {
        var animal = other.GetComponentInParent<Animal>();
        if (animal != null && animal.GetSourceType() == SourceType.Rabbit) {
            // Fill hunger, play sound, ...
            Destroy(gameObject.transform.parent.gameObject);
        }
    }
}
