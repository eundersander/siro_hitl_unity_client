using System;
using UnityEngine;

public class HighlightManager : MessageConsumer
{
    const float TWO_PI = Mathf.PI * 2.0f;
    
    [Tooltip("Size of the highlight object pool.\nIf the amount of highlights received from the server is higher that this number, the excess will be discarded.\nThis value cannot be changed during runtime.")]
    public int poolSize = 32;

    [Tooltip("Line segment count composing the highlight circles.\nThis value cannot be changed during runtime.")]
    public int circleResolution = 32;

    [Tooltip("Vertex color of the highlight lines.")]
    public Color highlightColor = Color.white;

    [Tooltip("Materials used to shade the highlight circles.")]
    public Material[] highlightMaterials;

    [Tooltip("Thickness of the highlight circles, in meters.")]
    public float highlightWidth = 0.005f;

    [Tooltip("Base radius of the highlight circles, in meters. It is multiplied by the radius received from server messages.")]
    public float highlightBaseRadius = 1.0f;

    private LineRenderer[] _highlightPool;
    private int _activeHighlightCount = 0;

    // Start is called before the first frame update
    void Awake()
    {
        _highlightPool = new LineRenderer[poolSize];
        float _arcSegmentLength = TWO_PI / circleResolution;

        // Construct a pool of highlight line renderers
        GameObject container = new GameObject("Highlights");
        container.transform.parent = transform;
        for (int i = 0; i < _highlightPool.Length; ++i)
        {
            GameObject highlight = new GameObject($"Highlight {i}");
            var lineRenderer = highlight.AddComponent<LineRenderer>();

            highlight.transform.parent = container.transform;
            lineRenderer.enabled = false;
            lineRenderer.loop = true;
            lineRenderer.useWorldSpace = false;
            lineRenderer.startColor = highlightColor;
            lineRenderer.endColor = highlightColor;
            lineRenderer.startWidth = highlightWidth;
            lineRenderer.endWidth = highlightWidth;
            lineRenderer.materials = highlightMaterials;
            lineRenderer.positionCount = circleResolution;
            for (int j = 0; j < circleResolution; ++j)
            {
                float xOffset = highlightBaseRadius * Mathf.Sin(j * _arcSegmentLength);
                float yOffset = highlightBaseRadius * Mathf.Cos(j * _arcSegmentLength);
                lineRenderer.SetPosition(j, new Vector3(xOffset, yOffset, 0.0f));
            }
            _highlightPool[i] = lineRenderer;
        }
    }

    public override void ProcessMessage(Message message)
    {
        if (!enabled) return;

        // Disable all highlights in the pool
        for (int i = 0; i < _activeHighlightCount; ++i)
        {
            _highlightPool[i].enabled = false;
        }
        // Draw highlights
        var highlightsMessage = message.highlights;
        Vector3 up = new Vector3(0, 1, 0);
        Color defaultColor = Color.white;
        if (highlightsMessage != null)
        {
            _activeHighlightCount = Math.Min(highlightsMessage.Length, _highlightPool.Length);
            for (int i = 0; i < _activeHighlightCount; ++i)
            {
                Highlight msg = highlightsMessage[i];
                var highlight = _highlightPool[i];
                highlight.enabled = true;

                Color color = defaultColor;
                if (msg.c != null && msg.c.Length == 4)
                {
                    color.r = (float)msg.c[0] / 255.0f;
                    color.g = (float)msg.c[1] / 255.0f;
                    color.b = (float)msg.c[2] / 255.0f;
                    color.a = (float)msg.c[3] / 255.0f;
                }
                highlight.startColor = color;
                highlight.endColor = color;

                // Apply translation from message
                Vector3 center = CoordinateSystem.ToUnityVector(msg.t);
                highlight.transform.position = center;

                // Billboarding
                if (msg.b == 1)
                {
                    highlight.transform.LookAt(Camera.main.transform);
                } else
                {
                    highlight.transform.LookAt(center + up);
                }

                // Apply radius from message using scale
                highlight.transform.localScale = highlightsMessage[i].r * Vector3.one;
            }
        }
    }
}
