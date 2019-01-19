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
    bool wasPinching;
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
        RaycastHit hitInfo;
        Screen selectedScreen = null;

        var hits = Physics.BoxCastAll(Camera.main.transform.position, Vector3.one * 0.05f, Camera.main.transform.forward, Quaternion.identity, 2);
        foreach(var hit in hits)
        {
            if(hit.collider.gameObject.tag == "screen")
            {
                selectedScreen = hit.collider.gameObject.GetComponent<Screen>();
                SocketClient.log(hit.distance.ToString());
            }
        }
        return selectedScreen;
    }
    	
	void Update ()
    {
        if (!isOperational) return;
        //if (!MLHands.IsStarted) return;

        bool isPinching = fhand.KeyPose == MLHandKeyPose.Pinch;

        if(isPinching && !wasPinching)
        {
            //Pinch start          
            SocketClient.log("Item Placer sees pinch looking for a screen");
            //figure out who to ask for content
            var screen = findTargetScreen(); //GameObject.FindObjectOfType<Screen>(); //TODO: actually hook this up, instaed of assuming theres just one

            if (screen != null)
            {
                SocketClient.log("ItemPlacer requesting media from " + screen.ScreenId);
                screen.RequestMedia(this.gameObject);

                //Spawn pending indicator while we wait for actual media
                itemObject = Instantiate<GameObject>(PendingPrefab);
                itemObject.transform.SetParent(this.transform);
                itemObject.transform.localPosition = Vector3.zero;
                isPlacing = true;
            }
            else SocketClient.log("Couldnt find a screen to pull media from");
        }

        if(!isPinching && wasPinching)
        {
            //Pinch end
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

        wasPinching = isPinching;
	}

    void mediaReady(GameObject mediaItem)
    {
        SocketClient.log("ItemPlacer got mediaReady");
        var pos = itemObject.transform.position;
        Destroy(itemObject);
        itemObject = mediaItem;
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
