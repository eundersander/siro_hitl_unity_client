using TMPro;
using UnityEngine;

public class TextRenderer : MonoBehaviour, IKeyframeMessageConsumer
{
    const float UI_GAZE_FOLLOWING_SPEED = 10.0f;

    [Tooltip("Distance between the offline icon and the camera")]
    public float uiPlaneDistance = 3.0f;

    public GameObject textPanelRoot;
    public TextMeshPro textComponent;
    Transform _targetTransform;

    void Awake()
    {
        _targetTransform = new GameObject("Target transform").transform;
    }

    void Update()
    {
        Camera camera = Camera.main;
        _targetTransform.transform.position = camera.transform.position + camera.transform.forward * uiPlaneDistance;
        _targetTransform.transform.LookAt(camera.transform, Vector3.up);
        _targetTransform.transform.Rotate(Vector3.up, 180.0f, Space.Self);

        if (textPanelRoot.activeSelf)
        {
            textPanelRoot.transform.position = 
                Vector3.Lerp(textPanelRoot.transform.position, _targetTransform.position, Time.deltaTime * UI_GAZE_FOLLOWING_SPEED);
            textPanelRoot.transform.rotation =
                Quaternion.Slerp(textPanelRoot.transform.rotation, _targetTransform.rotation, Time.deltaTime * UI_GAZE_FOLLOWING_SPEED);
        }
        else 
        {
            textPanelRoot.transform.position = _targetTransform.position;
            textPanelRoot.transform.rotation = _targetTransform.rotation;
        }
    }

    public void SetText(string text)
    {
        if (!enabled) return;

        textPanelRoot.SetActive(string.IsNullOrEmpty(text));
        textComponent.text = text;
    }

    public void ProcessMessage(Message message)
    {
        SetText(message.textMessage);
    }
}
