using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

/// <summary>
/// VR Habitat client application.
/// Intended to be deployed to Quest devices (Android).
/// </summary>
[RequireComponent(typeof(AvatarPositionHandler))]
[RequireComponent(typeof(HighlightManager))]
[RequireComponent(typeof(StatusDisplayHelper))]
[RequireComponent(typeof(TextRenderer))]
[RequireComponent(typeof(NavmeshHelper))]
[RequireComponent(typeof(InputTrackerXRControllers))]
[RequireComponent(typeof(InputTrackerXRPose))]
public class AppVR : App
{
    protected override void Main()
    {
        base.Main();
    }

    void Awake()
    {
        Main();
        LaunchXRDeviceSimulator();
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
