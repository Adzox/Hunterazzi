using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class SightScript : DetectorScript {

    [Header("Sight Settings")]
    [Range(0, 360)]
    public float fieldOfView;
    public LayerMask raycastMask;

    [Tooltip("The offset for the raycasting.")]
    [Range(0, float.MaxValue)]
    public float rayOffset;
    public float rayHeight = 0.5f;

    [Tooltip("The radius in which all is 'seen', no matter angle.")]
    [Range(0, float.MaxValue)]
    public float sensePresenceRadius;

    [Tooltip("The length to draw debug lines. Does not correspond to collider.")]
    public float debugLineLength = 1;

    public delegate void OnSight(GameObject gameObject);

    public OnSight EnterSight;
    public OnSight StaySight;
    public OnSight ExitSight;

    private List<GameObject> seen;
    private DetectorScript detector;

    protected override void Start() {
        base.Start();
        seen = new List<GameObject>();
        OnDetectExit += CheckExited;
        OnDetectStay += CheckSight;
    }

    public ReadOnlyCollection<GameObject> GetSeen() {
        return seen.AsReadOnly();
    }

    void CheckExited(GameObject gameObject) {
        seen.Remove(gameObject);
    }

    void CheckSight(GameObject other) {
        if (other == null)
            return;
        float length = (other.transform.parent.position - transform.parent.position).magnitude;

        var startPos = transform.parent.position + (other.transform.parent.position - transform.parent.position).normalized * rayOffset;
        startPos.y += rayHeight;
        var rayDir = other.transform.parent.position - transform.parent.position;

        Ray ray = new Ray(startPos, rayDir);
        RaycastHit hitInfo;

        if (seen.Contains(other)) {
            if (!InVision(startPos, other.transform) || !Physics.Raycast(ray, out hitInfo, length, raycastMask.value) || hitInfo.collider.gameObject != other) {
                if (ExitSight != null)
                    ExitSight.Invoke(other);
                seen.Remove(other);
            } else {
                Debug.DrawRay(ray.origin, ray.direction * length, Color.yellow);
                //Debug.DrawLine(transform.parent.position, other.transform.parent.position, Color.yellow);
                if (StaySight != null)
                    StaySight.Invoke(other);
            }
            return;
        } else if ((other.transform.position - transform.position).magnitude <= sensePresenceRadius && Physics.Raycast(ray, out hitInfo, length, raycastMask.value) && hitInfo.collider.gameObject == other) {
            seen.Add(other);
            if (EnterSight != null)
                EnterSight.Invoke(other);
        } else if (InVision(startPos, other.transform) && Physics.Raycast(ray, out hitInfo, length, raycastMask.value) && hitInfo.collider.gameObject == other) {
            seen.Add(other);
            if (EnterSight != null)
                EnterSight.Invoke(other);
        } else {
#if false
            if (gameObject.transform.parent.name == "Rabbit")
                Debug.DrawRay(ray.origin, ray.direction * length);
            bool hit = Physics.Raycast(ray, out hitInfo, length, raycastMask.value);
            var msg = gameObject.transform.parent.name + " can't see " + other.transform.parent.name + ":\n";
            msg += "In presence: " + ((other.transform.position - transform.position).magnitude <= sensePresenceRadius) + "\n";
            msg += "In Vision: " + InVision(other.transform) + "\n Raycast hit: " + hit;
            if (hit) {
                msg += ": " + hitInfo.transform.parent.name;
                msg += "\n Hit obj vs expected: " + hitInfo.collider.gameObject.name + ", " + other.name;
            }
            msg += "\n";
            Debug.Log(msg);
#endif
        }
    }

    void OnDrawGizmos() {
        Vector3 center = transform.parent.position + transform.parent.forward * rayOffset;
        center.y = transform.position.y;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(center, Quaternion.Euler(new Vector3(0, fieldOfView / 2, 0)) * transform.parent.forward * debugLineLength);
        Gizmos.DrawRay(center, transform.parent.forward * debugLineLength);
        Gizmos.DrawRay(center, Quaternion.Euler(new Vector3(0, -fieldOfView / 2, 0)) * transform.parent.forward * debugLineLength);
        Gizmos.DrawWireSphere(transform.position, sensePresenceRadius);
    }

    bool InVision(Vector3 startPos, Transform other) {
        Vector3 forwardDirection = transform.parent.forward * rayOffset;
        Vector3 toOther = other.position - startPos;
        return Vector3.Angle(forwardDirection, toOther) < fieldOfView / 2;
    }
}
