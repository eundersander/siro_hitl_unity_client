using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

/// <summary>
/// VR Habitat client application.
/// Intended to be deployed to Quest devices (Android).
/// </summary>
public class AppVR : MonoBehaviour
{
    [Header("Config Defaults")]
    [Tooltip("Config defaults are used directly when running in the Editor. On device, they are used to populate config.txt at Android/data/com.meta.siro_hitl_vr_client/files/. This file persists and can be edited between runs, e.g. by connecting via USB to a laptop.")]
    [SerializeField] private bool _mouseoverForTooltip;  // dummy member so we can add tooltip in Inspector pane

    [Tooltip("Default server locations.")]
    [SerializeField] private string[] _defaultServerLocations = new string[]{ "127.0.01:8888" };

    [Tooltip("If specified, the keyframe will be played upon launching the client.")]
    [SerializeField] TextAsset _testKeyframe;

    [Header("Rendering Configuration")]

    [Tooltip("Main camera (XR camera).")]
    [SerializeField] private Camera _camera;

    [Tooltip("Global application configuration.")]
    [SerializeField] private AppConfig _appConfig;

    [Tooltip("GameObject instance of the left XR controller.")]
    [SerializeField] private GameObject _xrLeftController;

    [Tooltip("GameObject instance of the right XR controller.")]
    [SerializeField] private GameObject _xrRightController;

    [Tooltip("Root GameObject of the XR camera and controllers.")]
    [SerializeField] private GameObject _xrOrigin;

    [Tooltip("Icon that is displayed when connection is lost.")]
    [SerializeField] private GameObject _offlineIcon;

    [Tooltip("Root of the text panel.")]
    [SerializeField] private GameObject _textPanelRoot;

    [Tooltip("Text to render using TextRenderer.")]
    [SerializeField] private TextMeshPro _textComponent;

    [Tooltip("Text UI plane distance from camera.")]
    [SerializeField] float _textUiPlaneDistance;
    

    // IKeyframeMessageConsumers
    AvatarPositionHandler _avatarPositionHandler;
    HighlightManager _highlightManager;
    LoadingEffectHandler _loadingEffectHandler;
    TextRenderer _textRenderer;
    NavmeshHelper _navmeshHelper;

    // IClientStateProducers
    InputTrackerXRControllers _inputTrackerXrControllers;
    InputTrackerXRPose _inputTrackerXrPose;

    // Application state
    ConfigLoader _configLoader;
    GfxReplayPlayer _gfxReplayPlayer;
    NetworkClient _networkClient;
    CollisionFloor _collisionFloor;
    OnlineStatusDisplayHandler _onlineStatusDisplayHandler;
    ReplayFileLoader _replayFileLoader;

    void Awake()
    {
        LaunchXRDeviceSimulator();

        // Create collision floor for XR controller.
        _collisionFloor = new CollisionFloor();

        // Initialize IKeyframeMessageConsumers.
        _avatarPositionHandler = new AvatarPositionHandler(_camera.gameObject, _xrOrigin);
        _highlightManager = new HighlightManager(_appConfig, _camera);
        _loadingEffectHandler = new LoadingEffectHandler();
        _textRenderer = new TextRenderer(_textUiPlaneDistance, _textPanelRoot, _textComponent, _camera);
        _navmeshHelper = new NavmeshHelper();
        var keyframeMessageConsumers = new IKeyframeMessageConsumer[]
        {
            _avatarPositionHandler,
            _highlightManager,
            _loadingEffectHandler,
            _textRenderer,
            _navmeshHelper,
        };

        // Initialize IClientStateProducers.
        _inputTrackerXrControllers = new InputTrackerXRControllers();
        _inputTrackerXrPose = new InputTrackerXRPose(_camera.gameObject, _xrLeftController, _xrRightController);
        var clientStateProducers = new IClientStateProducer[]
        {
            _inputTrackerXrControllers,
            _inputTrackerXrPose,
        };

        // Initialize application state.
        _configLoader = new ConfigLoader(_defaultServerLocations);
        _gfxReplayPlayer = new GfxReplayPlayer(keyframeMessageConsumers);
        _networkClient = new NetworkClient(_gfxReplayPlayer, _configLoader, clientStateProducers);
        _onlineStatusDisplayHandler = new OnlineStatusDisplayHandler(_offlineIcon, _camera);
        _replayFileLoader = new ReplayFileLoader(_gfxReplayPlayer, _testKeyframe);
    }

    void Update()
    {
        _gfxReplayPlayer.Update();
        _networkClient.Update();
        _onlineStatusDisplayHandler.Update(_networkClient.IsConnected());
        _replayFileLoader.Update();
    }

    void OnDestroy()
    {
        _networkClient.OnDestroy();
    }

    /// <summary>
    /// Activate the XR Device Simulator.
    /// </summary>
    public static void LaunchXRDeviceSimulator()
    {
#if UNITY_EDITOR
        XRDeviceSimulator xrDeviceSimulator = FindObjectOfType<XRDeviceSimulator>(true);

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
}
