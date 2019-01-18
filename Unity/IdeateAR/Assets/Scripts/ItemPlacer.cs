using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class ItemPlacer : MonoBehaviour
{
    public MLHandType handType;
    public GameObject StickyNotePrefab;
    public GameObject ImageBlockPrefab;

    MLHand mlhand { get { return handType == MLHandType.Left ? MLHands.Left : MLHands.Right; } }
    FHand fhand { get { return handType == MLHandType.Left ? FHands.Left : FHands.Right; } }

    bool wasPinching;
    bool isWaiting;
    bool isPlacing;

    GameObject itemObject;
    
    void Start ()
    {
        SocketClient.log("ItemPlacer start");
        MLInput.OnControllerButtonDown += MLInput_OnControllerButtonDown;
	}

    private void MLInput_OnControllerButtonDown(byte arg1, MLInputControllerButton button)
    {
        if(button == MLInputControllerButton.Bumper)
        {
            this.transform.Clear();
        }
    }

    void loadImage(string imageData)
    {
        imageData = imageData.Substring("data:image/png;base64,".Length);

        byte[] bytes = System.Convert.FromBase64String(imageData);

        var tex = new Texture2D(1, 1);
        tex.LoadImage(bytes);
        tex.Apply();

        var renderer = itemObject.GetComponent<MeshRenderer>();
        
        renderer.material.mainTexture = tex; 
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

            itemObject = Instantiate<GameObject>(ImageBlockPrefab); //StickyNotePrefab);
            itemObject.transform.SetParent(this.transform);
            itemObject.transform.localPosition = Vector3.zero;
            isPlacing = true;
        }

        if(!isPinching && wasPinching)
        {
            //Pinch end
            SocketClient.log("Pinch End");
            isWaiting = false;
            isPlacing = false;
            itemObject = null;
        }

        if(isWaiting && SocketClient.Instance.IsNewMessage)
        {
            isWaiting = false;

            //noteObject.GetComponent<StickyNote>().Text = SocketClient.Instance.LastMessage;
            loadImage(SocketClient.Instance.LastMessage);

        }

        if(isPlacing && itemObject != null)
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
