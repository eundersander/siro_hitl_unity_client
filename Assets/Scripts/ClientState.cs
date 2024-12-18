using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Serializable]
// Message that is sent from the client to the server periodically.
public class ClientState
{
    public AvatarData avatar;
    public ButtonInputData input; // TODO: Separate VR and keyboard input.
    public MouseInputData mouse;
    public int? recentServerKeyframeId = null;
}

[Serializable]
public class ArticulatedHandData
{
    // https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#convention-of-hand-joints
    // typedef enum XrHandJointEXT {
    //     XR_HAND_JOINT_PALM_EXT = 0,
    //     XR_HAND_JOINT_WRIST_EXT = 1,
    //     XR_HAND_JOINT_THUMB_METACARPAL_EXT = 2,
    //     XR_HAND_JOINT_THUMB_PROXIMAL_EXT = 3,
    //     XR_HAND_JOINT_THUMB_DISTAL_EXT = 4,
    //     XR_HAND_JOINT_THUMB_TIP_EXT = 5,
    //     XR_HAND_JOINT_INDEX_METACARPAL_EXT = 6,
    //     XR_HAND_JOINT_INDEX_PROXIMAL_EXT = 7,
    //     XR_HAND_JOINT_INDEX_INTERMEDIATE_EXT = 8,
    //     XR_HAND_JOINT_INDEX_DISTAL_EXT = 9,
    //     XR_HAND_JOINT_INDEX_TIP_EXT = 10,
    //     XR_HAND_JOINT_MIDDLE_METACARPAL_EXT = 11,
    //     XR_HAND_JOINT_MIDDLE_PROXIMAL_EXT = 12,
    //     XR_HAND_JOINT_MIDDLE_INTERMEDIATE_EXT = 13,
    //     XR_HAND_JOINT_MIDDLE_DISTAL_EXT = 14,
    //     XR_HAND_JOINT_MIDDLE_TIP_EXT = 15,
    //     XR_HAND_JOINT_RING_METACARPAL_EXT = 16,
    //     XR_HAND_JOINT_RING_PROXIMAL_EXT = 17,
    //     XR_HAND_JOINT_RING_INTERMEDIATE_EXT = 18,
    //     XR_HAND_JOINT_RING_DISTAL_EXT = 19,
    //     XR_HAND_JOINT_RING_TIP_EXT = 20,
    //     XR_HAND_JOINT_LITTLE_METACARPAL_EXT = 21,
    //     XR_HAND_JOINT_LITTLE_PROXIMAL_EXT = 22,
    //     XR_HAND_JOINT_LITTLE_INTERMEDIATE_EXT = 23,
    //     XR_HAND_JOINT_LITTLE_DISTAL_EXT = 24,
    //     XR_HAND_JOINT_LITTLE_TIP_EXT = 25,
    //     XR_HAND_JOINT_MAX_ENUM_EXT = 0x7FFFFFFF
    // } XrHandJointEXT;

    public const int NumHandBones = 26;
    public const int NumFloatsPerPosition = 3;
    public const int NumFloatsPerRotation = 4;
    public float[] positions = new float[NumHandBones * NumFloatsPerPosition];
    public float[] rotations = new float[NumHandBones * NumFloatsPerRotation];

    public void FromGameObjectList(GameObject[] gameObjects)
    {
        if (gameObjects.Length != NumHandBones)
        {
            throw new ArgumentException($"Expected {NumHandBones} game objects, but received {gameObjects.Length}.");
        }

        for (int i = 0; i < gameObjects.Length; i++)
        {
            GameObject gameObject = gameObjects[i];

            // Get position and rotation as float arrays
            float[] position = CoordinateSystem.ToHabitatVector(gameObject.transform.position).ToArray();
            float[] rotation = CoordinateSystem.ToHabitatQuaternion(gameObject.transform.rotation).ToArray();

            // Copy position data into the positions array
            int positionIndex = i * NumFloatsPerPosition;
            positions[positionIndex] = position[0];
            positions[positionIndex + 1] = position[1];
            positions[positionIndex + 2] = position[2];

            // Copy rotation data into the rotations array
            int rotationIndex = i * NumFloatsPerRotation;
            rotations[rotationIndex] = rotation[0];
            rotations[rotationIndex + 1] = rotation[1];
            rotations[rotationIndex + 2] = rotation[2];
            rotations[rotationIndex + 3] = rotation[3];
        } 
    } 
};


[Serializable]
// Contains the avatar head and controller poses.
public class AvatarData
{
    public PoseData root = new PoseData();
    public PoseData[] hands = Enumerable.Range(0, 2).Select(_ => new PoseData()).ToArray();
    public ArticulatedHandData[] articulatedHands = Enumerable.Range(0, 2).Select(_ => new ArticulatedHandData()).ToArray();    
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
        rotation = CoordinateSystem.ToHabitatQuaternion(gameObject.transform.rotation).ToArray();
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
    public float[] screenPosition = new float[2];
}