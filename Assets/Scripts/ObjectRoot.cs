using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class CollisionEvent : UnityEvent<Collision> { }

[Serializable]
public class TriggerEvent : UnityEvent<Collider> { }

public class ObjectRoot : MonoBehaviour {

    [Header("Object Root Events")]
    public CollisionEvent onCollisionEnter;
    public CollisionEvent onCollisionStay;
    public CollisionEvent onCollisionExit;
    public TriggerEvent onTriggerEnter;
    public TriggerEvent onTriggerStay;
    public TriggerEvent onTriggerExit;
}
