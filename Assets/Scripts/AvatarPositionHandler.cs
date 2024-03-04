using UnityEngine;

public class AvatarPositionHandler : IKeyframeMessageConsumer
{
    private GameObject _xrCamera;
    private GameObject _xrOrigin;

    public AvatarPositionHandler(GameObject xrCamera, GameObject xrOrigin)
    {
        _xrCamera = xrCamera;
        _xrOrigin = xrOrigin;
    }

    public void ProcessMessage(Message message)
    {
        var avatarPosition = message.teleportAvatarBasePosition;
        if (avatarPosition != null && avatarPosition.Count == 3)
        {
            Vector3 newPosition = CoordinateSystem.ToUnityVector(avatarPosition);

            Vector3 delta = newPosition - _xrCamera.transform.position;

            // TODO: Handle y-axis changes so that multi-story scenes work.
            delta = new Vector3(delta.x, 0.0f, delta.z); // Ignore y-axis delta

            _xrOrigin.transform.position += delta;
        }
    }

    public void Update() {}
}
