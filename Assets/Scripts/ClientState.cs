using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ClientState
{
    public AvatarData avatar;
    public ButtonInputData input;
}

[Serializable]
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
public class PoseData
{
    public float[] position = new float[3];
    public float[] rotation = new float[4];

    public void FromGameObject(GameObject gameObject)
    {
        position = CoordinateSystem.ToHabitatVector(gameObject.transform.position).ToArray();
        rotation = CoordinateSystem.ToHabitatQuaternion(gameObject.transform.rotation).ToArray();
    }
}

[Serializable]
public class ButtonInputData
{
    public List<int> buttonHeld = new List<int>();
    public List<int> buttonUp = new List<int>();
    public List<int> buttonDown = new List<int>();
}
