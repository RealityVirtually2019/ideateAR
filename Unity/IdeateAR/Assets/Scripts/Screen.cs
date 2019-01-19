using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public enum ScreenTypeEnum
{
    Mobile,
    Static
}

public enum ContentType
{
    Drawing,
    Excel,
    Model
}


public class Screen : MonoBehaviour
{
    public TextMeshPro IDLabel;
    public GameObject ImageBlockPrefab;
    public GameObject ExcelPrefab;
    public GameObject[] Models;

    ScreenTypeEnum screenType;
    ContentType contentType;
    FVector handCenter = new FVector();

    string screenId;
    GameObject mediaRequestor;
    bool requestPending;
    bool expectingImageData;
    string modelId;
    string mediaType;
    
    public void SetParams(string id, string screenTypeKey, string contentTypeKey)
    {
        screenId = id;

        switch(screenTypeKey)
        {
            case "mobile":
                this.screenType = ScreenTypeEnum.Mobile;
                transform.localScale = Vector3.one * 0.1f;
                break;
            case "static":
                this.screenType = ScreenTypeEnum.Static;
                this.gameObject.AddComponent<ScreenEnroller>();
                break;
        }
       
        switch (contentTypeKey)
        {
            case "drawing":
                contentType = ContentType.Drawing;
                break;
            case "model":
                contentType = ContentType.Model;                
                break;
            case "excel":
                contentType = ContentType.Excel;
                break;
        }

        IDLabel.text = screenId + "\n" + screenType + " - " + contentType;
    }

    private void Update()
    {
        if(screenType == ScreenTypeEnum.Mobile && MLHands.IsStarted)
        {
            handCenter.Push(MLHands.Left.Center);
            transform.position = handCenter.Value;
        }


        if(SocketClient.Instance.IsNewMessage)
        {
            if (SocketClient.Instance.Sender == screenId || contentType == ContentType.Model)
            {
                SocketClient.log("Screen sees a msg its interested in");

                if (expectingImageData && requestPending)
                {
                    SocketClient.log("Screen got image data");
                    spawnImage(SocketClient.Instance.LastMessage);
                }
                else if (SocketClient.Instance.LastMessage.StartsWith("media"))
                {
                    SocketClient.log("Screen sees a media msg");
                    var tokens = SocketClient.Instance.LastMessage.Split('|');
                    mediaType = tokens[1];

                    SocketClient.log("Screen media type " + mediaType);
                    switch (mediaType)
                    {
                        case "img":
                            SocketClient.log("Screen is waiting on image data");
                            expectingImageData = true;
                            break;
                        case "model":
                            modelId = tokens[2];
                            break;
                    }
                }

                
            }
        }
    }

    public void RequestMedia(GameObject requestor)
    {
        SocketClient.log("Screen sees media request from " + requestor.name);

        mediaRequestor = requestor;

        switch(contentType)
        {
            case ContentType.Model:
                spawnModel(modelId);
                break;
            case ContentType.Excel:
                spawnExcelDoc();
                break;
            case ContentType.Drawing:
                SocketClient.log("sending query request");
                SocketClient.Instance.Send("q", screenId);
                requestPending = true;
                break;
        }
        
    }

    void spawnExcelDoc()
    {
        var itemObject = Instantiate<GameObject>(ExcelPrefab);
        mediaRequestor.SendMessage("mediaReady", itemObject);
    }


    void spawnImage(string imageData)
    {
        var itemObject = Instantiate<GameObject>(ImageBlockPrefab);
        imageData = imageData.Substring("data:image/png;base64,".Length);

        SocketClient.log("spawning image " + imageData);

        byte[] bytes = System.Convert.FromBase64String(imageData);

        var tex = new Texture2D(1, 1);
        var renderer = itemObject.GetComponent<MeshRenderer>();
        tex.LoadImage(bytes);
        tex.Apply();
        renderer.material.mainTexture = tex;

        SocketClient.log("Image block created. sending back to item placer " + mediaRequestor.name);
        mediaRequestor.SendMessage("mediaReady", itemObject);
    }

    void spawnModel(string modelId)
    {
        int index = int.Parse(modelId) - 1;
        SocketClient.log("Spawning model " + index);
        var itemObject = Instantiate<GameObject>(Models[index]);

        mediaRequestor.SendMessage("mediaReady", itemObject);
    }
}
