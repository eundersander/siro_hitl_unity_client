using UnityEngine;

/// <summary>
/// Mouse/Keyboard Habitat client application.
/// Intended to be used from web browsers (WebGL).
/// </summary>
public class AppMouseKeyboard : MonoBehaviour
{
    [Header("Config Defaults")]
    [Tooltip("Config defaults are used directly when running in the Editor. On device, they are used to populate config.txt at Android/data/com.meta.siro_hitl_vr_client/files/. This file persists and can be edited between runs, e.g. by connecting via USB to a laptop.")]
    [SerializeField] private bool _mouseoverForTooltip;  // dummy member so we can add tooltip in Inspector pane

    [Tooltip("Default server locations.")]
    [SerializeField] private string[] _defaultServerLocations = new string[]{ "127.0.01:8888" };

    [Tooltip("If specified, the keyframe will be played upon launching the client.")]
    [SerializeField] TextAsset _testKeyframe;

    [Header("Rendering Configuration")]
    [Tooltip("Main camera.")]
    [SerializeField] private Camera _camera;

    [Tooltip("Global application configuration.")]
    [SerializeField] private AppConfig _appConfig;

    [Tooltip("Icon that is displayed when connection is lost.")]
    [SerializeField] private GameObject _offlineIcon;

    // IKeyframeMessageConsumers
    ServerKeyframeIdHandler _serverKeyframeIdHandler;
    HighlightManager _highlightManager;
    LoadingEffectHandler _loadingEffectHandler;
    CameraTransformHandler _cameraTransformHandler;

    // IClientStateProducers
    InputTrackerMouse _inputTrackerMouse;
    InputTrackerKeyboard _inputTrackerKeyboard;

    // Application state
    ConfigLoader _configLoader;
    GfxReplayPlayer _gfxReplayPlayer;
    NetworkClient _networkClient;
    OnlineStatusDisplayHandler _onlineStatusDisplayHandler;
    ReplayFileLoader _replayFileLoader;

    protected void Awake()
    {
        // Initialize IKeyframeMessageConsumers.
        _serverKeyframeIdHandler = new ServerKeyframeIdHandler();
        _highlightManager = new HighlightManager(_appConfig, _camera);
        _loadingEffectHandler = new LoadingEffectHandler();
        _cameraTransformHandler = new CameraTransformHandler(_camera);
        var keyframeMessageConsumers = new IKeyframeMessageConsumer[]
        {
            _serverKeyframeIdHandler,
            _highlightManager,
            _loadingEffectHandler,
            _cameraTransformHandler,
        };

        // Initialize IClientStateProducers.
        _inputTrackerMouse = new InputTrackerMouse();
        _inputTrackerKeyboard = new InputTrackerKeyboard();
        var clientStateProducers = new IClientStateProducer[]
        {
            _inputTrackerMouse,
            _inputTrackerKeyboard,
        };

        // Initialize application state.
        _configLoader = new ConfigLoader(_defaultServerLocations);
        _gfxReplayPlayer = new GfxReplayPlayer(keyframeMessageConsumers);
        _networkClient = new NetworkClient(_gfxReplayPlayer, _configLoader, clientStateProducers, _serverKeyframeIdHandler);
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
}
