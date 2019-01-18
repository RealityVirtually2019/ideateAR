using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class SocketClient : MonoBehaviour
{
    public static SocketClient Instance;

    public Text StatusLabel;
    public string ServerUrl = "wss://theserver.com/ws";
    WebSocket socket;

    private readonly object messageLock = new object();
    string socketMessage;

    public string LastMessage;
    public bool IsNewMessage;

    void Start()
    {
        Instance = this;
        socket = new WebSocket(ServerUrl);
        socket.OnMessage += Socket_OnMessage;
        socket.OnClose += Socket_OnClose;
        socket.Connect();
    }

    private void Socket_OnClose(object sender, CloseEventArgs e)
    {
        socket.Connect();
    }

    void Update()
    {
        IsNewMessage = false;
        lock (messageLock)
        {
            if (!string.IsNullOrEmpty(socketMessage))
            {
                //StatusLabel.text = "< " + socketMessage;
                LastMessage = socketMessage;
                socketMessage = null;
                IsNewMessage = true;
            }
        }
    }

    public static void send(string message)
    {
        SocketClient.Instance.Send(message);
    }

    public void Send(string message)
    {
        StatusLabel.text = "> " + message;
        if (socket.ReadyState == WebSocketState.Open) socket.SendAsync(message, i => { });
    }

    public static void log(string message)
    {
        SocketClient.Instance.Log(message);
    }

    public void Log(string message)
    {
        Send("l," + message);
    }

    private void Socket_OnMessage(object sender, MessageEventArgs e)
    {
        Debug.Log("Message received: " + e.Data);
        //socket.Send("Unity says oi");

        lock (messageLock)
        {
            socketMessage = e.Data;
        }
    }
}
