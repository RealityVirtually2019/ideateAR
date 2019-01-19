using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.MagicLeap;

public enum AppModeEnum
{
    Enrolling,
    Normal
}

public class AppController : MonoBehaviour
{
    public static AppController Instance;

    public Transform PinchPointer;
    public Transform ActiveNucleus;
    public TextMeshPro ControllerLabel;
    public GameObject ScreenPrefab;
    public AppModeEnum CurrentMode;

    bool wasPinching;

    public bool isPinchStart;
    public bool isPinchEnd;
    public bool isPinching;
    public bool isPinchHandled;

    MLHand mlhand { get { return MLHands.Right; } }
    FHand fhand { get { return FHands.Right; } }

    public FVector PinchPoint = new FVector();

    void Start ()
    {
        Instance = this;
        SetMode(AppModeEnum.Normal);
	}

    private void Update()
    {
        bool isPinching = fhand.KeyPose == MLHandKeyPose.Pinch;

        isPinchHandled = false;
        isPinchStart = isPinching && !wasPinching;
        isPinchEnd = !isPinching && wasPinching;

        var pinchPoint = Vector3.Lerp(mlhand.Index.Tip.Position, mlhand.Thumb.Tip.Position, 0.5f);
        if (isPinchStart)
        {
            PinchPoint.Value = Vector3.Lerp(mlhand.Index.Tip.Position, mlhand.Thumb.Tip.Position, 0.5f);
        }
        
        PinchPoint.Push(pinchPoint);
        PinchPointer.position = PinchPoint.Value;

        if (SocketClient.Instance.IsNewMessage)
        {
            if(SocketClient.Instance.LastMessage.StartsWith("enroll"))
            {
                var tokens = SocketClient.Instance.LastMessage.Split('|');
                var screenType = tokens[1];
                var contentType = tokens[2];
                var newScreen = Instantiate<GameObject>(ScreenPrefab);
                newScreen.GetComponent<Screen>().SetParams(SocketClient.Instance.Sender, screenType, contentType);
            }
            else if (SocketClient.Instance.LastMessage == "reset")
            {
                SocketClient.log("App is resetting");
                SceneManager.LoadScene(0);
            }
        }

        wasPinching = isPinching;
    }

    public void SetMode(AppModeEnum newMode)
    {
        CurrentMode = newMode;
        ControllerLabel.text = newMode.ToString();
    }
}
