using System.Collections;
using UnityEngine;

public class LoadingEffectHandler : IKeyframeMessageConsumer
{
    Coroutine _sceneChangeCoroutine = null;
    CoroutineContainer _coroutines;

    public LoadingEffectHandler()
    {
        _coroutines = CoroutineContainer.Create("LoadingEffectHandler");

        // Initialize fog
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = Color.black;
        RenderSettings.fog = false;
        RenderSettings.fogDensity = 0.0f;
    }

    public void Update() {}

    public void OnSceneChangeBegin()
    {
        RenderSettings.fog = true;
        RenderSettings.fogDensity = 1.0f;

        if (_sceneChangeCoroutine != null)
        {
            _coroutines.StopCoroutine(_sceneChangeCoroutine);
        }
    }

    public void OnSceneChangeEnd()
    {
        _sceneChangeCoroutine = _coroutines.StartCoroutine(ProgressivelyRemoveFog(0.75f));
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

    static float EaseInCubic(float x) {
        return x * x * x;
    }

    public void ProcessMessage(Message message)
    {
        if (message.sceneChanged)
        {
            OnSceneChangeBegin();
        }
    }

    public void PostProcessMessage(Message message)
    {
        if (message.sceneChanged)
        {
            OnSceneChangeEnd();
        }
    }
}
