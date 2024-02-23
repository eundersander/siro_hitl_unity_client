using UnityEngine;

public class InputTrackerMouse : MonoBehaviour, IClientStateProducer
{
    // Left: 0, Right: 1, Middle: 2
    const int MOUSE_BUTTON_COUNT = 3;

    MouseInputData _inputData = new MouseInputData();

    bool[] _buttonHeld;
    bool[] _buttonUp;
    bool[] _buttonDown;

    Vector2 _scrollDelta = Vector2.zero;

    void Awake()
    {
        // Create arrays that hold whether buttons were pressed since the last OnEndFrame() call.
        _buttonHeld = new bool[MOUSE_BUTTON_COUNT];
        _buttonUp = new bool[MOUSE_BUTTON_COUNT];
        _buttonDown = new bool[MOUSE_BUTTON_COUNT];
    }

    public void Update()
    {
        // Record all mouse actions that occurred OnEndFrame() call.
        // Note that multiple Unity frames may occur during that time.
        int mouseIndex = 0;
        for (KeyCode key = KeyCode.Mouse0; key < KeyCode.Mouse2; key++, mouseIndex++)
        {
            if (Input.GetKey(key))
            {
                _buttonHeld[mouseIndex] = true;
            }
            if (Input.GetKeyDown(key))
            {
                _buttonUp[mouseIndex] = true;
            }
            else if (Input.GetKeyUp(key))
            {
                _buttonDown[mouseIndex] = true;
            }
        }

        _scrollDelta += Input.mouseScrollDelta; // Cumulative
    }

    public void OnEndFrame()
    {
        System.Array.Clear(_buttonHeld, 0, _buttonHeld.Length);
        System.Array.Clear(_buttonUp, 0, _buttonUp.Length);
        System.Array.Clear(_buttonDown, 0, _buttonDown.Length);

        _scrollDelta = Vector2.zero;
    }

    public void UpdateClientState(ref ClientState state)
    {
        _inputData.buttons.buttonHeld.Clear();
        _inputData.buttons.buttonUp.Clear();
        _inputData.buttons.buttonDown.Clear();

        if (Input.mousePresent) // TODO: Don't send mouse info if we don't have it
        {
            for (int i = 0; i < MOUSE_BUTTON_COUNT; ++i)
            {
                // Omit buttons that were not used.
                if (_buttonHeld[i]) _inputData.buttons.buttonHeld.Add(i);
                if (_buttonUp[i]) _inputData.buttons.buttonUp.Add(i);
                if (_buttonDown[i]) _inputData.buttons.buttonDown.Add(i);
            }
            _inputData.scrollDelta[0] = _scrollDelta.x;
            _inputData.scrollDelta[1] = _scrollDelta.y;
        }

        state.mouse = _inputData;
    }
}
