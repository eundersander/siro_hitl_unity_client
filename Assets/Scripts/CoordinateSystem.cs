using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Collection of utilities to handle coordinate system transformations between Habitat and Unity.
/// 
/// Habitat: X-Left, Y-Up, Z-Back
/// Unity:   X-Left, Y-Up, Z-Forward
/// 
/// Beware that because of the change in handedness, the Unity asset pipeline bakes the z-axis flip into 3D models.
/// See ToUnityQuaternion3DModel() and ToHabitatQuaternion3DModel().
/// </summary>
public static class CoordinateSystem
{
    private static readonly Quaternion _3dModelRotationCorrection = Quaternion.Euler(0, 180, 0);
    private static readonly Quaternion _inv3dModelRotationCorrection = Quaternion.Inverse(_3dModelRotationCorrection);

    /// <summary>
    /// Convert a Habitat vector into Unity's coordinate system.
    /// </summary>
    public static Vector3 ToUnityVector(float x, float y, float z)
    {
        return new Vector3(
            x,
            y,
            -z
        );
    }

    /// <summary>
    /// Convert a Habitat vector into Unity's coordinate system.
    /// </summary>
    public static Vector3 ToUnityVector(IList<float> translation)
    {
        return new Vector3(
            translation[0],
            translation[1],
            -translation[2]
        );
    }
    
    /// <summary>
    /// Convert a Habitat quaternion into Unity's coordinate system.
    /// Beware: the Unity asset pipeline bakes transforms into 3D models. Use ToUnityQuaternion3DModel() to rotate models.
    /// </summary>
    public static Quaternion ToUnityQuaternion(IList<float> rotation)
    {
        return new Quaternion(
            rotation[1],
            rotation[2],
            -rotation[3],
            -rotation[0]
        );
    }

    /// <summary>
    /// Convert a Habitat quaternion into Unity's coordinate system, taking into account 3D model baked transformations.
    /// 3D models are pre-processed to handle the change in handedness (avoids negative scaling on the z-axis).
    /// </summary>
    public static Quaternion ToUnityQuaternion3DModel(IList<float> rotation)
    {
        Quaternion newRot = new Quaternion(
            rotation[1],
            -rotation[2],
            -rotation[3],
            rotation[0]
        );

        newRot = _3dModelRotationCorrection * newRot;
        return newRot;
    }

    /// <summary>
    /// Convert a Unity vector into Habitat's coordinate system.
    /// </summary>
    public static List<float> ToHabitatVector(Vector3 translation)
    {
        return new List<float>
        {
            translation.x,
            translation.y,
            -translation.z
        };
    }

    /// <summary>
    /// Convert a Unity quaternion into Habitat's coordinate system.
    /// Beware: the Unity asset pipeline bakes transforms into 3D models. Use ToHabitatQuaternion3DModel() to rotate models.
    /// </summary>
    public static List<float> ToHabitatQuaternion(Quaternion rotation)
    {
        Quaternion convertedRotation = _inv3dModelRotationCorrection * rotation;

        return new List<float>
        {
            -convertedRotation.w,
            convertedRotation.x,
            convertedRotation.y,
            -convertedRotation.z
        };
    }

    /// <summary>
    /// Convert a Unity quaternion into Habitat's coordinate system, taking into account 3D model baked transformations.
    /// 3D models are pre-processed to handle the change in handedness (avoids negative scaling on the z-axis).
    /// </summary>
    public static List<float> ToHabitatQuaternion3DModel(Quaternion rotation)
    {
        Quaternion convertedRotation = _inv3dModelRotationCorrection * rotation;

        return new List<float>
        {
            convertedRotation.w,
            convertedRotation.x,
            -convertedRotation.y,
            -convertedRotation.z
        };
    }
}
