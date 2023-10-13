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

    [Tooltip("Material used to shade the highlight circles.")]
    public Material highlightMaterial;

    [Tooltip("Thickness of the highlight circles, in meters.")]
    public float highlightWidth = 0.025f;

    [Tooltip("Radius of the highlight circles, in meters.")]
    public float highlightRadius = 0.15f;

    private LineRenderer[] _highlightPool;
    private float _arcSegmentLength;
    private int _activeHighlightCount = 0;

    // Start is called before the first frame update
    void Awake()
    {
        _arcSegmentLength = TWO_PI / circleResolution;
        _highlightPool = new LineRenderer[poolSize];

        // Construct a pool of highlight line renderers
        GameObject container = new GameObject("Highlights");
        container.transform.parent = transform;
        for (int i = 0; i < _highlightPool.Length; ++i)
        {
            GameObject highlight = new GameObject($"Highlight {i}");
            var lineRenderer = highlight.AddComponent<LineRenderer>();
            highlight.transform.parent = container.transform;
            lineRenderer.loop = true;
            lineRenderer.useWorldSpace = false;
            lineRenderer.startColor = highlightColor;
            lineRenderer.endColor = highlightColor;
            lineRenderer.startWidth = highlightWidth;
            lineRenderer.endWidth = highlightWidth;
            lineRenderer.material = highlightMaterial;
            _highlightPool[i] = lineRenderer;
            lineRenderer.positionCount = circleResolution;
            for (int j = 0; j < circleResolution; ++j)
            {
                float xOffset = highlightRadius * Mathf.Sin(j * _arcSegmentLength);
                float zOffset = highlightRadius * Mathf.Cos(j * _arcSegmentLength);
                lineRenderer.SetPosition(j, new Vector3(xOffset, 0.0f, zOffset));
            }
            lineRenderer.enabled = false;
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

                Vector3 center = CoordinateConventionHelper.ToUnityVector(highlightsMessage[i].t);
                highlight.transform.position = center;
            }
        }
    }

    void Update()
    {
        
    }
}
