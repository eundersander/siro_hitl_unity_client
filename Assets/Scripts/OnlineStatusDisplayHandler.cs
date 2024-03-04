using System.Collections;
using UnityEngine;

public class OnlineStatusDisplayHandler
{
    // Speed of UI gaze following.
    const float UI_GAZE_FOLLOWING_SPEED = 10.0f;

    // Time that the offline icon is displayed.
    const float OFFLINE_ICON_DISPLAY_TIME = 5.0f;

    // Distance between the offline icon and the camera.
    const float UI_PLANE_DISTANCE = 3.0f;

    Color _ambientColor;

    bool _onlineLastFrame = false;

    Camera _camera;
    Transform _iconTargetTransform;
    GameObject _offlineIcon;
    Coroutine _networkStatusIconCoroutine = null;
    CoroutineContainer _coroutines;

    public OnlineStatusDisplayHandler(GameObject offlineIcon, Camera camera)
    {
        _offlineIcon = offlineIcon;
        _offlineIcon.SetActive(false);

        _camera = camera;

        _coroutines = CoroutineContainer.Create("StatusDisplayHelper");

        _ambientColor = RenderSettings.ambientLight;
        _iconTargetTransform = new GameObject("Icon target transform").transform;
    }

    public void Update(bool online)
    {
        if (!online)
        {
            _iconTargetTransform.transform.position = _camera.transform.position + _camera.transform.forward * UI_PLANE_DISTANCE;
            _iconTargetTransform.transform.LookAt(_camera.transform, Vector3.up);
            _iconTargetTransform.transform.Rotate(Vector3.up, 180.0f, Space.Self);
            _offlineIcon.transform.position = 
                Vector3.Lerp(_offlineIcon.transform.position, _iconTargetTransform.position, Time.deltaTime * UI_GAZE_FOLLOWING_SPEED);
            _offlineIcon.transform.rotation =
                Quaternion.Slerp(_offlineIcon.transform.rotation, _iconTargetTransform.rotation, Time.deltaTime * UI_GAZE_FOLLOWING_SPEED);
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

    void OnConnectionLost()
    {
        if (_networkStatusIconCoroutine != null)
        {
            _coroutines.StopCoroutine(_networkStatusIconCoroutine);
        }
        _networkStatusIconCoroutine = _coroutines.StartCoroutine(ShowElementForDuration(_offlineIcon, OFFLINE_ICON_DISPLAY_TIME));
        
        RenderSettings.ambientLight = Color.red;
        _offlineIcon.transform.position = _iconTargetTransform.position;
        _offlineIcon.transform.rotation = _iconTargetTransform.rotation;
    }

    void OnConnectionRestored()
    {
        _offlineIcon.SetActive(false);
        RenderSettings.ambientLight = _ambientColor;
    }

    IEnumerator ShowElementForDuration(GameObject o, float time)
    {
        o.SetActive(true);
        yield return new WaitForSeconds(time);
        o.SetActive(false);
    }
}
