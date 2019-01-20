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

    public bool IsPointerInRange = false;
    public string ScreenId;
    GameObject mediaRequestor;
    bool requestPending;
    bool expectingImageData;
    string modelId;
    string mediaType;
    
    
    public void SetParams(string id, string screenTypeKey, string contentTypeKey)
    {
        ScreenId = id;

        switch(screenTypeKey)
        {
            case "mobile":
                this.screenType = ScreenTypeEnum.Mobile;
                transform.localScale = Vector3.one * 0.2f;
                GetComponent<MeshRenderer>().enabled = false;
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

        IDLabel.text = ScreenId + "\n" + screenType + " - " + contentType;
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
            if (SocketClient.Instance.Sender == ScreenId || contentType == ContentType.Model)
            {
                if (expectingImageData && requestPending)
                {
                    spawnImage(SocketClient.Instance.LastMessage);
                }
                else if (SocketClient.Instance.LastMessage.StartsWith("media"))
                {
                    var tokens = SocketClient.Instance.LastMessage.Split('|');
                    mediaType = tokens[1];

                    switch (mediaType)
                    {
                        case "img":
                            expectingImageData = true;
                            break;
                        case "model":
                            modelId = tokens[2];
                            SocketClient.log("Screen's got a new model " + modelId);
                            break;
                    }
                }
            }
        }
    }

    public void RequestMedia(GameObject requestor)
    {
        SocketClient.log("Screen got a request for media");
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
                SocketClient.Instance.Send("q", ScreenId);
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
        imageData = imageData.Substring(imageData.IndexOf(",")+1); // "data:image/png;base64,".Length);
        //0123
        //1234

        byte[] bytes = System.Convert.FromBase64String(imageData);

        var tex = new Texture2D(1, 1);
        var renderer = itemObject.GetComponent<MeshRenderer>();
        tex.LoadImage(bytes);
        tex.Apply();
        renderer.material.mainTexture = tex;

        mediaRequestor.SendMessage("mediaReady", itemObject);
    }

    void spawnModel(string modelId)
    {
        int index = int.Parse(modelId) - 1;
        var itemObject = Instantiate<GameObject>(Models[index]);
        mediaRequestor.SendMessage("mediaReady", itemObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "pointer")
        {
            IsPointerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "pointer")
        {
            IsPointerInRange = false;
        }
    }
}
