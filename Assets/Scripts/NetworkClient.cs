using System;
using UnityEngine;
using System.Collections.Generic;
using NativeWebSocket;
using Newtonsoft.Json;
using UnityEngine.Assertions;
using System.Collections;
using System.Threading.Tasks;

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

    private GfxReplayPlayer _player;
    private ConfigLoader _configLoader;

    Dictionary<string, string> _connectionParams;

    ClientState _clientState = new ClientState();
    IClientStateProducer[] _clientStateProducers;

    // Used to handle data that is only meant to be sent during the first transmission.
    private bool _firstTransmission = true;

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
        _player = GetComponent<GfxReplayPlayer>();
        Assert.IsTrue(_player);  // our object should have a GfxReplayPlayer
        _configLoader = GetComponent<ConfigLoader>();
        Assert.IsTrue(_configLoader);

        // Search the codebase for available IClientStateProducer.
        // They should be added to this GameObject via the Editor (or programmatically, before adding this Component).
        _clientStateProducers = GetComponents<IClientStateProducer>();
        if (_clientStateProducers.Length == 0)
        {
            Debug.LogWarning("No IClientStateProducer could be found. The client won't send any data to the server.");
        }
    }

    void Start()
    {
        // Read URL query parameters
        _connectionParams = GetConnectionParameters();

        GetServerHostnameAndPort(_connectionParams, 
                                 out string? _serverHostnameOverride,
                                 out int? _serverPortOverride);

        // Set up server hostnames and port.
        string[] serverLocations = _serverHostnameOverride != null ? 
                                        new[]{_serverHostnameOverride} : 
                                        _configLoader.AppConfig.serverLocations;
        int serverPort = _serverPortOverride != null ?
                            _serverPortOverride.Value :
                            defaultServerPort;
        Assert.IsTrue(serverLocations.Length > 0);
        foreach (string location in serverLocations)
        {
            string adjustedLocation;
            if (!location.Contains(":"))
            {
                adjustedLocation = $"{location}:{serverPort}";
            }
            else
            {
                adjustedLocation = location;
            }

            bool isHttps = false;
            string wsProtocol = isHttps ? "wss" : "ws";
            _serverURLs.Add($"{wsProtocol}://{adjustedLocation}");
        }

        // Start networking.
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
            messagesReceivedCount++;
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
                // Log the count of received messages
                float messageRate = (float)messagesReceivedCount / duration;

                Debug.Log($"Message rate: {messageRate.ToString("F1")}, FPS: {fps.ToString("F1")}");

                _player.SetKeyframeRate(messageRate);
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
        if (_clientStateProducers == null)
        {
            return;
        }

        foreach (var updater in _clientStateProducers)
        {
            updater.UpdateClientState(ref _clientState);
        }
    }

    async void SendClientState()
    {
        if (isConnected())
        {
            // Update ClientState
            UpdateClientState();

            // Only include connection parameters in the first successful transmission.
            if (_firstTransmission && _connectionParams?.Count > 0)
            {
                _clientState.connection_params_dict = _connectionParams;
            }

            // Serialize ClientState to JSON.
            string jsonStr = JsonConvert.SerializeObject(_clientState, Formatting.None, _jsonSettings);

            // Reset the state of IClientStateProducers
            foreach (var updater in _clientStateProducers)
            {
                updater.OnEndFrame();
            }

            // Send the ClientState to the server.
            Task task = mainWebSocket.SendText(jsonStr);
            await task;

            // Update state after first successful transmission.
            if (_firstTransmission && task.Status == TaskStatus.RanToCompletion)
            {
                _clientState.connection_params_dict = null;
                _firstTransmission = false;
            }
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

    private Dictionary<string, string> GetConnectionParameters()
    {
        var output  = new Dictionary<string, string>();

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

    private void GetServerHostnameAndPort(Dictionary<string, string> queryParams,                                   
                                         out string? _serverHostnameOverride,
                                         out int? _serverPortOverride)
    {
        _serverHostnameOverride = null;
        _serverPortOverride = null;

        // Override server URL from query arguments.
        if (queryParams.TryGetValue("server_hostname", out string serverHostnameString))
        {
            if (Uri.CheckHostName(serverHostnameString) != UriHostNameType.Unknown)
            {
                _serverHostnameOverride = serverHostnameString;
            }
            else
            {
                Debug.LogError($"Invalid server_hostname: '{serverHostnameString}'");
            }
        }

        // Override server port from query arguments.
        if (queryParams.TryGetValue("server_port", out string serverPortString))
        {
            if (int.TryParse(serverPortString, out int port))
            {
                _serverPortOverride = port;
            }
            else
            {
                Debug.LogError($"Invalid server_port: '{serverPortString}'");
            }
        }
    }
}