using System.Collections.Generic;
using UnityEngine;

public class InputTrackerKeyboard : IClientStateProducer
{
    // Physical keys from USB HID Usage Tables
    // https://www.usb.org/sites/default/files/documents/hut1_12v2.pdf, page 53
    Dictionary<KeyCode, int> KEY_MAP = new Dictionary<KeyCode, int>()
    {
        [KeyCode.A]             = 0x04,
        [KeyCode.B]             = 0x05,
        [KeyCode.C]             = 0x06,
        [KeyCode.D]             = 0x07,
        [KeyCode.E]             = 0x08,
        [KeyCode.F]             = 0x09,
        [KeyCode.G]             = 0x0A,
        [KeyCode.H]             = 0x0B,
        [KeyCode.I]             = 0x0C,
        [KeyCode.J]             = 0x0D,
        [KeyCode.K]             = 0x0E,
        [KeyCode.L]             = 0x0F,
        [KeyCode.M]             = 0x10,
        [KeyCode.N]             = 0x11,
        [KeyCode.O]             = 0x12,
        [KeyCode.P]             = 0x13,
        [KeyCode.Q]             = 0x14,
        [KeyCode.R]             = 0x15,
        [KeyCode.S]             = 0x16,
        [KeyCode.T]             = 0x17,
        [KeyCode.U]             = 0x18,
        [KeyCode.V]             = 0x19,
        [KeyCode.W]             = 0x1A,
        [KeyCode.X]             = 0x1B,
        [KeyCode.Y]             = 0x1C,
        [KeyCode.Z]             = 0x1D,
        [KeyCode.Alpha1]        = 0x1E,
        [KeyCode.Alpha2]        = 0x1F,
        [KeyCode.Alpha3]        = 0x20,
        [KeyCode.Alpha4]        = 0x21,
        [KeyCode.Alpha5]        = 0x22,
        [KeyCode.Alpha6]        = 0x23,
        [KeyCode.Alpha7]        = 0x24,
        [KeyCode.Alpha8]        = 0x25,
        [KeyCode.Alpha9]        = 0x26,
        [KeyCode.Alpha0]        = 0x27,
        [KeyCode.Return]        = 0x28,
        [KeyCode.Escape]        = 0x29,
        [KeyCode.Backspace]     = 0x2A,
        [KeyCode.Tab]           = 0x2B,
        [KeyCode.Space]         = 0x2C,
        [KeyCode.Minus]         = 0x2D,
        [KeyCode.Equals]        = 0x2E,
        [KeyCode.LeftBracket]   = 0x2F,
        [KeyCode.RightBracket]  = 0x30,
        [KeyCode.Backslash]     = 0x31,
        [KeyCode.Semicolon]     = 0x33,
        [KeyCode.Quote]         = 0x34,
        [KeyCode.BackQuote]     = 0x35,
        [KeyCode.Comma]         = 0x36,
        [KeyCode.Period]        = 0x37,
        [KeyCode.Slash]         = 0x38,
        [KeyCode.CapsLock]      = 0x39,
        [KeyCode.F1]            = 0x3A,
        [KeyCode.F2]            = 0x3B,
        [KeyCode.F3]            = 0x3C,
        [KeyCode.F4]            = 0x3D,
        [KeyCode.F5]            = 0x3E,
        [KeyCode.F6]            = 0x3F,
        [KeyCode.F7]            = 0x40,
        [KeyCode.F8]            = 0x41,
        [KeyCode.F9]            = 0x42,
        [KeyCode.F10]           = 0x43,
        [KeyCode.F11]           = 0x44,
        [KeyCode.F12]           = 0x45,
        [KeyCode.Print]         = 0x46,
        [KeyCode.ScrollLock]    = 0x47,
        [KeyCode.Pause]         = 0x48,
        [KeyCode.Insert]        = 0x49,
        [KeyCode.Home]          = 0x4A,
        [KeyCode.PageUp]        = 0x4B,
        [KeyCode.Delete]        = 0x4C,
        [KeyCode.End]           = 0x4D,
        [KeyCode.PageDown]      = 0x4E,
        [KeyCode.RightArrow]    = 0x4F,
        [KeyCode.LeftArrow]     = 0x50,
        [KeyCode.DownArrow]     = 0x51,
        [KeyCode.UpArrow]       = 0x52,
        [KeyCode.Numlock]       = 0x53,
        [KeyCode.KeypadDivide]  = 0x54,
        [KeyCode.KeypadMultiply]= 0x55,
        [KeyCode.KeypadMinus]   = 0x56,
        [KeyCode.KeypadPlus]    = 0x57,
        [KeyCode.KeypadEnter]   = 0x58,
        [KeyCode.Keypad1]       = 0x59,
        [KeyCode.Keypad2]       = 0x5A,
        [KeyCode.Keypad3]       = 0x5B,
        [KeyCode.Keypad4]       = 0x5C,
        [KeyCode.Keypad5]       = 0x5D,
        [KeyCode.Keypad6]       = 0x5E,
        [KeyCode.Keypad7]       = 0x5F,
        [KeyCode.Keypad8]       = 0x60,
        [KeyCode.Keypad9]       = 0x61,
        [KeyCode.Keypad0]       = 0x62,
        [KeyCode.KeypadPeriod]  = 0x63,
        [KeyCode.KeypadEquals]  = 0x67,
        [KeyCode.F13]           = 0x68,
        [KeyCode.F14]           = 0x69,
        [KeyCode.F15]           = 0x6A,
        [KeyCode.Help]          = 0x75,
        [KeyCode.Menu]          = 0x76,
    };
    const int KEY_COUNT = (int)KeyCode.Menu + 1;

