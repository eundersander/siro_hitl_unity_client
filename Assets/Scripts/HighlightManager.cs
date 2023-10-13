using System;
using System.Collections.Generic;
using UnityEngine;

public class HighlightManager : MonoBehaviour
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

    public void ProcessKeyframe(KeyframeData keyframe)
    {
        if (!enabled) return;

        // Disable all highlights in the pool
        for (int i = 0; i < _activeHighlightCount; ++i)
        {
            _highlightPool[i].enabled = false;
        }
        // Draw highlights
        var highlightsMessage = keyframe.message?.highlights;
        if (highlightsMessage != null)
        {
            _activeHighlightCount = Math.Min(highlightsMessage.Length, _highlightPool.Length);
            for (int i = 0; i < _activeHighlightCount; ++i)
            {
                var highlight = _highlightPool[i];
                highlight.enabled = true;

                // Apply translation from message
                Vector3 center = CoordinateConventionHelper.ToUnityVector(highlightsMessage[i].t);
                highlight.transform.position = center;

                // Billboarding
                highlight.transform.LookAt(Camera.main.transform);

                // Apply radius from message using scale
                highlight.transform.localScale = highlightsMessage[i].r * Vector3.one;
            }
        }
    }
}
