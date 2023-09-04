using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class GfxReplayPlayer : MonoBehaviour
{

    private Dictionary<int, GameObject> _instanceDictionary = new Dictionary<int, GameObject>();
    private Dictionary<string, Load> _loadDictionary = new Dictionary<string, Load>();

    private Quaternion _defaultRotation = Quaternion.Euler(0, 180, 0); //  Quaternion.Euler(0, 180, 0);

    // simplify "path/abc/../to/file" to "path/to/file"
    static string SimplifyRelativePath(string path)
    {
        string[] parts = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        var simplifiedParts = new System.Collections.Generic.List<string>();

        foreach (var part in parts)
        {
            if (part == ".." && simplifiedParts.Count > 0)
            {
                simplifiedParts.RemoveAt(simplifiedParts.Count - 1);  // Remove the previous directory
            }
            else if (part != "." && part != "..")
            {
                simplifiedParts.Add(part);
            }
        }

        return string.Join("/", simplifiedParts);
    }

    static string getResourcePath(string sourceFilepath)
    {
        return SimplifyRelativePath(sourceFilepath).Replace(".glb", "");
    }

    GameObject HandleFrame(GameObject node, Frame frame)
    {
        if (frame.up[0] == 0 && frame.up[1] == 1 && frame.up[2] == 0)
        {
            // optimization todo: if node.transform is identity, no need to add parent here
            GameObject newRootNode = new GameObject(node.name + "_parent");
            node.transform.parent = newRootNode.transform;
            return newRootNode;
        }
        else if (frame.up[0] == 0 && frame.up[1] == 0 && frame.up[2] == 1)
        {
            // Rotate 90 degrees about x axis
            node.transform.Rotate(-Vector3.right * 90, Space.Self);

            GameObject newRootNode = new GameObject(node.name + "_parent");
            node.transform.parent = newRootNode.transform;
            return newRootNode;
        }
        else
        {
            Debug.LogError($"Unexpected value for frame.up: {frame.up[0]}, {frame.up[1]}, {frame.up[2]}");
            return null;
        }
    }

    public void ProcessKeyframe(KeyframeData keyframe)
    {
        // Handle Loads
        if (keyframe.loads != null)
        {
            foreach (var load in keyframe.loads)
            {
                _loadDictionary[load.filepath] = load;
            }
        }

        // Handle Creations
        if (keyframe.creations != null)
        {
            foreach (var creationItem in keyframe.creations)
            {
                var source = creationItem.creation.filepath;
                if (!_loadDictionary.TryGetValue(source, out Load load))
                {
                    Debug.LogError("Unable to find loads entry for " + source);
                    continue;
                }

                string resourcePath = getResourcePath(source);
                GameObject prefab = Resources.Load<GameObject>(resourcePath);

                if (prefab == null)
                {
                    Debug.LogError("Unable to load GameObject for " + resourcePath);
                    continue;
                }

                GameObject instance = Instantiate(prefab);

                if (creationItem.creation.scale != null)
                {
                    instance.transform.localScale = new Vector3(creationItem.creation.scale[0], creationItem.creation.scale[1], creationItem.creation.scale[2]);
                }

                instance = HandleFrame(instance, load.frame);

                _instanceDictionary[creationItem.instanceKey] = instance;
            }
            Debug.Log($"Processed {keyframe.creations.Length} creations!");
        }

        // Handle State Updates
        if (keyframe.stateUpdates != null)
        {
            foreach (var update in keyframe.stateUpdates)
            {
                if (_instanceDictionary.ContainsKey(update.instanceKey))
                {
                    GameObject instance = _instanceDictionary[update.instanceKey];

                    // Note various negations to adjust handedness and coordinate
                    // convention.
                    // Extract translation and rotation from the update state
                    //Vector3 translation = new Vector3(
                    //    update.state.absTransform.translation[0],
                    //    update.state.absTransform.translation[1],
                    //    -update.state.absTransform.translation[2]
                    //);

                    Vector3 translation = CoordinateConventionHelper.ToUnityVector(update.state.absTransform.translation);

                    //Quaternion rotation = new Quaternion(
                    //    update.state.absTransform.rotation[1],
                    //    -update.state.absTransform.rotation[2],
                    //    -update.state.absTransform.rotation[3],
                    //    update.state.absTransform.rotation[0]
                    //);
                    // rotation = _defaultRotation * rotation;
                    Quaternion rotation = CoordinateConventionHelper.ToUnityQuaternion(update.state.absTransform.rotation);


                    instance.transform.position = translation;
                    instance.transform.rotation = rotation;

                    // temp hack
                    //instance.isStatic = true;

                }
            }
        }

        if (keyframe.deletions != null)
        {
            foreach (var key in keyframe.deletions)
            {
                if (_instanceDictionary.ContainsKey(key))
                {
                    Destroy(_instanceDictionary[key]);
                    _instanceDictionary.Remove(key);
                }
            }
            Debug.Log($"Processed {keyframe.deletions.Length} deletions!");
        }
    }

    public void DeleteAllInstancesFromKeyframes()
    {
        foreach (var kvp in _instanceDictionary)
        {
            Destroy(kvp.Value);
        }
        Debug.Log($"Deleted all {_instanceDictionary.Count} instances!");
        _instanceDictionary.Clear();
    }
}
