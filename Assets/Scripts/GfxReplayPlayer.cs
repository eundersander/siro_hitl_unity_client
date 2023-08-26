using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class GfxReplayPlayer : MonoBehaviour
{
    [SerializeField]
    private TextAsset jsonFile; // Drag your JSON file onto this field in the inspector

    private Dictionary<int, GameObject> _instanceDictionary = new Dictionary<int, GameObject>();
    private Dictionary<string, Load> _loadDictionary = new Dictionary<string, Load>();
    private int _nextKeyframeIdx = 0;

    private Quaternion _defaultRotation = Quaternion.Euler(0, 180, 0); //  Quaternion.Euler(0, 180, 0);
    private ReplayData _replayData;

    // todo: better assert that is visible on device
    public static void Assert(bool condition, string message = "Assertion failed!")
    {
        if (!condition)
        {
            Debug.LogError(message);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;  // Stops the play mode in the editor
#endif
        }
    }

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

    public GameObject HandleFrame(GameObject node, Frame frame)
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

    void ProcessKeyframe(KeyframeData keyframe)
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
                    Vector3 translation = new Vector3(
                        update.state.absTransform.translation[0],
                        update.state.absTransform.translation[1],
                        -update.state.absTransform.translation[2]
                    );

                    Quaternion rotation = new Quaternion(
                        update.state.absTransform.rotation[1],
                        -update.state.absTransform.rotation[2],
                        -update.state.absTransform.rotation[3],
                        update.state.absTransform.rotation[0]
                    );

                    rotation = _defaultRotation * rotation;

                    instance.transform.position = translation;
                    instance.transform.rotation = rotation;

                    // temp hack
                    //instance.isStatic = true;

                }
            }
        }
    }

    void Start()
    {
        if (jsonFile == null)
        {
            Debug.LogError("No JSON file provided.");
            return;
        }

        _replayData = JsonUtility.FromJson<ReplayData>(jsonFile.text);

        Assert(_replayData.keyframes.Length > 0);
        _nextKeyframeIdx = 0;
        NextKeyframe();
    }

    private void NextKeyframe()
    {
        if (_nextKeyframeIdx >= _replayData.keyframes.Length)
        {
            return;
        }
        ProcessKeyframe(_replayData.keyframes[_nextKeyframeIdx]);
        Debug.Log($"processed keyframe {_nextKeyframeIdx}");
        _nextKeyframeIdx++;
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            NextKeyframe();
        }
    }

    [Serializable]
    private class ReplayData
    {
        public KeyframeData[] keyframes;
    }

    [Serializable]
    private class KeyframeData
    {
        public Load[] loads;
        public CreationItem[] creations;
        public StateUpdate[] stateUpdates;
    }

    [Serializable]
    public class Load
    {
        public int type;
        public string filepath;
        public Frame frame;
        // public float virtualUnitToMeters;
        // public bool forceFlatShading;
        // public bool splitInstanceMesh;
        // public string shaderTypeToUse;
        // public bool hasSemanticTextures;
    }

    [Serializable]
    public class Frame
    {
        public float[] up;
        public float[] front;
        public float[] origin;
    }

    [Serializable]
    public class CreationItem
    {
        public int instanceKey;
        public Creation creation;
    }

    [Serializable]
    public class Creation
    {
        public string filepath;
        public float[] scale;
    }

    [Serializable]
    public class StateUpdate
    {
        public int instanceKey;
        public StateData state;

        [Serializable]
        public class StateData
        {
            public AbsTransform absTransform;
            public int semanticId;

            [Serializable]
            public class AbsTransform
            {
                public List<float> translation;
                public List<float> rotation;
            }
        }
    }
}
