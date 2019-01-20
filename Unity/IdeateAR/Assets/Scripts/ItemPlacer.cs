using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class ItemPlacer : MonoBehaviour
{
    public MLHandType handType;
    public GameObject PendingPrefab;

    MLHand mlhand { get { return handType == MLHandType.Left ? MLHands.Left : MLHands.Right; } }
    FHand fhand { get { return handType == MLHandType.Left ? FHands.Left : FHands.Right; } }

    bool isClientReady = true;
    bool isPending;
    bool isPlacing;

    GameObject itemObject;
    
    bool isOperational {  get { return AppController.Instance.CurrentMode == AppModeEnum.Normal; } }
    
    void Start ()
    {
        SocketClient.log("ItemPlacer start");
        MLInput.OnControllerButtonDown += MLInput_OnControllerButtonDown;
	}

    private void MLInput_OnControllerButtonDown(byte arg1, MLInputControllerButton button)
    {
        if (!isOperational) return;
        if (button == MLInputControllerButton.Bumper)
        {
            this.transform.Clear();
        }
    }

    Screen findTargetScreen()
    {
        var screens = GameObject.FindObjectsOfType<Screen>();
        foreach(var screen in screens)
        {
            SocketClient.log("Checking screen " + screen.name);
            if (screen.IsPointerInRange)
            {
                SocketClient.log("Winner winner chicken dinner");
                return screen;
            }
        }

        return null;
    }
    	
	void Update ()
    {
        if (!isOperational) return;
        //if (!MLHands.IsStarted) return;

        bool isPinching = fhand.KeyPose == MLHandKeyPose.Pinch;

        //Pinch start          
        if (AppController.Instance.isPinchStart && !AppController.Instance.isPinchHandled)
        {
            //figure out who to ask for content
            var screen = findTargetScreen();

            if (screen != null)
            {
                //Spawn pending indicator while we wait for actual media
                itemObject = Instantiate<GameObject>(PendingPrefab);
                itemObject.transform.SetParent(this.transform);
                itemObject.transform.localPosition = Vector3.zero;
                isPlacing = true;

                screen.RequestMedia(this.gameObject);

                AppController.Instance.isPinchHandled = true;
            }
            else SocketClient.log("Couldnt find a screen to pull media from: " + AppController.Instance.isPinchHandled);
        }

        if(AppController.Instance.isPinchEnd)
        {
            //Pinch end

            itemObject.transform.SetParent(AppController.Instance.ActiveNucleus);

            isPending = false;
            isPlacing = false;
            itemObject = null;
        }


        if(isPlacing && itemObject != null && MLHands.IsStarted)
        {
            var targetPosition = mlhand.Index.Tip.Position + (mlhand.Index.Tip.Position - mlhand.Center).normalized * 0.05f;

            if (itemObject.transform.localPosition == Vector3.zero)
                itemObject.transform.position = targetPosition;
            else
                itemObject.transform.position = Vector3.Lerp(itemObject.transform.position, targetPosition, 0.15f);

            itemObject.transform.LookAt(Camera.main.transform.position);
        }
	}

    void mediaReady(GameObject mediaItem)
    {
        SocketClient.log("ItemPlacer got mediaReady");
        var pos = itemObject.transform.position;

        SocketClient.log("Destroying pending indicator");
        Destroy(itemObject);

        SocketClient.log("Switching to spawned item");
        itemObject = mediaItem;

        SocketClient.log("Done with media ready");
    }
}

public static class TransformEx
{
    public static Transform Clear(this Transform transform)
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        return transform;
    }
}
