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

	void Start ()
    {
        var collider = GetComponent<SphereCollider>();
        radius = collider.radius;
	}
	
	void Update ()
    {
        if(AppController.Instance.isPinchStart && !AppController.Instance.isPinchHandled)
        {
            var pinchPoint = AppController.Instance.PinchPoint.Value;
            var localPoint = transform.InverseTransformPoint(pinchPoint);
            if (localPoint.magnitude <= radius)
            {
                isGrabbed = true;
                AppController.Instance.isPinchHandled = true;
                proxyObject = new GameObject("proxy object");
                proxyObject.transform.position = pinchPoint;
                originalParent = transform.parent;
                transform.SetParent(proxyObject.transform, true);
            }
        }

        if(isGrabbed)
        {
            proxyObject.transform.position = AppController.Instance.PinchPoint.Value;
        }

        if(AppController.Instance.isPinchEnd && isGrabbed)
        {
            transform.SetParent(originalParent, true);
            originalParent = null;
            Destroy(proxyObject);
            proxyObject = null;
        }
	}
}
