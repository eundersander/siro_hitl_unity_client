using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GfxReplayPlayer : IUpdatable
{
    public struct MovementData
    {
        public Vector3 startPosition;    // Starting position of the interpolation
        public Quaternion startRotation; // Starting rotation of the interpolation

        public Vector3 endPosition;      // Target position of the interpolation
        public Quaternion endRotation;   // Target rotation of the interpolation

        public float startTime;          // Time when this movement data was created or updated
    }

    const bool USE_KEYFRAME_INTERPOLATION = true;

    private Dictionary<int, GfxReplayInstance> _instanceDictionary = new();
    private Dictionary<string, Load> _loadDictionary = new();
    private Dictionary<int, MovementData> _movementData = new();
    Dictionary<int, GfxReplaySkinnedMesh> _skinnedMeshes = new();
    CoroutineContainer _coroutines;
    IKeyframeMessageConsumer[] _messageConsumers;
    private float _keyframeInterval = 0.1f;  // assume 10Hz, but see also SetKeyframeRate

    public GfxReplayPlayer(IKeyframeMessageConsumer[] messageConsumers)
    {
        _messageConsumers = messageConsumers;
        _coroutines = CoroutineContainer.Create("GfxReplayPlayer");
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

    private void ProcessStateUpdatesImmediate(KeyframeData keyframe)
    {
        // Handle State Updates
        if (keyframe.stateUpdates != null)
        {
            foreach (var update in keyframe.stateUpdates)
            {
                if (_instanceDictionary.ContainsKey(update.instanceKey))
                {
                    Transform instance = _instanceDictionary[update.instanceKey].transform;

                    Vector3 translation = CoordinateSystem.ToUnityVector(update.state.absTransform.translation);
                    Quaternion rotation = CoordinateSystem.ToUnityQuaternion(update.state.absTransform.rotation);

                    instance.position = translation;
                    instance.rotation = rotation;
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
                    GameObject instance = _instanceDictionary[update.instanceKey].gameObject;

                    Vector3 newTranslation = CoordinateSystem.ToUnityVector(update.state.absTransform.translation);
                    Quaternion newRotation = CoordinateSystem.ToUnityQuaternion(update.state.absTransform.rotation);

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
        if (USE_KEYFRAME_INTERPOLATION)
        {
            ProcessStateUpdatesForInterpolation(keyframe);
        }
        // Suppress 'Unreachable Code' warning.
        #pragma warning disable 0162 
        else
        {
            ProcessStateUpdatesImmediate(keyframe);
        }
        #pragma warning restore 0162

    }

    private void UpdateForInterpolatedStateUpdates()
    {
        // Use a list to keep track of keys to remove after processing
        List<int> keysToRemove = new List<int>();

        foreach (var kvp in _instanceDictionary)
        {
            int instanceKey = kvp.Key;
            Transform instance = kvp.Value.transform;

            if (_movementData.ContainsKey(instanceKey))
            {
                var data = _movementData[instanceKey];
                float t = (Time.time - data.startTime) / _keyframeInterval;

                if (t < 1.0f)
                {
                    instance.position = Vector3.Lerp(data.startPosition, data.endPosition, t);
                    instance.rotation = Quaternion.Slerp(data.startRotation, data.endRotation, t);
                }
                else
                {
                    instance.position = data.endPosition;
                    instance.rotation = data.endRotation;

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

    public void Update()
    {
        foreach (var messageConsumer in _messageConsumers)
        {
            messageConsumer.Update();
        }

        if (USE_KEYFRAME_INTERPOLATION)
        {
            UpdateForInterpolatedStateUpdates();
        }
    }

    public void ProcessKeyframe(KeyframeData keyframe)
    {
        // Handle messages
        if (keyframe.message != null)
        {
            foreach (var messageConsumer in _messageConsumers)
            {
                messageConsumer.ProcessMessage(keyframe.message);
            }
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
                var instance = GfxReplayInstance.CreateAndLoad(creationItem.instanceKey.ToString(), resourcePath, load.frame);

                if (creationItem.creation.scale != null)
                {
                    instance.transform.localScale = creationItem.creation.scale.ToVector3();
                }

                int rigId = creationItem.creation.rigId;
                if (rigId != Constants.ID_UNDEFINED)
                {
                    var skinnedMesh = instance.gameObject.AddComponent<GfxReplaySkinnedMesh>();
                    skinnedMesh.rigId = rigId;
                    _skinnedMeshes[rigId] = skinnedMesh;
                }

                _instanceDictionary[creationItem.instanceKey] = instance;
            }
            Debug.Log($"Processed {keyframe.creations.Length} creations!");
        }

        if (keyframe.rigCreations != null)
        {
            foreach (var rigCreation in keyframe.rigCreations)
            {
                int rigId = rigCreation.id;
                if (_skinnedMeshes.TryGetValue(rigId, out GfxReplaySkinnedMesh skinnedMesh))
                {
                    skinnedMesh.configureRigInstance(rigCreation.boneNames);
                }
            }
        }

        if (keyframe.rigUpdates != null)
        {
            foreach (var rigUpdate in keyframe.rigUpdates)
            {
                int rigId = rigUpdate.id;
                if (_skinnedMeshes.TryGetValue(rigId, out GfxReplaySkinnedMesh skinnedMesh))
                {
                    skinnedMesh.setPose(rigUpdate.pose);
                }
            }
        }

        ProcessStateUpdates(keyframe);

        // Handle Deletions
        if (keyframe.deletions != null)
        {
            foreach (var key in keyframe.deletions)
            {
                if (_instanceDictionary.TryGetValue(key, out GfxReplayInstance obj))
                {
                    GfxReplaySkinnedMesh skinnedMesh = obj.GetComponent<GfxReplaySkinnedMesh>();
                    if (skinnedMesh != null)
                    {
                        _skinnedMeshes.Remove(skinnedMesh.rigId);
                    }
                }
                
                GameObject.Destroy(_instanceDictionary[key].gameObject);
            }
            _coroutines.StartCoroutine(ReleaseUnusedMemory(
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
        if (keyframe.message != null)
        {
            foreach (var messageConsumer in _messageConsumers)
            {
                messageConsumer.PostProcessMessage(keyframe.message);
            }
        }
    }

    public void DeleteAllInstancesFromKeyframes()
    {
        foreach (var kvp in _instanceDictionary)
        {
            GameObject.Destroy(kvp.Value.gameObject);
        }
        _coroutines.StartCoroutine(ReleaseUnusedMemory());
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