    ButtonInputData _inputData = new ButtonInputData();

    int[] _keyMap;
    bool[] _buttonHeld;
    bool[] _buttonUp;
    bool[] _buttonDown;

    public InputTrackerKeyboard()
    {
        // Bake the dict into an array for faster lookups.
        _keyMap = new int[KEY_COUNT];
        foreach (var kv in KEY_MAP)
        {
            _keyMap[(int)kv.Key] = kv.Value;
        }

        // Create arrays that hold whether keys were pressed since the last OnEndFrame() call.
        _buttonHeld = new bool[KEY_COUNT];
        _buttonUp = new bool[KEY_COUNT];
        _buttonDown = new bool[KEY_COUNT];
    }

    public void Update()
    {
        // Record all keys that were pressed or released since the last OnEndFrame() call.
        // Note that multiple Unity frames may occur during that time.
        for (int i = 0; i < KEY_COUNT; i++)
        {
            KeyCode key = (KeyCode)i;
            if (Input.GetKey(key))
            {
                _buttonHeld[i] = true;
            }
            if (Input.GetKeyDown(key))
            {
                _buttonDown[i] = true;
            }
            else if (Input.GetKeyUp(key))
            {
                _buttonUp[i] = true;
                // Don't count the key as down if it was released.
                _buttonHeld[i] = false;
            }
        }
    }

    public void OnEndFrame()
    {
        System.Array.Clear(_buttonHeld, 0, _buttonHeld.Length);
        System.Array.Clear(_buttonUp, 0, _buttonUp.Length);
        System.Array.Clear(_buttonDown, 0, _buttonDown.Length);
    }

    public void UpdateClientState(ref ClientState state)
    {
        _inputData.buttonHeld.Clear();
        _inputData.buttonUp.Clear();
        _inputData.buttonDown.Clear();

        for (int i = 0; i < KEY_COUNT; ++i)
        {
            int key = _keyMap[i];

            // Omit keys that were not used.
            if (_buttonHeld[i]) _inputData.buttonHeld.Add(key);
            if (_buttonUp[i]) _inputData.buttonUp.Add(key);
            if (_buttonDown[i]) _inputData.buttonDown.Add(key);
        }
        state.input = _inputData;
    }
}
