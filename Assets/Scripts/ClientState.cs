using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
// Message that is sent from the client to the server periodically.
public class ClientState
{
    public AvatarData avatar;
    public ButtonInputData input;
    public MouseInputData mouse;
    public Dictionary<string, string> connection_params_dict;
}

[Serializable]
// Contains the avatar head and controller poses.
public class AvatarData
{
    public PoseData root = new PoseData();
    public PoseData[] hands = new PoseData[]
    {
        new PoseData(),
        new PoseData()
    };
}

[Serializable]
// Serializable transform.
public class PoseData
{
    public float[] position = new float[3];
    public float[] rotation = new float[4];

    public void FromGameObject(GameObject gameObject)
    {
        position = CoordinateSystem.ToHabitatVector(gameObject.transform.position).ToArray();
        rotation = CoordinateSystem.ToHabitatQuaternion3DModel(gameObject.transform.rotation).ToArray();
    }
}

[Serializable]
// Collection of buttons that were held, pressed or released since the last client message.
public class ButtonInputData
{
    public List<int> buttonHeld = new List<int>();
    public List<int> buttonUp = new List<int>();
    public List<int> buttonDown = new List<int>();
}

[Serializable]
// Mouse input.
public class MouseInputData
{
    public ButtonInputData buttons = new ButtonInputData();

    public float[] scrollDelta = new float[2];
}
