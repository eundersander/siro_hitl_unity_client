using UnityEngine;


[RequireComponent(typeof(HighlightManager))]
[RequireComponent(typeof(StatusDisplayHelper))]
[RequireComponent(typeof(TextRenderer))]
public class AppMouseKeyboard : App
{
    protected HighlightManager _highlightManager;
    protected StatusDisplayHelper _statusDisplayHelper;
    protected TextRenderer _textRenderer;
    
    protected override void Main()
    {
        _highlightManager = gameObject.GetComponent<HighlightManager>();
        _statusDisplayHelper = gameObject.GetComponent<StatusDisplayHelper>();
        _textRenderer = gameObject.GetComponent<TextRenderer>();

        base.Main();
    }

    void Awake()
    {
        Main();
    }
}
