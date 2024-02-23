using System;
using System.Collections.Generic;

public static class Constants
{
    public const int ID_UNDEFINED = -1;
}

[Serializable]
public class AbsTransform
{
    public List<float> translation;
    public List<float> rotation;
}

[Serializable]
public class KeyframeWrapper
{
    public KeyframeData[] keyframes;
}

[Serializable]
public class KeyframeData
{
    public Load[] loads;
    public RigCreation[] rigCreations;
    public CreationItem[] creations;
    public StateUpdate[] stateUpdates;
    public RigUpdate[] rigUpdates;
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
    public int rigId;
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
    }
}

[Serializable]
public class RigCreation
{
    public int id;
    public List<string> boneNames;
}

[Serializable]
public class RigUpdate
{
    [Serializable]
    public class BoneTransform
    {
        public List<float> t;
        public List<float> r;
    }

    public int id;
    public List<BoneTransform> pose;
}

[Serializable]
public class Message
{
    public Highlight[] highlights;

    public List<float> teleportAvatarBasePosition;

    public bool sceneChanged;
    // nonindexed triangle list, serialized as a flat list of floats
    public List<float> navmeshVertices;

    public string textMessage;

    public AbsTransform camera;
}

[Serializable]
public class Highlight
{
    public List<float> t;
    public float r;
}