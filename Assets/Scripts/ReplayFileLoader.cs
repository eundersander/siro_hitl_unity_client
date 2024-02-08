using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;

public class ReplayFileLoader : MonoBehaviour
{
    [SerializeField]
    private TextAsset jsonFile; // Drag your JSON file onto this field in the inspector

    private KeyframeWrapper _keyframeWrapper;
    private GfxReplayPlayer _player;
    private int _nextKeyframeIdx = 0;

    void Start()
    {
        _player = GetComponent<GfxReplayPlayer>();
        Assert.IsTrue(_player);  // our object should have a GfxReplayPlayer

        _keyframeWrapper = JsonUtility.FromJson<KeyframeWrapper>(jsonFile.text);

        Assert.IsTrue(_keyframeWrapper.keyframes.Length > 0);
        _nextKeyframeIdx = 0;
        NextKeyframe();
    }

    private void NextKeyframe()
    {
        if (_nextKeyframeIdx >= _keyframeWrapper.keyframes.Length)
        {
            return;
        }
        _player.ProcessKeyframe(_keyframeWrapper.keyframes[_nextKeyframeIdx]);
        Debug.Log($"processed keyframe {_nextKeyframeIdx}");
        _nextKeyframeIdx++;
    }

    void Update()
    {
        // if (Keyboard.current.spaceKey.wasPressedThisFrame)
        if (Keyboard.current.spaceKey.isPressed)
        {
                NextKeyframe();
        }
    }
}
