using UnityEngine;

/// <summary>
/// Mouse/Keyboard Habitat client application.
/// Intended to be used from web browsers (WebGL).
/// </summary>
[RequireComponent(typeof(HighlightManager))]
[RequireComponent(typeof(StatusDisplayHelper))]
[RequireComponent(typeof(TextRenderer))]
public class AppMouseKeyboard : App
{
    protected override void Main()
    {
        base.Main();
    }

    void Awake()
    {
        Main();
    }
}
