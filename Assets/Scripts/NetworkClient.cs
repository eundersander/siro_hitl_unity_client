using System;
using UnityEngine;

using NativeWebSocket;
using UnityEngine.Assertions;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections;
using static UnityEngine.InputSystem.HID.HID;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

public class BoolArrayHelper
{
    public static List<int> GetTrueIndices(bool[] arr)
    {
        List<int> indices = new List<int>();
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i])
            {
                indices.Add(i);
            }
        }
        return indices;
    }
}

[Serializable]
public class ButtonInputData
{
    public List<int> buttonHeld = new List<int>();
    public List<int> buttonUp = new List<int>();
    public List<int> buttonDown = new List<int>();
}

public class XRInputHelper
{
    XRIDefaultInputActions _inputActions;
    ButtonInputData _inputData = new ButtonInputData();
    const int NUM_BUTTONS = 4;
    bool[] _buttonHeld = new bool[4];

    public XRInputHelper()
    {
        for (int buttonId = 0; buttonId < NUM_BUTTONS; buttonId++)
        {
            _buttonHeld[buttonId] = false;
        }
        OnEndFrame();

        _inputActions = new XRIDefaultInputActions();
        _inputActions.Enable();
        _inputActions.XRILeftHandInteraction.Activate.performed += LeftActivateCallback;
        _inputActions.XRILeftHandInteraction.Activate.canceled += LeftActivateCallback;
        _inputActions.XRILeftHandInteraction.Select.performed += LeftSelectCallback;
        _inputActions.XRILeftHandInteraction.Select.canceled += LeftSelectCallback;
        _inputActions.XRIRightHandInteraction.Activate.performed += RightActivateCallback;
        _inputActions.XRIRightHandInteraction.Activate.canceled += RightActivateCallback;
        _inputActions.XRIRightHandInteraction.Select.performed += RightSelectCallback;
        _inputActions.XRIRightHandInteraction.Select.canceled += RightSelectCallback;
    }

    private void ButtonPressReleaseCallback(int buttonId, bool down)
    {
        Assert.IsTrue(buttonId >= 0 && buttonId < NUM_BUTTONS);
        Debug.Log($"ButtonPressReleaseCallback {buttonId} {down}");
        if (down)
        {
            if (!_buttonHeld[buttonId])
            {
                _buttonHeld[buttonId] = true;
                _inputData.buttonDown.Add(buttonId);
            }
        }
        else
        {
            if (_buttonHeld[buttonId])
            {
                _buttonHeld[buttonId] = false;
                _inputData.buttonUp.Add(buttonId);
            }
        }
    }

    public ButtonInputData UpdateInputData()
    {
        _inputData.buttonHeld = BoolArrayHelper.GetTrueIndices(_buttonHeld);
        return _inputData;
    }
    public void OnEndFrame()
    {
        for (int buttonId = 0; buttonId < NUM_BUTTONS; buttonId++)
        {
            _inputData.buttonUp.Clear();
            _inputData.buttonDown.Clear();
        }
    }

    private void LeftActivateCallback(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        ButtonPressReleaseCallback(0, obj.performed);
    }

    private void LeftSelectCallback(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        ButtonPressReleaseCallback(1, obj.performed);
    }

    private void RightActivateCallback(InputAction.CallbackContext obj)
    {
        ButtonPressReleaseCallback(2, obj.performed);
    }

    private void RightSelectCallback(InputAction.CallbackContext obj)
    {
        ButtonPressReleaseCallback(3, obj.performed);
    }
}

public class CoordinateConventionHelper
{
    private static Quaternion _defaultRotation = Quaternion.Euler(0, 180, 0);
    private static Quaternion _invDefaultRotation = Quaternion.Inverse(_defaultRotation);


    public static Vector3 ToUnityVector(List<float> translation)
    {
        return new Vector3(
            translation[0],
            translation[1],
            -translation[2]
        );
    }

