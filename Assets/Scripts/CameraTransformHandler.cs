using UnityEngine;

public class CameraTransformHandler : IKeyframeMessageConsumer
{
    Camera _camera;

    public CameraTransformHandler(Camera camera)
    {
        _camera = camera;
    }

    public void ProcessMessage(Message message)
    {
        if (message.camera?.translation?.Count == 3 && message.camera?.rotation?.Count == 4) {
            _camera.transform.position = CoordinateSystem.ToUnityVector(message.camera.translation);
            _camera.transform.rotation = CoordinateSystem.ToUnityQuaternion(message.camera.rotation);
        }
    }

    public void Update() {}
}
