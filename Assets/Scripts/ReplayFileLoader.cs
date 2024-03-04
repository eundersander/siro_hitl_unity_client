using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;

public class ReplayFileLoader : IUpdatable
{
    private bool _enabled = true;
    private KeyframeWrapper _keyframeWrapper;
    private GfxReplayPlayer _player;
    private int _nextKeyframeIdx = 0;

    public ReplayFileLoader(GfxReplayPlayer player, TextAsset keyframes)
    {
        if (keyframes == null)
        {
            _enabled = false;
            return;
        }
        _player = player;

        _keyframeWrapper = JsonUtility.FromJson<KeyframeWrapper>(keyframes.text);
        Assert.IsTrue(_keyframeWrapper.keyframes.Length > 0);

        NextKeyframe();
    }

    private void NextKeyframe()
    {
        if (!_enabled || _nextKeyframeIdx >= _keyframeWrapper.keyframes.Length)
        {
            return;
        }
        _player.ProcessKeyframe(_keyframeWrapper.keyframes[_nextKeyframeIdx]);
        Debug.Log($"processed keyframe {_nextKeyframeIdx}");
        _nextKeyframeIdx++;
    }

    public void Update()
    {
        if (!_enabled)
        {
            return;
        }
        if (Keyboard.current.spaceKey.isPressed)
        {
            NextKeyframe();
        }
    }
}