    public static Quaternion ToUnityQuaternion(List<float> rotation)
    {
        Quaternion newRot = new Quaternion(
            rotation[1],
            -rotation[2],
            -rotation[3],
            rotation[0]
        );

        newRot = _defaultRotation * newRot;
        return newRot;
    }

    public static List<float> ToHabitatVector(Vector3 translation)
    {
        return new List<float>
        {
            translation.x,
            translation.y,
            -translation.z
        };
    }

    public static List<float> ToHabitatQuaternion(Quaternion rotation)
    {
        Quaternion convertedRotation = _invDefaultRotation * rotation;

        return new List<float>
        {
            convertedRotation.w,
            convertedRotation.x,
            -convertedRotation.y,
            -convertedRotation.z
        };
    }
}

[Serializable]
public class ClientState
{
    public AvatarData avatar = new AvatarData();
    public ButtonInputData input; //  = new ButtonInputData();
}

[System.Serializable]
public class AvatarData
{
    public PoseData root = new PoseData();
    public PoseData[] hands = new PoseData[]
    {
        new PoseData(),
        new PoseData()
    };
}

[System.Serializable]
public class PoseData
{
    public List<float> position = new List<float>(3);
    public List<float> rotation = new List<float>(4);

    public void FromGameObject(GameObject gameObject)
    {
        position = CoordinateConventionHelper.ToHabitatVector(gameObject.transform.position);
        rotation = CoordinateConventionHelper.ToHabitatQuaternion(gameObject.transform.rotation);
    }
}

public class NetworkClient : MonoBehaviour
{
    //Avatar avatar; // Assuming createAvatar() returns an Avatar instance
    //InputManager inputMgr; // Assuming InputManager is defined elsewhere

    public GameObject vrHeadsetObject;
    public GameObject vrLeftControllerObject;
    public GameObject vrRightControllerObject;

    string wsProtocol;
    string serverAddress;

    WebSocket websocket;

    private GfxReplayPlayer _player;

    ClientState _clientState = new ClientState();
    XRInputHelper _xrInputHelper;

    void Start()
    {
        _player = GetComponent<GfxReplayPlayer>();
        Assert.IsTrue(_player);  // our object should have a GfxReplayPlayer

        _xrInputHelper = new XRInputHelper();

        //avatar = CreateAvatar(); // Assuming CreateAvatar() method exists
        //inputMgr = new InputManager(); // Assuming InputManager is defined elsewhere

        bool isHttps = false; // Application.absoluteURL.StartsWith("https:");
        wsProtocol = isHttps ? "wss://" : "ws://";
        serverAddress = $"{wsProtocol}192.168.4.58:8888";

        SetConnectionState(false);

        ConnectWebSocket();

        // Attempt reconnection every X seconds
        InvokeRepeating("AttemptReconnection", 0.0f, 5.0f);

        // Keep sending messages at every 0.1s
        InvokeRepeating("SendClientState", 0.0f, 0.1f);

        TryEnableXRDeviceSimulatorForEditor();
    }

    private void TryEnableXRDeviceSimulatorForEditor()
    {
#if UNITY_EDITOR
        XRDeviceSimulator xrDeviceSimulator = (XRDeviceSimulator)FindObjectOfType(typeof(XRDeviceSimulator), true);

        // GameObject xrDeviceSimulator = GameObject.Find("XR Device Simulator");
        if (xrDeviceSimulator != null)
        {
            xrDeviceSimulator.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("XR Device Simulator not found in the scene!");
        }
#endif
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
        websocket = new WebSocket(serverAddress);

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
            // Debug.Log("OnMessage!");
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            // Debug.Log("OnMessage! " + message);

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

    // Define your own implementation of these methods
    //Avatar CreateAvatar() { /* ... */ }
    void SetConnectionState(bool connected)
    {
        Debug.Log($"SetConnectionState: {connected}");
    }
    //void DeleteAllInstancesFromKeyframes() { /* ... */ }
}

[Serializable]
public class Avatar
{
    // Define properties and methods relevant to Avatar here
}

public class InputManager
{
    // Define properties and methods relevant to InputManager here
}
