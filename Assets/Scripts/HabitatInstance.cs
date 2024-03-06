using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a Habitat object instance from gfx-replay.
/// Acts as a placeholder when the object is loading, or has failed to load.
/// </summary>
public class HabitatInstance : MonoBehaviour
{
    /// <summary>
    /// Create a 'HabitatInstance' object and initiates loading.
    /// </summary>
    /// <param name="name">Name of the GameObject (visible from the Editor in Play mode).</param>
    /// <param name="address">Address of the resource to load.</param>
    /// <param name="coordinateFrame">Coordinate frame of the object. See Keyframe.cs.</param>
    /// <returns>Object instance.</returns>
    public static HabitatInstance CreateAndLoad(string name, string address, Frame coordinateFrame)
    {
        var newInstance = new GameObject(name).AddComponent<HabitatInstance>();
        newInstance.Load(address, coordinateFrame);
        return newInstance;
    }

    public void Load(string address, Frame frame)
    {
        // TODO: Asynchronous resource loading.
        GameObject prefab = Resources.Load<GameObject>(address);

        if (prefab == null)
        {
            Debug.LogError($"Unable to load GameObject for '{address}'.");
            return;
        }

        GameObject offsetNode = CreateOffsetNode(frame);
        GameObject instance = Instantiate(prefab);
        offsetNode.transform.SetParent(transform, worldPositionStays: false);
        instance.transform.SetParent(offsetNode.transform, worldPositionStays: false);
    }

    Quaternion ComputeFrameRotationOffset(Frame frame)
    {
        Quaternion unityFrameInv = Quaternion.Inverse(Quaternion.LookRotation(
            Vector3.forward,
            Vector3.up
        ));
        Quaternion habitatFrame = Quaternion.LookRotation(
            frame.front.ToVector3(),
            frame.up.ToVector3()
        );
        return unityFrameInv * habitatFrame;
    }

    // TODO: Optimization: Skip the offset node. Instead, bake the transform into the instance root node.
    GameObject CreateOffsetNode(Frame frame)
    {
        GameObject offsetNode = new GameObject("Offset");
        offsetNode.transform.localRotation = ComputeFrameRotationOffset(frame);
        return offsetNode;
    }
}
