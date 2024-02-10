using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Collection of utilities to handle coordinate system transformations between Habitat and Unity.
/// 
/// Habitat: X-Left, Y-Up, Z-Back
/// Unity:   X-Left, Y-Up, Z-Forward
/// </summary>
public static class CoordinateSystem
{
    private static Quaternion _defaultRotation = Quaternion.Euler(0, 180, 0);
    private static Quaternion _invDefaultRotation = Quaternion.Inverse(_defaultRotation);

    public static Vector3 ToUnityVector(float x, float y, float z)
    {
        return new Vector3(
            x,
            y,
            -z
        );
    }

    public static Vector3 ToUnityVector(List<float> translation)
    {
        return new Vector3(
            translation[0],
            translation[1],
            -translation[2]
        );
    }

    public static Quaternion ToUnityQuaternion(List<float> rotation)
    {
        Quaternion newRot = new Quaternion(
            rotation[1],
            -rotation[2],
            -rotation[3],
            rotation[0]
        );

        newRot = _defaultRotation * newRot;
        return newRot;
    }

    public static List<float> ToHabitatVector(Vector3 translation)
    {
        return new List<float>
        {
            translation.x,
            translation.y,
            -translation.z
        };
    }

    public static List<float> ToHabitatQuaternion(Quaternion rotation)
    {
        Quaternion convertedRotation = _invDefaultRotation * rotation;

        return new List<float>
        {
            convertedRotation.w,
            convertedRotation.x,
            -convertedRotation.y,
            -convertedRotation.z
        };
    }
}
