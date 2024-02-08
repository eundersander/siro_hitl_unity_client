using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

[RequireComponent(typeof(AvatarPositionHandler))]
[RequireComponent(typeof(HighlightManager))]
[RequireComponent(typeof(StatusDisplayHelper))]
[RequireComponent(typeof(TextRenderer))]
[RequireComponent(typeof(NavmeshHelper))]
[RequireComponent(typeof(InputTrackerXRControllers))]
[RequireComponent(typeof(InputTrackerXRPose))]
public class AppVR : App
{
    protected AvatarPositionHandler _avatarPositionHandler;
    protected HighlightManager _highlightManager;
    protected StatusDisplayHelper _statusDisplayHelper;
    protected TextRenderer _textRenderer;
    protected NavmeshHelper _navmeshHelper;
    protected InputTrackerXRControllers _inputTrackerXRControllers;
    protected InputTrackerXRPose _inputTrackerXRPose;

    protected override void Main()
    {
        _avatarPositionHandler = gameObject.GetComponent<AvatarPositionHandler>();
        _highlightManager = gameObject.GetComponent<HighlightManager>();
        _statusDisplayHelper = gameObject.GetComponent<StatusDisplayHelper>();
        _textRenderer = gameObject.GetComponent<TextRenderer>();
        _navmeshHelper = gameObject.GetComponent<NavmeshHelper>();
        _inputTrackerXRControllers = gameObject.GetComponent<InputTrackerXRControllers>();
        _inputTrackerXRPose = gameObject.GetComponent<InputTrackerXRPose>();

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
