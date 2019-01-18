using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public enum ItemTypeEnum
{
    Unknown,
    Text,
    Image,
    Model
}

public class ItemPlacer : MonoBehaviour
{
    public MLHandType handType;
    public GameObject PendingPrefab;
    public GameObject StickyNotePrefab;
    public GameObject ImageBlockPrefab;

    public GameObject[] Models;

    MLHand mlhand { get { return handType == MLHandType.Left ? MLHands.Left : MLHands.Right; } }
    FHand fhand { get { return handType == MLHandType.Left ? FHands.Left : FHands.Right; } }

    bool isClientReady = true;
    bool wasPinching;
    bool isPending;
    bool isPlacing;

    ItemTypeEnum expectedItemType;
    GameObject itemObject;
    string currentModelId;
    string currentText;
    
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
    /*
    void loadImage(string imageData)
    {
        imageData = imageData.Substring("data:image/png;base64,".Length);

        byte[] bytes = System.Convert.FromBase64String(imageData);

        var tex = new Texture2D(1, 1);
        tex.LoadImage(bytes);
        tex.Apply();

        var renderer = itemObject.GetComponent<MeshRenderer>();
        
        renderer.material.mainTexture = tex; 
    }*/
	
	void Update ()
    {       
        if (!MLHands.IsStarted) return;

        processIncoming();

        bool isPinching = fhand.KeyPose == MLHandKeyPose.Pinch;

        if(isPinching && !wasPinching)
        {
            //Pinch start          

            if (isClientReady)
            {
                //Send out request for item
                //TODO: this needs to be spatialy aware of the screen to direct request
                SocketClient.send("q");
                isPending = true;

                //Spawn pending object
                itemObject = Instantiate<GameObject>(PendingPrefab); //StickyNotePrefab);
                itemObject.transform.SetParent(this.transform);
                itemObject.transform.localPosition = Vector3.zero;
                isPlacing = true;
            }
            else if(!string.IsNullOrEmpty(currentModelId))
            {
                isPlacing = true;
                spawnModel(currentModelId);
            }
        }

        if(!isPinching && wasPinching)
        {
            //Pinch end
            isPending = false;
            isPlacing = false;
            itemObject = null;
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

    void processIncoming()
    {
        if (SocketClient.Instance.IsNewMessage)
        {
            var msg = SocketClient.Instance.LastMessage;
            if (msg[0] == '~')
            {
                msg = msg.Substring(1);
                var tokens = msg.Split('|');
                var command = tokens[0];
                switch (command)
                {
                    case "client_ready":
                        SocketClient.log("I see the client is ready");
                        isClientReady = true;
                        break;
                    case "i": //Incoming type
                        var typeName = tokens[1].ToLower();
                        switch (typeName)
                        {
                            case "txt":
                                expectedItemType = ItemTypeEnum.Text;
                                spawnStickyNote(tokens[2]);
                                isPending = false;
                                break;
                            case "img":
                                expectedItemType = ItemTypeEnum.Image;
                                break;
                            case "scan":
                                isClientReady = false;
                                expectedItemType = ItemTypeEnum.Model;
                                currentModelId = tokens[2];
                                break;
                        }
                        break;
                }
            }
            else
            {
                //raw data
                if (isPending)
                {
                    switch (expectedItemType)
                    {
                        case ItemTypeEnum.Image:
                            spawnImage(msg);
                            isPending = false;
                            break;
                    }
                }
            }
        }
    }

    void spawnStickyNote(string text)
    {
        var pos = itemObject.transform.position;
        Destroy(itemObject);//Get rid of the pending indicator

        itemObject = Instantiate<GameObject>(StickyNotePrefab);
        itemObject.GetComponent<StickyNote>().Text = text;
        itemObject.transform.SetParent(this.transform);
        itemObject.transform.position = pos;
    }

    void spawnImage(string imageData)
    {
        var pos = itemObject.transform.position;
        Destroy(itemObject);//Get rid of the pending indicator

        itemObject = Instantiate<GameObject>(ImageBlockPrefab);
        itemObject.transform.SetParent(this.transform);
        itemObject.transform.position = pos;

        imageData = imageData.Substring("data:image/png;base64,".Length);

        byte[] bytes = System.Convert.FromBase64String(imageData);

        var tex = new Texture2D(1, 1);
        var renderer = itemObject.GetComponent<MeshRenderer>();
        tex.LoadImage(bytes);
        tex.Apply();
        renderer.material.mainTexture = tex;

    }

    void spawnModel(string modelId)
    {
        int index = int.Parse(modelId) - 1;
        SocketClient.log("Spawning model " + index);
        itemObject = Instantiate<GameObject>(Models[index]);
        itemObject.transform.SetParent(this.transform);
        itemObject.transform.localPosition = Vector3.zero;
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
