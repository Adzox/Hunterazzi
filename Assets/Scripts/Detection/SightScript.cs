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
        float length = (other.transform.position - transform.parent.position).magnitude;
        Ray ray = new Ray(transform.parent.position + (other.transform.position - transform.parent.position).normalized * rayOffset, other.transform.position - transform.parent.position);
        RaycastHit hitInfo;
        if (seen.Contains(other)) {
            if (!InVision(other.transform) || !Physics.Raycast(ray, out hitInfo, length, raycastMask.value) || hitInfo.collider.gameObject != other) {
                if (ExitSight != null)
                    ExitSight.Invoke(other);
                seen.Remove(other);
            } else {
                Debug.DrawLine(transform.parent.position, other.transform.position, Color.yellow);
                if (StaySight != null)
                    StaySight.Invoke(other);
            }
            return;
        } else if ((other.transform.position - transform.position).magnitude <= sensePresenceRadius && Physics.Raycast(ray, out hitInfo, length, raycastMask.value) && hitInfo.collider.gameObject == other) {
            seen.Add(other);
            if (EnterSight != null)
                EnterSight.Invoke(other);
        } else if (InVision(other.transform) && Physics.Raycast(ray, out hitInfo, length, raycastMask.value) && hitInfo.collider.gameObject == other) {
            seen.Add(other);
            if (EnterSight != null)
                EnterSight.Invoke(other);
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

    bool InVision(Transform other) {
        Vector3 forwardDirection = transform.parent.forward * rayOffset;
        Vector3 toOther = other.position - (transform.parent.position + forwardDirection);
        return Vector3.Angle(forwardDirection, toOther) < fieldOfView / 2;
    }
}
