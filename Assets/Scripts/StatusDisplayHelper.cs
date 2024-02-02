using System.Collections;
using UnityEngine;

public class StatusDisplayHelper : MonoBehaviour
{
    const float UI_GAZE_FOLLOWING_SPEED = 10.0f;

    [Tooltip("Icon that is displayed when connection is lost.")]
    public GameObject offlineIcon;

    [Tooltip("Time that the offline icon is displayed.")]
    public float offlineIconDisplayTime = 5.0f;

    [Tooltip("Distance between the offline icon and the camera")]
    public float uiPlaneDistance = 3.0f;

    NetworkClient _networkClient;

    Color _ambientColor;

    bool _onlineLastFrame = false;

    Transform _iconTargetTransform;


    Coroutine _networkStatusIconCoroutine = null;
    Coroutine _sceneChangeCoroutine = null;

    void Awake()
    {
        offlineIcon.SetActive(false);
        _networkClient = GetComponent<NetworkClient>();
        if (_networkClient == null)
        {
            Debug.LogWarning($"Network client missing from '{name}'. Network status updates will be ignored.");
        }
        _ambientColor = RenderSettings.ambientLight;
        _iconTargetTransform = new GameObject("Icon target transform").transform;

        // Initialize fog
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = Color.black;
        RenderSettings.fog = false;
        RenderSettings.fogDensity = 0.0f;
    }

    void Update()
    {
        if (_networkClient)
        {
            bool online = _networkClient.IsConnected();

            if (!online)
            {
                Camera camera = Camera.main;
                _iconTargetTransform.transform.position = camera.transform.position + camera.transform.forward * uiPlaneDistance;
                _iconTargetTransform.transform.LookAt(camera.transform, Vector3.up);
                _iconTargetTransform.transform.Rotate(Vector3.up, 180.0f, Space.Self);
                offlineIcon.transform.position = 
                    Vector3.Lerp(offlineIcon.transform.position, _iconTargetTransform.position, Time.deltaTime * UI_GAZE_FOLLOWING_SPEED);
                offlineIcon.transform.rotation =
                    Quaternion.Slerp(offlineIcon.transform.rotation, _iconTargetTransform.rotation, Time.deltaTime * UI_GAZE_FOLLOWING_SPEED);
            }

            if (!online && _onlineLastFrame)
            {
                OnConnectionLost();
            }
            else if (online && !_onlineLastFrame)
            {
                OnConnectionRestored();
            }

            _onlineLastFrame = online;
        }
    }

    void OnConnectionLost()
    {
        if (_networkStatusIconCoroutine != null)
        {
            StopCoroutine(_networkStatusIconCoroutine);
        }
        _networkStatusIconCoroutine = StartCoroutine(ShowElementForDuration(offlineIcon, offlineIconDisplayTime));
        
        RenderSettings.ambientLight = Color.red;
        offlineIcon.transform.position = _iconTargetTransform.position;
        offlineIcon.transform.rotation = _iconTargetTransform.rotation;
    }

    void OnConnectionRestored()
    {
        offlineIcon.SetActive(false);
        RenderSettings.ambientLight = _ambientColor;
    }

    public void OnSceneChangeBegin()
    {
        RenderSettings.fog = true;
        RenderSettings.fogDensity = 1.0f;

        if (_sceneChangeCoroutine != null)
        {
            StopCoroutine(_sceneChangeCoroutine);
        }
    }

    public void OnSceneChangeEnd()
    {
        _sceneChangeCoroutine = StartCoroutine(ProgressivelyRemoveFog(0.75f));
    }

    IEnumerator ShowElementForDuration(GameObject o, float time)
    {
        o.SetActive(true);
        yield return new WaitForSeconds(time);
        o.SetActive(false);
    }

    IEnumerator ProgressivelyRemoveFog(float duration)
    {
        float initialFogDensity = RenderSettings.fogDensity;
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            float t = EaseInCubic(elapsedTime / duration);
            float fogDensity = Mathf.Lerp(initialFogDensity, 0.0f, t);
            RenderSettings.fogDensity = fogDensity;

            elapsedTime += Time.deltaTime;
            yield return null; // Skip one frame
        }

        RenderSettings.fog = false;
    }

    float EaseInCubic(float x) {
        return x * x * x;
    }
}
