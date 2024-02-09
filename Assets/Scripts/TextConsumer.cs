using System;
using TMPro;
using UnityEngine;

public class TextConsumer : MessageConsumer
{
    const float TWO_PI = Mathf.PI * 2.0f;
    
    [Tooltip("Size of the text object pool.\nIf the amount of texts received from the server is higher that this number, the excess will be discarded.\nThis value cannot be changed during runtime.")]
    public int poolSize = 32;

    [Tooltip("UI canvas to render text on.")]
    public Canvas canvas;

    [Tooltip("Text prefab that contains the text properties. This template is cloned upon initialization.")]
    public TextMeshProUGUI textTemplate;

    private TextMeshProUGUI[] _textPool;
    private int _activeTextCount = 0;

    // Start is called before the first frame update
    void Awake()
    {
        _textPool = new TextMeshProUGUI[poolSize];

        // Construct a pool of text text renderers
        GameObject container = new GameObject("Texts");
        container.transform.SetParent(canvas.transform);
        container.transform.localPosition = Vector3.zero;
        container.transform.localRotation = Quaternion.identity;
        container.transform.localScale = Vector3.one;
        for (int i = 0; i < _textPool.Length; ++i)
        {
            GameObject text = Instantiate(textTemplate.gameObject);
            text.name = $"Text {i}";
            var textRenderer = text.GetComponent<TextMeshProUGUI>();
            textRenderer.rectTransform.anchoredPosition = Vector2.zero;
            textRenderer.rectTransform.parent = container.transform;
            textRenderer.enabled = false;
            _textPool[i] = textRenderer;
        }
    }

    public override void ProcessMessage(Message message)
    {
        if (!enabled) return;

        // Disable all texts in the pool
        for (int i = 0; i < _activeTextCount; ++i)
        {
            _textPool[i].enabled = false;
        }
        // Draw texts
        if (message.texts != null)
        {
            _activeTextCount = Math.Min(message.texts.Count, _textPool.Length);
            for (int i = 0; i < _activeTextCount; ++i)
            {
                var textInstance = _textPool[i];
                var text = message.texts[i];
                textInstance.enabled = true;
                textInstance.text = text.text;

                textInstance.rectTransform.anchoredPosition = new Vector2(
                    Camera.main.pixelWidth * text.position[0],
                    Camera.main.pixelHeight * text.position[1]
                );
            }
        }
    }
}
