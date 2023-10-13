using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class KeyframeWrapper
{
    public KeyframeData[] keyframes;
}

[Serializable]
public class KeyframeData
{
    public Load[] loads;
    public CreationItem[] creations;
    public StateUpdate[] stateUpdates;
    public int[] deletions;
    public Message message;
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

[Serializable]
public class Message
{
    public Highlight[] highlights;
}

[Serializable]
public class Highlight
{
    public List<float> t;
}
