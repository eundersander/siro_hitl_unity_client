using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GfxReplayPlayer : MonoBehaviour
{
    [SerializeField]
    private TextAsset jsonFile; // Drag your JSON file onto this field in the inspector

    private Dictionary<int, GameObject> instanceDictionary = new Dictionary<int, GameObject>();

    void Start()
    {
        if (jsonFile == null)
        {
            Debug.LogError("No JSON file provided.");
            return;
        }

        var data = JsonUtility.FromJson<ReplayData>(jsonFile.text);

        Quaternion defaultRotation = Quaternion.Euler(0, 180, 0);

        // Handle Creations
        foreach (var creationItem in data.keyframes[0].creations)
        {
            string resourcePath = creationItem.creation.filepath.Replace(".glb", "");
            GameObject prefab = Resources.Load<GameObject>(resourcePath);

            if (prefab == null)
            {
                Debug.LogError("Unable to load GameObject for " + resourcePath);
                continue;
            }

            GameObject instance = Instantiate(prefab);
            instanceDictionary[creationItem.instanceKey] = instance; // Add to dictionary

            if (creationItem.creation.scale != null)
            {
                instance.transform.localScale = new Vector3(creationItem.creation.scale[0], creationItem.creation.scale[1], creationItem.creation.scale[2]);
            }

            instance.transform.rotation = defaultRotation;
        }

        // Handle State Updates
        foreach (var update in data.keyframes[0].stateUpdates)
        {
            if (instanceDictionary.ContainsKey(update.instanceKey))
            {
                GameObject instance = instanceDictionary[update.instanceKey];

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

                rotation *= defaultRotation;

                instance.transform.position = translation;
                instance.transform.rotation = rotation;

                // temp hack
                //instance.isStatic = true;

            }
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
        public CreationItem[] creations;
        public StateUpdate[] stateUpdates;
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
