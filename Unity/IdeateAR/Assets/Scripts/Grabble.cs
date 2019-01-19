using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class Grabble : MonoBehaviour
{
    float radius;
    bool isGrabbed;
    GameObject proxyObject;
    Transform originalParent;
    Rigidbody body;

    bool isPointerInRange = false;

	void Start ()
    {
        var collider = GetComponent<SphereCollider>();
        radius = collider.radius;
        body = GetComponent<Rigidbody>();
    }
	
	void Update ()
    {
        if(AppController.Instance.isPinchStart && !AppController.Instance.isPinchHandled)
        {
            var pinchPoint = AppController.Instance.PinchPoint.Value;
            //var localPoint = transform.InverseTransformPoint(pinchPoint);
            //if (localPoint.magnitude <= radius)
            if(isPointerInRange)
            {
                isGrabbed = true;
                AppController.Instance.isPinchHandled = true;
                proxyObject = new GameObject("proxy object");
                proxyObject.transform.position = pinchPoint;
                originalParent = transform.parent;
                transform.SetParent(proxyObject.transform, true);

                if(body != null) body.isKinematic = true;
            }
        }

        if(isGrabbed)
        {
            proxyObject.transform.position = AppController.Instance.PinchPoint.Value;
        }

        if(AppController.Instance.isPinchEnd && isGrabbed)
        {
            if (body != null) body.isKinematic = false;
            transform.SetParent(originalParent, true);
            originalParent = null;
            Destroy(proxyObject);
            proxyObject = null;
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "pointer")
        {
            isPointerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "pointer")
        {
            isPointerInRange = false;
        }
    }
}
