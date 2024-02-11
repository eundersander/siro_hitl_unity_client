using System;
using UnityEngine;
using System.Collections.Generic;
using NativeWebSocket;
using UnityEngine.Assertions;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Web;

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

    List<string> _serverURLs = new List<string>();
    private WebSocket mainWebSocket;
    private int currentServerIndex = 0;
    private int messagesReceivedCount = 0;
    private int frameCount = 0;
    private float _delayReconnect = 0.0f;
    private string _disconnectReason = "";

    private GfxReplayPlayer _player;
    private ConfigLoader _configLoader;

    Dictionary<string, string> _connectionParams;

    string? _serverHostnameOverride = null;
    int? _serverPortOverride = null;

    ClientState _clientState = new ClientState();
    InputTracker[] _inputTrackers;
    TextConsumer _textConsumer;  // used to display disconnect status
    int _recentConnectionMessageCount = 0;

    JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
    {
        // Omit null values when serializing objects.
        // E.g. XR-specific fields will be omitted when XR is disabled.
        NullValueHandling = NullValueHandling.Ignore,
        // Skip missing JSON fields when deserializing.
        MissingMemberHandling = MissingMemberHandling.Ignore
    };

    void Awake()
    {
        _connectionParams = GetURLQueryParameters();

        // Override server URL from query arguments.
        if (_connectionParams.TryGetValue("server_hostname", out string serverHostname))
        {
            if (Uri.CheckHostName(serverHostname) != UriHostNameType.Unknown)
            {
                _serverHostnameOverride = serverHostname;
            }
            else
            {
                Debug.LogError($"Invalid server_hostname: '{serverHostname}'");
            }
        }

        // Override server port from query arguments.
        if (_connectionParams.TryGetValue("server_port", out string serverPort))
        {
            if (int.TryParse(serverPort, out int port))
            {
                _serverPortOverride = port;
            }
            else
            {
                Debug.LogError($"Invalid server_port: '{serverPort}'");
            }
        }

        _textConsumer = GetComponent<TextConsumer>();
    }

    void Start()
    {
        _player = GetComponent<GfxReplayPlayer>();
        Assert.IsTrue(_player);  // our object should have a GfxReplayPlayer
        _configLoader = GetComponent<ConfigLoader>();
        Assert.IsTrue(_configLoader);

        _inputTrackers = GetComponents<InputTracker>();
        if (_inputTrackers.Length == 0)
        {
            Debug.LogWarning("No InputTracker could be found. The client won't send any data to the server.");
        }

        string[] serverLocations = _serverHostnameOverride != null ? 
                                        new[]{_serverHostnameOverride} : 
                                        _configLoader.AppConfig.serverLocations;
        Assert.IsTrue(serverLocations.Length > 0);
        foreach (string location in serverLocations)
        {
            string adjustedLocation = location;
            if (!adjustedLocation.Contains(":"))
            {
                adjustedLocation += ":" + (_serverPortOverride != null ?
                                            _serverPortOverride.Value :
                                            defaultServerPort);
            }

            bool isHttps = false;
            string wsProtocol = isHttps ? "wss" : "ws";
            _serverURLs.Add($"{wsProtocol}://{adjustedLocation}");
        }

        StartCoroutine(TryConnectToServers());

        StartCoroutine(LogMessageRate());

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
        frameCount++;
    }

    private IEnumerator TryConnectToServers()
    {
        while (true)
        {
            if (mainWebSocket == null)
            {
                if (_delayReconnect > 0.0f)
                {
                    Debug.Log($"Delaying reconnect for {_delayReconnect}s");
                    int numWaits = (int)_delayReconnect;
                    for (int i = 0; i < numWaits; i++)
                    {
                        int remainingTime = numWaits - i;
                        SetDisconnectStatus($"{_disconnectReason}\nRetrying in {remainingTime}s...");
                        yield return new WaitForSeconds(1.0f);
                    }
                    SetDisconnectStatus("");
                    _delayReconnect = 0.0f;
                    _disconnectReason = "";
                }

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
                    SetDisconnectStatus("Trying to reach servers...");
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

                    if (_disconnectReason == "")
                    {
                        _delayReconnect = 5.0f;
                        _disconnectReason = "Unable to reach servers!";
                    }
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
            _recentConnectionMessageCount = 0;

            // send connection params
            _connectionParams["isClientReady"] = "1";  // sloppy: set to string "1" instead of True
            string jsonStr = JsonConvert.SerializeObject(_connectionParams, Formatting.None, _jsonSettings);
            websocket.SendText(jsonStr);
            Debug.Log("Sent message: client ready!");

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
                SetDisconnectStatus("");

                const int messageThreshold = 10;

                // delay reconnect after close, to avoid spamming servers and to give other clients a chance to connect
                if (_recentConnectionMessageCount >= messageThreshold)
                {
                    // This was a long-lasting connection. Disconnected most likely due to client idle.
                    _disconnectReason = "Disconnected!";
                    _delayReconnect = 15.0f;
                }
                else
                {
                    // This was a short connection. Disconnected most likely due to server already having a client.
                    _disconnectReason = "All servers are busy!";
                    _delayReconnect = 10.0f;
                }
            }
        };

        websocket.OnMessage += (bytes) =>
        {
            if (_recentConnectionMessageCount == 0) {
                // delete all old instances on reconnect
                _player.DeleteAllInstancesFromKeyframes();
            }

            string message = System.Text.Encoding.UTF8.GetString(bytes);
            ProcessReceivedKeyframes(message);
            messagesReceivedCount++;
            _recentConnectionMessageCount++;
        };

        await websocket.Connect();
    }

    private bool isConnected()
    {
        return mainWebSocket != null && mainWebSocket.State == WebSocketState.Open;
    }

    private IEnumerator LogMessageRate()
    {
        while (true)
        {
            // Wait for a second
            float duration = 2.0F;
            yield return new WaitForSeconds(duration);

            float fps = frameCount / duration;

            if (isConnected())
            {
                if (messagesReceivedCount > 0 && duration > 0)
                {
                    // Log the count of received messages
                    float messageRate = (float)messagesReceivedCount / duration;
                    Debug.Log($"Message rate: {messageRate.ToString("F1")}, FPS: {fps.ToString("F1")}");
                    _player.SetKeyframeRate(messageRate);
                }
            } else
            {
                Debug.Log($"disconnected, FPS: {fps.ToString("F1")}");
            }

            // Reset the count
            messagesReceivedCount = 0;
            frameCount = 0;
        }
    }

    void ProcessReceivedKeyframes(string message)
    {
        KeyframeWrapper wrapperArray = JsonConvert.DeserializeObject<KeyframeWrapper>(message, _jsonSettings);
        foreach (KeyframeData keyframe in wrapperArray.keyframes)
        {
            _player.ProcessKeyframe(keyframe);
        }
    }

    void UpdateClientState()
    {
        if (_inputTrackers == null)
        {
            return;
        }

        foreach (var updater in _inputTrackers)
        {
            updater.UpdateClientState(ref _clientState);
        }
    }

    void SetDisconnectStatus(string status)
    {
        if (!_textConsumer)
        {
            return;
        }

        Message message = new Message();

        if (!String.IsNullOrEmpty(status))
        {
            Text text = new Text();
            text.text = status;
            text.position = new List<float> { 0.0f, 0.3f }; // roughly centered
            message.texts = new List<Text> { text };
        }
        _textConsumer.ProcessMessage(message);
    }

    async void SendClientState()
    {
        if (isConnected())
        {
            // Update the ClientState data.
            UpdateClientState();

            // Serialize the ClientState data to JSON.
            string jsonStr = JsonConvert.SerializeObject(_clientState, Formatting.None, _jsonSettings);

            // Reset the trackers.
            foreach (var updater in _inputTrackers)
            {
                updater.OnEndFrame();
            }

            // Send the message to the server.
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

    public bool IsConnected() 
    {
        return mainWebSocket != null && mainWebSocket.State == WebSocketState.Open;
    }

    private Dictionary<string, string> GetURLQueryParameters()
    {
        var output  = new Dictionary<string, string>();

        // example: http://localhost:49603/?server_hostname=127.0.0.1&server_port=8888&workername=eric
        string url = Application.absoluteURL;
        if (url?.Length > 0)
        {
            var splitUrl = url.Split('?');
            if (splitUrl.Length == 2)
            {
                var queryString = splitUrl[1];
                var paramsCollection = HttpUtility.ParseQueryString(queryString);

                foreach (var key in paramsCollection.AllKeys)
                {
                    output[key] = paramsCollection[key];
                }
            }
        }

        return output;
    }
}