using System;
using UnityEngine;
using UnityEngine.Assertions;

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
            lineRenderer.startColor = config.highlightDefaultColor;
            lineRenderer.endColor = config.highlightDefaultColor;
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
                Highlight msg = highlightsMessage[i];
                var lineRenderer = _highlightPool[i];
                lineRenderer.enabled = true;

                Color color = _config.highlightDefaultColor;
                if (msg.c != null && msg.c.Length > 0)                
                {
                    Assert.AreEqual(msg.c.Length, 4, $"Invalid highlight color format. Expected 4 ints, got {msg.c.Length}.");
                    color.r = (float)msg.c[0] / 255.0f;
                    color.g = (float)msg.c[1] / 255.0f;
                    color.b = (float)msg.c[2] / 255.0f;
                    color.a = (float)msg.c[3] / 255.0f;
                }
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;

                // Apply translation from message
                Vector3 center = CoordinateSystem.ToUnityVector(msg.t);
                lineRenderer.transform.position = center;

                // Billboarding
                if (msg.b == 1)
                {
                    lineRenderer.transform.LookAt(_camera.transform);
                }
                else
                {
                    lineRenderer.transform.LookAt(center + Vector3.up);
                }

                // Apply radius from message using scale
                lineRenderer.transform.localScale = highlightsMessage[i].r * Vector3.one;
            }
        }
    }

    public void Update() {}
}
