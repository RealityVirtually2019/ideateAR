using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum AppModeEnum
{
    Enrolling,
    Normal
}

public class AppController : MonoBehaviour
{
    public static AppController Instance;

    public TextMeshPro ControllerLabel;
    public GameObject ScreenPrefab;
    public AppModeEnum CurrentMode;
    

	void Start ()
    {
        Instance = this;
        SetMode(AppModeEnum.Normal);
	}

    private void Update()
    {
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
    }

    public void SetMode(AppModeEnum newMode)
    {
        CurrentMode = newMode;
        ControllerLabel.text = newMode.ToString();
    }
}
