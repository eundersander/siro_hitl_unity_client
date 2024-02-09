using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

/// <summary>
/// VR Habitat client application.
/// Intended to be deployed to Quest devices (Android).
/// </summary>
[RequireComponent(typeof(ConfigLoader))]
[RequireComponent(typeof(AvatarPositionHandler))]
[RequireComponent(typeof(HighlightManager))]
[RequireComponent(typeof(StatusDisplayHelper))]
[RequireComponent(typeof(TextRenderer))]
[RequireComponent(typeof(NavmeshHelper))]
[RequireComponent(typeof(InputTrackerXRControllers))]
[RequireComponent(typeof(InputTrackerXRPose))]
public class AppVR : MonoBehaviour
{
    protected GfxReplayPlayer _gfxReplayPlayer;
    protected NetworkClient _networkClient;
    protected CollisionFloor _collisionFloor;

    void Main()
    {
        LaunchXRDeviceSimulator();

        _gfxReplayPlayer = gameObject.AddComponent<GfxReplayPlayer>();
        _networkClient = gameObject.AddComponent<NetworkClient>();
        _collisionFloor = new CollisionFloor();

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
