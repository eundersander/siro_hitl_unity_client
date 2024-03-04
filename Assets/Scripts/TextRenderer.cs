using TMPro;
using UnityEngine;

public class TextRenderer : IKeyframeMessageConsumer
{
    const float UI_GAZE_FOLLOWING_SPEED = 10.0f;

    float _uiPlaneDistance = 20.0f;
    GameObject _textPanelRoot;
    TextMeshPro _textComponent;
    Transform _targetTransform;
    Camera _camera;

    public TextRenderer(float uiPlaneDistance, GameObject textPanelRoot, TextMeshPro textComponent, Camera camera)
    {
        _uiPlaneDistance = uiPlaneDistance;
        _textPanelRoot = textPanelRoot;
        _textComponent = textComponent;
        _camera = camera;
        _targetTransform = new GameObject("TextRenderer Target transform").transform;
    }

    public void Update()
    {
        _targetTransform.transform.position = _camera.transform.position + _camera.transform.forward * _uiPlaneDistance;
        _targetTransform.transform.LookAt(_camera.transform, Vector3.up);
        _targetTransform.transform.Rotate(Vector3.up, 180.0f, Space.Self);

        if (_textPanelRoot.activeSelf)
        {
            _textPanelRoot.transform.position = 
                Vector3.Lerp(_textPanelRoot.transform.position, _targetTransform.position, Time.deltaTime * UI_GAZE_FOLLOWING_SPEED);
            _textPanelRoot.transform.rotation =
                Quaternion.Slerp(_textPanelRoot.transform.rotation, _targetTransform.rotation, Time.deltaTime * UI_GAZE_FOLLOWING_SPEED);
        }
        else 
        {
            _textPanelRoot.transform.position = _targetTransform.position;
            _textPanelRoot.transform.rotation = _targetTransform.rotation;
        }
    }

    public void SetText(string text)
    {
        _textPanelRoot.SetActive(string.IsNullOrEmpty(text));
        _textComponent.text = text;
    }

    public void ProcessMessage(Message message)
    {
        SetText(message.textMessage);
    }
}
