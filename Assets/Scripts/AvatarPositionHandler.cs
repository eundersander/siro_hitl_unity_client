using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarPositionHandler : MonoBehaviour
{
    [Tooltip("GameObject containing the XR camera.")]
    public GameObject xrCameraNode;

    [Tooltip("Parent GameObject to the XR camera and controllers to be displaced by this script.")]
    public GameObject xrOriginNode;

    public void ProcessKeyframe(KeyframeData keyframe)
    {
        if (!enabled) return;

        var avatarPosition = keyframe?.message.teleportAvatarBasePosition;
        if (avatarPosition != null && avatarPosition.Count == 3)
        {
            Vector3 newPosition = CoordinateSystem.ToUnityVector(avatarPosition);

            Vector3 delta = newPosition - xrCameraNode.transform.position;

            // TODO: Handle y-axis changes so that multi-story scenes work.
            delta = new Vector3(delta.x, 0.0f, delta.z); // Ignore y-axis delta

            xrOriginNode.transform.position += delta;
        }
    }
}
