using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPositionHandler : MonoBehaviour
{
    public GameObject xrOrigin;

    public GameObject xrCamera;

    public void ProcessKeyframe(KeyframeData keyframe)
    {
        if (!enabled) return;

        var humanoidPosition = keyframe?.message.humanoidPosition;
        if (humanoidPosition != null && humanoidPosition.Count == 3)
        {
            Vector3 newPosition = CoordinateConventionHelper.ToUnityVector(humanoidPosition);

            Vector3 delta = newPosition - xrCamera.transform.position;
            delta = new Vector3(delta.x, 0.0f, delta.z); // Ignore y-axis delta

            xrOrigin.transform.position += delta;
        }
    }
}
