using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Collection of utilities to handle coordinate system transformations between Habitat and Unity.
/// 
/// Habitat: X-Left, Y-Up, Z-Back
/// Unity:   X-Left, Y-Up, Z-Forward
/// </summary>
public static class CoordinateSystem
{
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
    /// </summary>
    public static List<float> ToHabitatQuaternion(Quaternion rotation)
    {
        return new List<float>
        {
            -rotation.w,
            rotation.x,
            rotation.y,
            -rotation.z
        };
    }

    /// <summary>
    /// Create a Unity quaternion from a gfx-replay 'Frame' object.
    /// </summary>
    public static Quaternion ComputeFrameRotationOffset(Frame frame)
    {
        Quaternion unityFrameQuatInv = Quaternion.Inverse(Quaternion.LookRotation(
            Vector3.forward,
            Vector3.up
        ));
        Quaternion habitatFrameQuat = Quaternion.LookRotation(
            frame.front.ToVector3(),
            frame.up.ToVector3()
        );
        return unityFrameQuatInv * habitatFrameQuat;
    }
}

public static class Extensions
{
    public static Vector3 ToVector3(this IList<float> v)
    {
        Assert.IsTrue(v?.Count == 3);
        return new Vector3(v[0], v[1], v[2]);
    }

    public static Quaternion ToQuaternion(this IList<float> q)
    {
        Assert.IsTrue(q?.Count == 4);
        return new Quaternion(q[0], q[1], q[2], q[3]);
    }

    public static float[] ToArray(this Vector3 v)
    {
        return new float[]{v.x, v.y, v.z};
    }
}
