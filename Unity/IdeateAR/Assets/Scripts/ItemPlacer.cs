using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class ItemPlacer : MonoBehaviour
{
    public MLHandType handType;
    public GameObject StickyNotePrefab;

    MLHand mlhand { get { return handType == MLHandType.Left ? MLHands.Left : MLHands.Right; } }
    FHand fhand { get { return handType == MLHandType.Left ? FHands.Left : FHands.Right; } }

    bool wasPinching;
    bool isWaiting;
    bool isPlacing;

    GameObject noteObject;


    void Start ()
    {
        SocketClient.log("ItemPlacer start");
	}
	
	void Update ()
    {
        if (!MLHands.IsStarted) return;

        bool isPinching = fhand.KeyPose == MLHandKeyPose.Pinch;

        if(isPinching && !wasPinching)
        {
            //Pinch start
            SocketClient.log("Pinch Start");
            SocketClient.send("q");
            isWaiting = true;
        }

        if(!isPinching && wasPinching)
        {
            //Pinch end
            SocketClient.log("Pinch End");
            isWaiting = false;
            isPlacing = false;
            noteObject = null;
        }

        if(isWaiting && SocketClient.Instance.IsNewMessage)
        {
            var newMessage = SocketClient.Instance.LastMessage;
            noteObject = Instantiate<GameObject>(StickyNotePrefab);
            noteObject.GetComponent<StickyNote>().Text = newMessage;
            noteObject.transform.localPosition = Vector3.zero;

            isPlacing = true;
            isWaiting = false;
        }

        if(isPlacing && noteObject != null)
        {
            var targetPosition = mlhand.Index.Tip.Position + (mlhand.Index.Tip.Position - mlhand.Center).normalized * 0.05f;

            if (noteObject.transform.localPosition == Vector3.zero)
                noteObject.transform.position = targetPosition;
            else
                noteObject.transform.position = Vector3.Lerp(noteObject.transform.position, targetPosition, 0.15f);

            noteObject.transform.LookAt(Camera.main.transform.position);
        }

        wasPinching = isPinching;
	}
}
