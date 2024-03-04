using System;
using UnityEngine;

public class HighlightManager : IKeyframeMessageConsumer
{
    const float TWO_PI = Mathf.PI * 2.0f;

    private AppConfig _config;
    private Camera _camera;
    private GameObject _container;
    private LineRenderer[] _highlightPool;
    private int _activeHighlightCount = 0;

    public HighlightManager(AppConfig config, Camera camera)
    {
        _config = config;
        _camera = camera;

        _container = new GameObject("HighlightManager");

        _highlightPool = new LineRenderer[_config.highlightPoolSize];
        float _arcSegmentLength = TWO_PI / _config.highlightCircleResolution;

        // Construct a pool of highlight line renderers
        GameObject container = new GameObject("Highlights");
        container.transform.parent = _container.transform;
        for (int i = 0; i < _highlightPool.Length; ++i)
        {
            GameObject highlight = new GameObject($"Highlight {i}");
            var lineRenderer = highlight.AddComponent<LineRenderer>();

            highlight.transform.parent = container.transform;
            lineRenderer.enabled = false;
            lineRenderer.loop = true;
            lineRenderer.useWorldSpace = false;
            lineRenderer.startColor = config.highlightColor;
            lineRenderer.endColor = config.highlightColor;
            lineRenderer.startWidth = config.highlightWidth;
            lineRenderer.endWidth = config.highlightWidth;
            lineRenderer.materials = config.highlightMaterials;
            lineRenderer.positionCount = config.highlightCircleResolution;
            for (int j = 0; j < config.highlightCircleResolution; ++j)
            {
                float xOffset = config.highlightBaseRadius * Mathf.Sin(j * _arcSegmentLength);
                float yOffset = config.highlightBaseRadius * Mathf.Cos(j * _arcSegmentLength);
                lineRenderer.SetPosition(j, new Vector3(xOffset, yOffset, 0.0f));
            }
            _highlightPool[i] = lineRenderer;
        }
    }

    public void ProcessMessage(Message message)
    {
        // Disable all highlights in the pool
        for (int i = 0; i < _activeHighlightCount; ++i)
        {
            _highlightPool[i].enabled = false;
        }
        // Draw highlights
        var highlightsMessage = message.highlights;
        if (highlightsMessage != null)
        {
            _activeHighlightCount = Math.Min(highlightsMessage.Length, _highlightPool.Length);
            for (int i = 0; i < _activeHighlightCount; ++i)
            {
                var highlight = _highlightPool[i];
                highlight.enabled = true;

                // Apply translation from message
                Vector3 center = CoordinateSystem.ToUnityVector(highlightsMessage[i].t);
                highlight.transform.position = center;

                // Billboarding
                highlight.transform.LookAt(_camera.transform);

                // Apply radius from message using scale
                highlight.transform.localScale = highlightsMessage[i].r * Vector3.one;
            }
        }
    }

    public void Update() {}
}
