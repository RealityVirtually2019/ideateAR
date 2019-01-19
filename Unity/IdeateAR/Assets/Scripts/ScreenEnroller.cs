using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;


public class ScreenEnroller : MonoBehaviour
{
    Transform TagPoint;

    MLHand leftMLHand { get { return MLHands.Left; } }
    FHand leftFHand { get { return FHands.Left; } }

    MLHand righttMLHand { get { return MLHands.Right; } }
    FHand rightFHand { get { return FHands.Right; } }

    Vector3[] corners = new Vector3[3];
    int cornerIndex = 0;

    FVector pinchPoint = new FVector();
    bool wasRightPinch;
    GameObject dot;

    List<GameObject> dots = new List<GameObject>();

    void Start ()
    {
        AppController.Instance.SetMode(AppModeEnum.Enrolling);
        TagPoint = GameObject.Find("Tag Point").transform;
        MLInput.OnTriggerUp += MLInput_OnTriggerUp;
        MLInput.OnControllerButtonDown += MLInput_OnControllerButtonDown;
        transform.localScale = Vector3.zero;
	}

    private void MLInput_OnControllerButtonDown(byte arg1, MLInputControllerButton button)
    {
        if(button == MLInputControllerButton.Bumper)
        {
            //We are done
            AppController.Instance.SetMode(AppModeEnum.Normal);
            Dispose(); //Take the enroller off the screen
        }
    }

    private void Dispose()
    {
        clearDots();
        MLInput.OnTriggerUp -= MLInput_OnTriggerUp;
        MLInput.OnControllerButtonDown -= MLInput_OnControllerButtonDown;
        Destroy(this);
    }

    private void MLInput_OnTriggerUp(byte arg1, float arg2)
    {
        if (cornerIndex == 0) clearDots();

        newDot(TagPoint.position);

        corners[cornerIndex] = TagPoint.position;
        cornerIndex++;
        if (cornerIndex >= 3)
        {
            calcPlane();
            cornerIndex = 0;
        }
    }

    void clearDots()
    {
        foreach(var dot in dots)
        {
            Destroy(dot);
        }
        dots.Clear();
    }

    void newDot(Vector3 position)
    {
        dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dot.GetComponent<MeshRenderer>().material.color = Color.red;
        dot.transform.localScale = Vector3.one * 0.01f;
        dot.transform.position = position;
        dots.Add(dot);
    }

    void calcPlane()
    {
        Vector3 tl = corners[0];
        Vector3 bl = corners[1];
        Vector3 br = corners[2];

        var width = (br - bl).magnitude;
        var height = (tl - bl).magnitude;
        var center = Vector3.Lerp(tl, bl, 0.5f) + (br - bl) / 2;
        var up = (tl - bl).normalized;
        var forward = Vector3.Cross((tl - bl).normalized, (br - bl).normalized).normalized;

        transform.localScale = new Vector3(width, height, 0.005f);
        transform.position = center;
        transform.rotation = Quaternion.LookRotation(forward, up);
    }

    private void Update()
    {
        
    }
}
