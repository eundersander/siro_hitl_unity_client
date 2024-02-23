using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTransformHandler : MonoBehaviour, IKeyframeMessageConsumer
{
    [Tooltip("Camera to manipulate upon receiving server updates.")]
    public Camera _camera;

    public void ProcessMessage(Message message)
    {
        if (message.camera?.translation?.Count == 3 && message.camera?.rotation?.Count == 4) {
            Camera.main.transform.position = CoordinateSystem.ToUnityVector(message.camera.translation);
            Camera.main.transform.rotation = CoordinateSystem.ToUnityQuaternion(message.camera.rotation);
        }
    }
}
