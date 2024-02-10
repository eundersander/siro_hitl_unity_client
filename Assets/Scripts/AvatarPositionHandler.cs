using UnityEngine;

public class AvatarPositionHandler : MonoBehaviour, IKeyframeMessageConsumer
{
    [Tooltip("GameObject containing the XR camera.")]
    public GameObject xrCameraNode;

    [Tooltip("Parent GameObject to the XR camera and controllers to be displaced by this script.")]
    public GameObject xrOriginNode;

    public void ProcessMessage(Message message)
    {
        if (!enabled) return;

        var avatarPosition = message.teleportAvatarBasePosition;
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
