using System;
using UnityEngine;

using NativeWebSocket;
using UnityEngine.Assertions;

[Serializable]
public class ClientState
{
    public AvatarData avatar = new AvatarData();
    public ButtonInputData input;
}

public class NetworkClient : MonoBehaviour
{
    public string serverAddress; //  = "192.168.4.58";
    public string serverPort; // = "8888";

    public GameObject vrHeadsetObject;
    public GameObject vrLeftControllerObject;
    public GameObject vrRightControllerObject;

    string wsProtocol;
    string _fullServerAddress;

    WebSocket websocket;

    private GfxReplayPlayer _player;

    ClientState _clientState = new ClientState();
    XRInputHelper _xrInputHelper;

    void Start()
    {
        _player = GetComponent<GfxReplayPlayer>();
        Assert.IsTrue(_player);  // our object should have a GfxReplayPlayer

        _xrInputHelper = new XRInputHelper();

        bool isHttps = false;
        wsProtocol = isHttps ? "wss" : "ws";
        _fullServerAddress = $"{wsProtocol}://{serverAddress}:{serverPort}";

        SetConnectionState(false);

        ConnectWebSocket();

        // Attempt reconnection every X seconds
        InvokeRepeating("AttemptReconnection", 0.0f, 5.0f);

        // Keep sending messages at every 0.1s
        InvokeRepeating("SendClientState", 0.0f, 0.1f);
    }

    void Update()
    {
        if (websocket != null)
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            websocket.DispatchMessageQueue();
#endif
        }
    }

    async void ConnectWebSocket()
    {
        websocket = new WebSocket(_fullServerAddress);

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
            websocket.SendText("client ready!");
            Debug.Log("Sent message: client ready!");

            SetConnectionState(true);

            // delete all old instances on (re)connect
            _player.DeleteAllInstancesFromKeyframes();
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError("WebSocket error: " + e);
            SetConnectionState(false);
            websocket.Close();
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);

            ProcessReceivedKeyframes(message);
        };

        await websocket.Connect();
    }

    void ProcessReceivedKeyframes(string message)
    {
        KeyframeWrapper wrapperArray = JsonUtility.FromJson<KeyframeWrapper>(message);
        foreach (KeyframeData keyframe in wrapperArray.keyframes)
        {
            _player.ProcessKeyframe(keyframe);
        }
    }

    void UpdateClientState()
    {
        Assert.IsNotNull(vrHeadsetObject);
        Assert.IsNotNull(vrLeftControllerObject);
        Assert.IsNotNull(vrRightControllerObject);

        _clientState.avatar.root.FromGameObject(vrHeadsetObject);
        _clientState.avatar.hands[0].FromGameObject(vrLeftControllerObject);
        _clientState.avatar.hands[1].FromGameObject(vrRightControllerObject);

        _clientState.input = _xrInputHelper.UpdateInputData();
    }

    async void SendClientState()
    {
        if (websocket.State == WebSocketState.Open)
        {
            UpdateClientState();
            string jsonStr = JsonUtility.ToJson(_clientState);

            _xrInputHelper.OnEndFrame();
                
            await websocket.SendText(jsonStr);
        }
    }

    void AttemptReconnection()
    {
        if (websocket == null || websocket.State == WebSocketState.Closed)
        {
            SetConnectionState(false);
            ConnectWebSocket();
        }
    }

    void OnDestroy()
    {
        if (websocket != null)
        {
            websocket.Close();
        }

        CancelInvoke("AttemptReconnection");
    }

    void SetConnectionState(bool connected)
    {
        Debug.Log($"SetConnectionState: {connected}");
    }
}