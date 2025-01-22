using UnityEngine;
using System;
using System.Collections.Generic;

public class InputTrackerXRPose : InputTracker
{
    AvatarData _inputData = new AvatarData();

    public GameObject xrHeadObject;
    public GameObject xrLeftControllerObject;
    public GameObject xrRightControllerObject;
    public Transform leftHandRigRoot;
    public Transform rightHandRigRoot;

    private GameObject[] _leftHandBoneObjects = null;
    private GameObject[] _rightHandBoneObjects = null;

    public override void UpdateClientState(ref ClientState state)
    {
        _inputData.root.FromGameObject(xrHeadObject);
        _inputData.hands[0].FromGameObject(xrLeftControllerObject);
        _inputData.hands[1].FromGameObject(xrRightControllerObject);

        if (_leftHandBoneObjects == null)
        {
            _leftHandBoneObjects = ConstructFlatBoneObjectList(leftHandRigRoot).ToArray();
            _rightHandBoneObjects = ConstructFlatBoneObjectList(rightHandRigRoot).ToArray();
        }
        _inputData.articulatedHands[0].FromGameObjectList(_leftHandBoneObjects);
        _inputData.articulatedHands[1].FromGameObjectList(_rightHandBoneObjects);

        state.avatar = _inputData;
    }

    // https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#convention-of-hand-joints
    // typedef enum XrHandJointEXT {
    //     XR_HAND_JOINT_PALM_EXT = 0,
    //     XR_HAND_JOINT_WRIST_EXT = 1,
    //     XR_HAND_JOINT_THUMB_METACARPAL_EXT = 2,
    //     XR_HAND_JOINT_THUMB_PROXIMAL_EXT = 3,
    //     XR_HAND_JOINT_THUMB_DISTAL_EXT = 4,
    //     XR_HAND_JOINT_THUMB_TIP_EXT = 5,
    //     XR_HAND_JOINT_INDEX_METACARPAL_EXT = 6,
    //     XR_HAND_JOINT_INDEX_PROXIMAL_EXT = 7,
    //     XR_HAND_JOINT_INDEX_INTERMEDIATE_EXT = 8,
    //     XR_HAND_JOINT_INDEX_DISTAL_EXT = 9,
    //     XR_HAND_JOINT_INDEX_TIP_EXT = 10,
    //     XR_HAND_JOINT_MIDDLE_METACARPAL_EXT = 11,
    //     XR_HAND_JOINT_MIDDLE_PROXIMAL_EXT = 12,
    //     XR_HAND_JOINT_MIDDLE_INTERMEDIATE_EXT = 13,
    //     XR_HAND_JOINT_MIDDLE_DISTAL_EXT = 14,
    //     XR_HAND_JOINT_MIDDLE_TIP_EXT = 15,
    //     XR_HAND_JOINT_RING_METACARPAL_EXT = 16,
    //     XR_HAND_JOINT_RING_PROXIMAL_EXT = 17,
    //     XR_HAND_JOINT_RING_INTERMEDIATE_EXT = 18,
    //     XR_HAND_JOINT_RING_DISTAL_EXT = 19,
    //     XR_HAND_JOINT_RING_TIP_EXT = 20,
    //     XR_HAND_JOINT_LITTLE_METACARPAL_EXT = 21,
    //     XR_HAND_JOINT_LITTLE_PROXIMAL_EXT = 22,
    //     XR_HAND_JOINT_LITTLE_INTERMEDIATE_EXT = 23,
    //     XR_HAND_JOINT_LITTLE_DISTAL_EXT = 24,
    //     XR_HAND_JOINT_LITTLE_TIP_EXT = 25,
    //     XR_HAND_JOINT_MAX_ENUM_EXT = 0x7FFFFFFF
    // } XrHandJointEXT;

    private static readonly string[] BoneRegexPatterns = new string[]
    {
        ".*palm.*",
        ".*wrist.*",
        ".*thumb.*metacarpal.*",
        ".*thumb.*proximal.*",
        ".*thumb.*distal.*",
        ".*thumb.*tip.*",
        ".*index.*metacarpal.*",
        ".*index.*proximal.*",
        ".*index.*intermediate.*",
        ".*index.*distal.*",
        ".*index.*tip.*",
        ".*middle.*metacarpal.*",
        ".*middle.*proximal.*",
        ".*middle.*intermediate.*",
        ".*middle.*distal.*",
        ".*middle.*tip.*",
        ".*ring.*metacarpal.*",
        ".*ring.*proximal.*",
        ".*ring.*intermediate.*",
        ".*ring.*distal.*",
        ".*ring.*tip.*",
        ".*little.*metacarpal.*",
        ".*little.*proximal.*",
        ".*little.*intermediate.*",
        ".*little.*distal.*",
        ".*little.*tip.*"
    };

    private static List<GameObject> ConstructFlatBoneObjectList(Transform rigRoot)
    {
        List<GameObject> orderedBones = new List<GameObject>(new GameObject[ArticulatedHandData.NumHandBones]);
        Transform[] allChildren = rigRoot.GetComponentsInChildren<Transform>(true);

        Debug.Log("All children under rigRoot:");
        foreach (Transform child in allChildren)
        {
            Debug.Log(child.name);
        }

        for (int i = 0; i < BoneRegexPatterns.Length; i++)
        {
            string pattern = BoneRegexPatterns[i];
            foreach (Transform child in allChildren)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(child.name, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    orderedBones[i] = child.gameObject;
                    break;
                }
            }
        }

        // Ensure no null entries remain in the list
        for (int i = 0; i < orderedBones.Count; i++)
        {
            if (orderedBones[i] == null)
            {
                throw new System.Exception($"Missing bone for pattern: {BoneRegexPatterns[i]} in rig: {rigRoot.name}");
            }
        }

        return orderedBones;
    }
    

    public override void OnEndFrame() {}
}
