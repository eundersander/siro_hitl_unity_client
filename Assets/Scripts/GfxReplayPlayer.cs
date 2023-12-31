using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GfxReplayPlayer : MonoBehaviour
{
    public struct MovementData
    {
        public Vector3 startPosition;    // Starting position of the interpolation
        public Quaternion startRotation; // Starting rotation of the interpolation

        public Vector3 endPosition;      // Target position of the interpolation
        public Quaternion endRotation;   // Target rotation of the interpolation

        public float startTime;          // Time when this movement data was created or updated
    }

    private Dictionary<int, GameObject> _instanceDictionary = new Dictionary<int, GameObject>();
    private Dictionary<string, Load> _loadDictionary = new Dictionary<string, Load>();

    HighlightManager _highlightManager;
    AvatarPositionHandler _avatarPositionHandler;
    StatusDisplayHelper _statusDisplayHelper;
    NavmeshHelper _navmeshHelper;
    TextRenderer _textRenderer;

    private Dictionary<int, MovementData> _movementData = new Dictionary<int, MovementData>();
    private float _keyframeInterval = 0.1f;  // assume 10Hz, but see also SetKeyframeRate
    const bool _useKeyframeInterpolation = true;

    void Awake()
    {
        _highlightManager = GetComponent<HighlightManager>();
        if (_highlightManager == null)
        {
            Debug.LogWarning($"Highlight manager missing from '{name}'. Object highlights will be ignored.");
        }
        _avatarPositionHandler = GetComponent<AvatarPositionHandler>();
        if (_avatarPositionHandler == null)
        {
            Debug.LogWarning($"Avatar position handler missing from '{name}'. Avatar position updates will be ignored.");
        }
        _statusDisplayHelper = GetComponent<StatusDisplayHelper>();
        if (_statusDisplayHelper == null)
        {
            Debug.LogWarning($"Status display helper missing from '{name}'. Status updates will be ignored.");
        }
        _navmeshHelper = GameObject.FindObjectOfType<NavmeshHelper>();
        if (!_navmeshHelper)
        {
            Debug.LogWarning($"Couldn't find a NavmeshHelper. Navmesh updates will be ignored.");
        }
        _textRenderer = GetComponent<TextRenderer>();
        if (_textRenderer == null)
        {
            Debug.LogWarning($"Text renderer missing from '{name}'. Text messages won't be displayed.");
        }
    }

    public void SetKeyframeRate(float rate)
    {
        Assert.IsTrue(rate > 0.0F);
        float adjustedRate = Mathf.Clamp(rate, 10, 30);
        _keyframeInterval = 1.0F / adjustedRate;
    }

    // simplify "path/abc/../to/file" to "path/to/file"
    static string SimplifyRelativePath(string path)
    {
        string[] parts = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        var simplifiedParts = new List<string>();

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


    private void ProcessStateUpdatesImmediate(KeyframeData keyframe)
    {
        // Handle State Updates
        if (keyframe.stateUpdates != null)
        {
            foreach (var update in keyframe.stateUpdates)
            {
                if (_instanceDictionary.ContainsKey(update.instanceKey))
                {
                    GameObject instance = _instanceDictionary[update.instanceKey];

                    Vector3 translation = CoordinateConventionHelper.ToUnityVector(update.state.absTransform.translation);
                    Quaternion rotation = CoordinateConventionHelper.ToUnityQuaternion(update.state.absTransform.rotation);

                    instance.transform.position = translation;
                    instance.transform.rotation = rotation;
                }
            }
        }
    }

    private void ProcessStateUpdatesForInterpolation(KeyframeData keyframe)
    {
        if (keyframe.stateUpdates != null)
        {
            foreach (var update in keyframe.stateUpdates)
            {
                if (_instanceDictionary.ContainsKey(update.instanceKey))
                {
                    GameObject instance = _instanceDictionary[update.instanceKey];

                    Vector3 newTranslation = CoordinateConventionHelper.ToUnityVector(update.state.absTransform.translation);
                    Quaternion newRotation = CoordinateConventionHelper.ToUnityQuaternion(update.state.absTransform.rotation);

                    // Check if the instance is at the origin
                    if (instance.transform.position == Vector3.zero)
                    {
                        // Snap to the new position and rotation
                        instance.transform.position = newTranslation;
                        instance.transform.rotation = newRotation;
                    }
                    else
                    {
                        // Set up the interpolation
                        if (!_movementData.ContainsKey(update.instanceKey))
                        {
                            _movementData[update.instanceKey] = new MovementData
                            {
                                startPosition = instance.transform.position,
                                startRotation = instance.transform.rotation,
                                endPosition = newTranslation,
                                endRotation = newRotation,
                                startTime = Time.time
                            };
                        }
                        else
                        {
                            // Update the existing MovementData
                            var data = _movementData[update.instanceKey];
                            data.startPosition = instance.transform.position;
                            data.startRotation = instance.transform.rotation;
                            data.endPosition = newTranslation;
                            data.endRotation = newRotation;
                            data.startTime = Time.time;

                            _movementData[update.instanceKey] = data;
                        }
                    }
                }
            }
        }
    }

    private void ProcessStateUpdates(KeyframeData keyframe)
    {
        if (_useKeyframeInterpolation)
        {
            ProcessStateUpdatesForInterpolation(keyframe);
        } else
        {
            ProcessStateUpdatesImmediate(keyframe);
        }

    }

    private void UpdateForInterpolatedStateUpdates()
    {
        // Use a list to keep track of keys to remove after processing
        List<int> keysToRemove = new List<int>();

        foreach (var kvp in _instanceDictionary)
        {
            int instanceKey = kvp.Key;
            GameObject instance = kvp.Value;

            if (_movementData.ContainsKey(instanceKey))
            {
                var data = _movementData[instanceKey];
                float t = (Time.time - data.startTime) / _keyframeInterval;

                if (t < 1.0f)
                {
                    instance.transform.position = Vector3.Lerp(data.startPosition, data.endPosition, t);
                    instance.transform.rotation = Quaternion.Slerp(data.startRotation, data.endRotation, t);
                }
                else
                {
                    instance.transform.position = data.endPosition;
                    instance.transform.rotation = data.endRotation;

                    // Mark this key for removal
                    keysToRemove.Add(instanceKey);
                }
            }
        }

        // Remove processed keys from _movementData
        foreach (var key in keysToRemove)
        {
            _movementData.Remove(key);
        }

    }
    private void Update()
    {
        if (_useKeyframeInterpolation)
        {
            UpdateForInterpolatedStateUpdates();
        }
    }

    private void ProcessKeyframeMessage(Message message)
    {
        if (_navmeshHelper != null && message.navmeshVertices != null && message.navmeshVertices.Count > 0)
        {
            if (message.navmeshVertices.Count % 9 != 0)
            {
                Debug.LogError($"Ignoring keyframe.message.navmeshVertices with Count == {message.navmeshVertices.Count}. Length should be a multiple of 9.");
            }
            else
            {
                // convert to Vector3[]
                Vector3[] vectorArray = new Vector3[message.navmeshVertices.Count / 3];
                for (int i = 0; i < message.navmeshVertices.Count; i += 3)
                {
                    vectorArray[i / 3] = CoordinateConventionHelper.ToUnityVector(message.navmeshVertices[i], message.navmeshVertices[i + 1], message.navmeshVertices[i + 2]);
                }

                // it's too error-prone to expect the server to know the
                // correct winding order for Unity raycasts, so let's do
                // double-sided.
                bool doDoublesided = true;
                _navmeshHelper.UpdateNavmesh(vectorArray, doDoublesided);
            }
        }
        if (_textRenderer != null)
        {
            _textRenderer.SetText(message.textMessage);
        }
        
    }

    public void ProcessKeyframe(KeyframeData keyframe)
    {
        // Handle messages
        if (_highlightManager)
        {
            _highlightManager.ProcessKeyframe(keyframe);
        }
        if (_avatarPositionHandler)
        {
            _avatarPositionHandler.ProcessKeyframe(keyframe);
        }
        if (_statusDisplayHelper != null && keyframe.message != null && keyframe.message.sceneChanged)
        {
            _statusDisplayHelper.OnSceneChangeBegin();
        }
        if (keyframe.message != null)
        {
            ProcessKeyframeMessage(keyframe.message);
        }

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

        ProcessStateUpdates(keyframe);

        // Handle Deletions
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
            StartCoroutine(ReleaseUnusedMemory(
                // Wait for memory clean-up to be finished before executing KeyframePostUpdate()
                () => {KeyframePostUpdate(keyframe);})
            );
            Debug.Log($"Processed {keyframe.deletions.Length} deletions!");
        }
        else
        {
            KeyframePostUpdate(keyframe);
        }
    }

    void KeyframePostUpdate(KeyframeData keyframe)
    {
        if (_statusDisplayHelper != null && keyframe.message != null && keyframe.message.sceneChanged)
        {
            _statusDisplayHelper.OnSceneChangeEnd();
        }
    }

    public void DeleteAllInstancesFromKeyframes()
    {
        foreach (var kvp in _instanceDictionary)
        {
            Destroy(kvp.Value);
        }
        StartCoroutine(ReleaseUnusedMemory());
        Debug.Log($"Deleted all {_instanceDictionary.Count} instances!");
        _instanceDictionary.Clear();
    }

    /// <summary>
    /// Unloads the unused resources from GPU and CPU memory.
    /// This is normally done automatically when changing scene.
    /// We must call this manually to avoid leaks because we are never changing scene.
    /// This is a slow operation - use the callback to execute code after this is done.
    /// </summary>
    /// <param name="callback">Code to execute when this is done.</param>
    /// <returns></returns>
    IEnumerator ReleaseUnusedMemory(Action callback = null)
    {
        // Wait for objects to be destroyed.
        yield return new WaitForEndOfFrame();

        // Unload unused assets.
        var asyncOp = Resources.UnloadUnusedAssets();

        // Wait for the operation to be done.
        while (!asyncOp.isDone)
        {
            yield return null;
        }

        // Invoke callback.
        if (callback != null)
        {
            callback.Invoke();
        }
    }
}
