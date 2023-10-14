using System;
using UnityEngine;
using System.Collections.Generic;
using NativeWebSocket;
using UnityEngine.Assertions;
using UnityEngine.Windows;
using System.Collections;
using System.Threading.Tasks;

[Serializable]
public class ClientState
{
    public AvatarData avatar = new AvatarData();
    public ButtonInputData input;
}

public class NetworkClient : MonoBehaviour
{
    public class FlagObject
    {
        private bool flag = false;

        public void SetFlag()
        {
            flag = true;
        }

        public bool IsSet()
        {
            return flag;
        }
    }

    public int defaultServerPort = 8888;

    public GameObject vrHeadsetObject;
    public GameObject vrLeftControllerObject;
    public GameObject vrRightControllerObject;

    List<string> _serverURLs = new List<string>();
    private WebSocket mainWebSocket;
    private int currentServerIndex = 0;

    private GfxReplayPlayer _player;
    private ConfigLoader _configLoader;

    ClientState _clientState = new ClientState();
    XRInputHelper _xrInputHelper;

    void Start()
    {
        _player = GetComponent<GfxReplayPlayer>();
        Assert.IsTrue(_player);  // our object should have a GfxReplayPlayer
        _configLoader = GetComponent<ConfigLoader>();
        Assert.IsTrue(_configLoader);

        _xrInputHelper = new XRInputHelper();

        string[] serverLocations = _configLoader.AppConfig.serverLocations;
        Assert.IsTrue(serverLocations.Length > 0);
        foreach (string location in serverLocations)
        {
            string adjustedLocation = location;
            if (!adjustedLocation.Contains(":"))
            {
                adjustedLocation += ":" + defaultServerPort;
            }

            bool isHttps = false;
            string wsProtocol = isHttps ? "wss" : "ws";
            _serverURLs.Add($"{wsProtocol}://{adjustedLocation}");
        }

        StartCoroutine(TryConnectToServers());

        // Keep sending messages at every 0.1s
        InvokeRepeating("SendClientState", 0.0f, 0.1f);
    }

    void Update()
    {
        if (mainWebSocket != null)
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            mainWebSocket.DispatchMessageQueue();
#endif
        }
    }

    private IEnumerator TryConnectToServers()
    {
        while (true)
        {
            if (mainWebSocket == null)
            {
                currentServerIndex++;
                if (currentServerIndex >= _serverURLs.Count)
                {
                    currentServerIndex = 0; // Reset to try again from the beginning.
                }

                string url = _serverURLs[currentServerIndex];
                Debug.Log("Attempting to connect to " + url);

                var websocket = new WebSocket(url);
                FlagObject doAbort = new FlagObject();
                // This is an async function that will finish later (but
                // still on the main Unity thread).
                var connectTask = ConnectWebSocket(websocket, url, doAbort);

                // Wait for 4s, then check the result
                yield return new WaitForSeconds(4);

                if (mainWebSocket == null && websocket.State == WebSocketState.Connecting)
                {
                    // Wait another 4s
                    yield return new WaitForSeconds(4);
                }

                if (mainWebSocket == null)
                {
                    if (websocket.State == WebSocketState.Connecting)
                    {
                        Debug.LogWarning($"Timeout! Aborting connect to {url}.");
                    }
                    // See OnOpen callback in ConnectWebSocket where we check
                    // this flag and potentially discard the connection.
                    doAbort.SetFlag();
                }
            }
            else
            {
                // If we have an active connection, we simply wait here.
                // Adjust this to check more or less frequently as desired.
                yield return new WaitForSeconds(1);
            }
        }
    }

    private async Task ConnectWebSocket(WebSocket websocket, string url, FlagObject doAbort)
    {
        websocket.OnOpen += () =>
        {
            if (doAbort.IsSet())
            {
                Debug.Log("Discarding connection to " + url);
                websocket.Close();
                return;
            }

            Debug.Log("Connected to: " + url);

            websocket.SendText("client ready!");
            Debug.Log("Sent message: client ready!");

            // delete all old instances on (re)connect
            _player.DeleteAllInstancesFromKeyframes();

            mainWebSocket = websocket;
        };

        websocket.OnError += (e) =>
        {
            Debug.LogWarning($"Error connecting to {url}: {e}.");
        };

        websocket.OnClose += (e) =>
        {
            if (websocket == mainWebSocket)
            {
                Debug.Log("Main connection closed!");
                mainWebSocket = null;
            }
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
        if (mainWebSocket != null && mainWebSocket.State == WebSocketState.Open)
        {
            UpdateClientState();
            string jsonStr = JsonUtility.ToJson(_clientState);

            _xrInputHelper.OnEndFrame();
                
            await mainWebSocket.SendText(jsonStr);
        }
    }

    void OnDestroy()
    {
        if (mainWebSocket != null)
        {
            mainWebSocket.Close();
            mainWebSocket = null;
        }

        StopAllCoroutines();
        CancelInvoke("SendClientState");
    }
}