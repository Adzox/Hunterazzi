using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent()]
public class DetectorScript : MonoBehaviour {

    public delegate void OnDetect(GameObject gameObject);

    [Header("Detector Settings")]
    public List<string> detectTags;
    public LayerMask detectLayers;
    protected List<GameObject> detected;
    public OnDetect OnDetectEnter;
    public OnDetect OnDetectStay;
    public OnDetect OnDetectExit;
    public GameObject ignoreObject;

    protected new Collider collider;

    protected virtual void Start () {
        detected = new List<GameObject>();
        collider = GetComponent<Collider>();
    }

    public Vector3 GetDetectionSize() {
        return collider.bounds.size;
    }

    public ReadOnlyCollection<GameObject> GetDetected() {
        return detected.AsReadOnly();
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject == ignoreObject)
            return;
        if (detectLayers.Contains(other.gameObject.layer)) {
            if (detectTags.Count == 0 || (detectTags.Count > 0 && detectTags.Contains(other.gameObject.tag))) {
                detected.Add(other.gameObject);
                if (OnDetectEnter != null)
                    OnDetectEnter.Invoke(other.gameObject);
            }    
        }
    }

    private void OnTriggerStay(Collider other) {
        if (other.gameObject == ignoreObject)
            return;
        if (detectLayers.Contains(other.gameObject.layer)) {
            if (detectTags.Count == 0 || (detectTags.Count > 0 && detectTags.Contains(other.gameObject.tag))) {
                var pos = other.transform.parent != null ? other.transform.parent.position : other.transform.position;
                Debug.DrawLine(transform.parent.position, pos, Color.magenta);
                if (OnDetectStay != null)
                    OnDetectStay.Invoke(other.gameObject);
            }
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.gameObject == ignoreObject)
            return;
        if (detectLayers.Contains(other.gameObject.layer)) {
            if (detectTags.Count == 0 || (detectTags.Count > 0 && detectTags.Contains(other.gameObject.tag))) {
                detected.Remove(other.gameObject);
                if (OnDetectExit != null)
                    OnDetectExit.Invoke(other.gameObject);
            }
        }
    }
}
