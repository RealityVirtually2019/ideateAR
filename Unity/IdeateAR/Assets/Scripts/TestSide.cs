using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSide : MonoBehaviour
{
    public Transform CenterPoint;
	
	void Start ()
    {	
	}
	
	void Update ()
    {
        Vector3 toObject = (transform.position - CenterPoint.position).normalized;
        Vector3 refVector = CenterPoint.forward.normalized;

        Debug.Log(Vector3.Dot(refVector, toObject));
	}
}
