using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTransformConsumer : MessageConsumer
{
    [Tooltip("Camera to manipulate upon receiving server updates.")]
    public Camera _camera;

    public override void ProcessMessage(Message message)
    {
        if (message.camera?.translation?.Count == 3 && message.camera?.rotation?.Count == 4) {
            Camera.main.transform.position = CoordinateSystem.ToUnityVector(message.camera.translation);
            Camera.main.transform.rotation = CoordinateSystem.ToUnityQuaternionBase(message.camera.rotation);
        }
    }
}
