using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

public static class XRUtils
{
#if UNITY_EDITOR
    /// <summary>
    /// Activate the XR Device Simulator.
    /// </summary>
    public static void LaunchXRDeviceSimulator()
    {
        XRDeviceSimulator xrDeviceSimulator = GameObject.FindObjectOfType<XRDeviceSimulator>(true);

        if (xrDeviceSimulator != null)
        {
            xrDeviceSimulator.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("XR Device Simulator not found in the scene!");
        }
    }
#endif
}
