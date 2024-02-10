using UnityEngine;

/// <summary>
/// Mouse/Keyboard Habitat client application.
/// Intended to be used from web browsers (WebGL).
/// </summary>
[RequireComponent(typeof(HighlightManager))]
[RequireComponent(typeof(StatusDisplayHelper))]
[RequireComponent(typeof(TextRenderer))]
public class AppMouseKeyboard : MonoBehaviour
{
    protected GfxReplayPlayer _gfxReplayPlayer;
    protected NetworkClient _networkClient;

    protected void Main()
    {
        _gfxReplayPlayer = gameObject.AddComponent<GfxReplayPlayer>();
        _networkClient = gameObject.AddComponent<NetworkClient>();

        // If the local replay file loader component is enabled, disable networking.
        // This is done in the Editor.
        var replayFileLoader = gameObject.GetComponent<ReplayFileLoader>();
        if (replayFileLoader.enabled)
        {
            Debug.LogWarning("Replay file loader enabled. Disabling network client.");
            _networkClient.enabled = false;
        }
    }

    void Awake()
    {
        Main();
    }
}
