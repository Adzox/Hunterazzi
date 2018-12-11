using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnEat : MonoBehaviour {

    private void OnTriggerEnter(Collider other) {
        var animal = other.GetComponentInParent<Animal>();
        var thisAnimal = transform.GetComponentInParent<Animal>();
        if (animal != null && animal.GetSourceType() == SourceType.Rabbit) {
            if (transform.tag == "Carrot") {
                Destroy(gameObject.transform.parent.gameObject);
            } 

        } if (animal != null && animal.GetSourceType() == SourceType.Wolf) {
            if (thisAnimal != null && thisAnimal.GetSourceType() == SourceType.Rabbit) {
                Destroy((gameObject.transform.parent.gameObject));
            }
        }
    }
}
