using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.Assertions;

public class BoolArrayHelper
{
    public static List<int> GetTrueIndices(bool[] arr)
    {
        List<int> indices = new List<int>();
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i])
            {
                indices.Add(i);
            }
        }
        return indices;
    }
}

public class XRInputHelper
{
    XRIDefaultInputActions _inputActions;
    ButtonInputData _inputData = new ButtonInputData();
    const int NUM_BUTTONS = 4;
    bool[] _buttonHeld = new bool[4];

    public XRInputHelper()
    {
        for (int buttonId = 0; buttonId < NUM_BUTTONS; buttonId++)
        {
            _buttonHeld[buttonId] = false;
        }
        OnEndFrame();

        _inputActions = new XRIDefaultInputActions();
        _inputActions.Enable();
        _inputActions.XRILeftHandInteraction.Activate.performed += LeftActivateCallback;
        _inputActions.XRILeftHandInteraction.Activate.canceled += LeftActivateCallback;
        _inputActions.XRILeftHandInteraction.Select.performed += LeftSelectCallback;
        _inputActions.XRILeftHandInteraction.Select.canceled += LeftSelectCallback;
        _inputActions.XRIRightHandInteraction.Activate.performed += RightActivateCallback;
        _inputActions.XRIRightHandInteraction.Activate.canceled += RightActivateCallback;
        _inputActions.XRIRightHandInteraction.Select.performed += RightSelectCallback;
        _inputActions.XRIRightHandInteraction.Select.canceled += RightSelectCallback;
    }

    private void ButtonPressReleaseCallback(int buttonId, bool down)
    {
        Assert.IsTrue(buttonId >= 0 && buttonId < NUM_BUTTONS);
        if (down)
        {
            if (!_buttonHeld[buttonId])
            {
                _buttonHeld[buttonId] = true;
                _inputData.buttonDown.Add(buttonId);
            }
        }
        else
        {
            if (_buttonHeld[buttonId])
            {
                _buttonHeld[buttonId] = false;
                _inputData.buttonUp.Add(buttonId);
            }
        }
    }

    public ButtonInputData UpdateInputData()
    {
        _inputData.buttonHeld = BoolArrayHelper.GetTrueIndices(_buttonHeld);
        return _inputData;
    }
    public void OnEndFrame()
    {
        for (int buttonId = 0; buttonId < NUM_BUTTONS; buttonId++)
        {
            _inputData.buttonUp.Clear();
            _inputData.buttonDown.Clear();
        }
    }

    private void LeftActivateCallback(InputAction.CallbackContext obj)
    {
        ButtonPressReleaseCallback(0, obj.performed);
    }

    private void LeftSelectCallback(InputAction.CallbackContext obj)
    {
        ButtonPressReleaseCallback(1, obj.performed);
    }

    private void RightActivateCallback(InputAction.CallbackContext obj)
    {
        ButtonPressReleaseCallback(2, obj.performed);
    }

    private void RightSelectCallback(InputAction.CallbackContext obj)
    {
        ButtonPressReleaseCallback(3, obj.performed);
    }
}

public class XRInputHelperComponent : MonoBehaviour
{
#if UNITY_EDITOR
    // Start is called before the first frame update
    void Start()
    {
        XRDeviceSimulator xrDeviceSimulator = (XRDeviceSimulator)FindObjectOfType(typeof(XRDeviceSimulator), true);

        if (xrDeviceSimulator != null)
        {
            xrDeviceSimulator.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("XR Device Simulator not found in the scene!");
        }
    }
#endif
}
