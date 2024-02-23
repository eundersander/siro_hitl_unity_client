using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows to control a skinned/rigged mesh from gfx-replay "RigCreation" and "RigUpdate" keyframes.
/// The component is expected to be added to the root object of a skinned mesh.
/// </summary>
public class GfxReplaySkinnedMesh : MonoBehaviour
{
    /// <summary>
    /// Gfx-replay rigId associated with this skinned mesh.
    /// </summary>
    public int rigId {get; set;} = Constants.ID_UNDEFINED;

    private Transform[] _bones;

    SkinnedMeshRenderer _skinnedMeshRenderer;

    void Awake()
    {
        // Find the skinned mesh renderer.
        // This component is automatically created by the asset pipeline upon importing a skinned mesh.
        _skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if (_skinnedMeshRenderer == null)
        {
            Debug.LogError($"Object '{name}' has no SkinnedMeshRenderer. Skinning will not be applied.");
            enabled = false;
            return;
        }

        // Hide the skinned mesh until it is configured (i.e. until RigCreation is received).
        _skinnedMeshRenderer.enabled = false;

        // We don't update bounds every update. Instead, we never cull the skinned mesh.
        _skinnedMeshRenderer.updateWhenOffscreen = true;
    }

    public void configureRigInstance(List<string> boneNames)
    {
        if (!enabled)
        {
            return;
        }
        if (_skinnedMeshRenderer.bones.Length != boneNames.Count + 1) // 'boneNames' doesn't include the root.
        {
            Debug.LogError($"Skinned object '{name}' does not have the same number of bones than the rig {rigId}.");
            enabled = false;
            return;
        }

        // Match Unity bones to Habitat bone indices using bone names.
        _bones = new Transform[boneNames.Count];
        
        int matchedBoneCount = 0;
        for (int i = 0; i < boneNames.Count; ++i)
        {
            for (int j = 0; j < _skinnedMeshRenderer.bones.Length; ++j)
            {
                if (boneNames[i] == _skinnedMeshRenderer.bones[j].gameObject.name)
                {
                    _bones[i] = _skinnedMeshRenderer.bones[j];
                    ++matchedBoneCount;
                    continue;
                }
            }
        }
        if (matchedBoneCount != boneNames.Count)
        {
            Debug.LogError($"Skinned object '{name}' does not match the bones defined in rig {rigId}.");
            enabled = false;
        }

        _skinnedMeshRenderer.enabled = true;
    }

    public void setPose(List<RigUpdate.BoneTransform> pose) {
        if (!enabled)
        {
            return;
        }
        if (_bones == null)
        {
            Debug.LogError($"Skinned object '{name}' is not configured.");
            enabled = false;
            return;
        }
        if (pose == null || pose.Count != _bones.Length)
        { 
            Debug.LogError($"Invalid pose submitted to skinned object '{name}'.");
            enabled = false;
            return;
        }

        for (int i = 0; i < pose.Count; ++i)
        {
            _bones[i].position = CoordinateSystem.ToUnityVector(pose[i].t);
            _bones[i].rotation = CoordinateSystem.ToUnityQuaternion3DModel(pose[i].r);
        }
    }
}
