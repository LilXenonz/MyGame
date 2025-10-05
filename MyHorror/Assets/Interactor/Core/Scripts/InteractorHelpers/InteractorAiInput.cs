using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public static class InteractorAiInput
{
    private const int inputPlayers = 4;
    private static Transform[] _interactorAis;
    private static bool[] _onMove;
    private static string[] _horizontalNames;
    private static float[] _horizontals;
    private static string[] _verticalNames;
    private static float[] _verticals;
    private static KeyCode[] _runButtons;
    private static string[] _runButtonStrings;
    private static bool[] _runValues;
    private static KeyCode[] _forwardButtons;
    private static bool[] _forwardValues;
    private static string[] _forwardButtonStrings;
    private static float tempValue;

    #region Get Functions For User
    public static float GetAxis(string axis)
    {
        if (!AnyOnMove()) return GetAxisUnity(axis);

        for (int i = 0; i < _interactorAis.Length; i++)
        {
            if (!_interactorAis[i]) continue;

            if (string.Equals(axis, _verticalNames[i]))
            {
                tempValue = _verticals[i];
                _verticals[i] = 0;
                return tempValue;
            }
            else if (string.Equals(axis, _horizontalNames[i]))
            {
                tempValue = _horizontals[i];
                _horizontals[i] = 0;
                return tempValue;
            }
        }
        return GetAxisUnity(axis);
    }
    public static float GetAxisRaw(string axis)
    {
        if (!AnyOnMove()) return GetAxisRawUnity(axis);

        for (int i = 0; i < _interactorAis.Length; i++)
        {
            if (!_interactorAis[i]) continue;

            if (string.Equals(axis, _verticalNames[i]))
            {
                tempValue = Mathf.Clamp(Mathf.Round(_verticals[i]), -1f, 1f);
                _verticals[i] = 0;
                return tempValue;
            }
            else if (string.Equals(axis, _horizontalNames[i]))
            {
                tempValue = Mathf.Clamp(Mathf.Round(_horizontals[i]), -1f, 1f);
                _horizontals[i] = 0;
                return tempValue;
            }
        }
        return GetAxisRawUnity(axis);
    }

    public static bool GetKey(KeyCode keycode)
    {
        if (!AnyOnMove()) return GetKeyUnity(keycode);

        for (int i = 0; i < _interactorAis.Length; i++)
        {
            if (!_interactorAis[i]) continue;

            if (_runButtons[i] == keycode) return _runValues[i];
            else if (_forwardButtons[i] == keycode) return _forwardValues[i];
        }

        return GetKeyUnity(keycode);
    }
    public static bool GetButton(string button)
    {
        if (!AnyOnMove()) return GetButtonUnity(button);

        for (int i = 0; i < _interactorAis.Length; i++)
        {
            if (!_interactorAis[i]) continue;

            if (string.Equals(button, _runButtonStrings[i])) return _runValues[i];
            else if (string.Equals(button, _forwardButtonStrings[i])) return _forwardValues[i];
        }

        return GetButtonUnity(button);
    }
    #endregion

    //Methods below for InteractorAI to use
    public static int AddInteractorAi(Transform interactorAi, string vertical, string horizontal, KeyCode runButton, string runButtonString, KeyCode forwardButton, string forwardButtonString)
    {
        int index = ArrangeSlot(interactorAi);
        if (index < 0)
        {
            Debug.Log("All 4 slots are full, increase the inputPlayers on InteractorAiInput script.");
            return -1;
        }

        _interactorAis[index] = interactorAi;
        _verticalNames[index] = vertical;
        _horizontalNames[index] = horizontal;
        _runButtons[index] = runButton;
        _runButtonStrings[index] = runButtonString;
        _forwardButtons[index] = forwardButton;
        _forwardButtonStrings[index] = forwardButtonString;
        return index;
    }
    private static int ArrangeSlot(Transform interactorAi)
    {
        if (_interactorAis == null) Init();

        for (int i = 0; i < _interactorAis.Length; i++)
        {
            if (interactorAi == _interactorAis[i]) return i;
        }
        for (int i = 0; i < _interactorAis.Length; i++)
        {
            if (!_interactorAis[i]) return i;
        }
        return -1;
    }
    public static void RemoveInteractorAi(int index)
    {
        if (index < _interactorAis.Length)
        {
            Reset(index);
            _interactorAis[index] = null;
        }
    }
    public static bool CheckUserInput(bool both, int index)
    {
        if (both && (GetAxisUnity(_verticalNames[index]) + GetAxisUnity(_horizontalNames[index]) == 0)) return false;
        else if (!both && GetAxisUnity(_verticalNames[index]) == 0) return false;

        return true;
    }
    private static bool AnyOnMove()
    {
        if (_onMove == null) return false;

        for (int i = 0; i < _onMove.Length; i++)
        {
            if (!_interactorAis[i]) continue;
            if (_onMove[i]) return true;
        }
        return false;
    }
    public static void Init()
    {
        _interactorAis = new Transform[inputPlayers];
        _onMove = new bool[inputPlayers];
        _verticalNames = new string[inputPlayers];
        _verticals = new float[inputPlayers];
        _horizontalNames = new string[inputPlayers];
        _horizontals = new float[inputPlayers];
        _runButtons = new KeyCode[inputPlayers];
        _runButtonStrings = new string[inputPlayers];
        _runValues = new bool[inputPlayers];
        _forwardButtons = new KeyCode[inputPlayers];
        _forwardButtonStrings = new string[inputPlayers];
        _forwardValues = new bool[inputPlayers];
    }
    public static void Reset(int index)
    {
        _verticals[index] = 0;
        _horizontals[index] = 0;
        _runValues[index] = false;
        _forwardValues[index] = false;
        _onMove[index] = false;
    }
    #region SetMethods
    public static void SetVertical(float v, int index)
    {
        _onMove[index] = true;
        _verticals[index] = v;
    }
    public static void SetHorizontal(float h, int index)
    {
        _onMove[index] = true;
        _horizontals[index] = h;
    }
    public static void SetForwardButton(bool value, int index)
    {
        _onMove[index] = true;
        _forwardValues[index] = value;
    }
    public static void SetRunButton(bool value, int index)
    {
        _runValues[index] = value;
    }
    #endregion

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
    private static InputAction horizontalAction;
    private static InputAction verticalAction;
    private static InputAction CreateAxisAction(string axisName)
    {
        InputAction action = null;

        if (axisName == "Horizontal")
        {
            if (horizontalAction == null)
            {
                horizontalAction = new InputAction("Horizontal");
                horizontalAction.AddCompositeBinding("1DAxis")
                    .With("Positive", "<Keyboard>/d")
                    .With("Negative", "<Keyboard>/a");
                horizontalAction.Enable();
            }
            action = horizontalAction;
        }
        else if (axisName == "Vertical")
        {
            if (verticalAction == null)
            {
                verticalAction = new InputAction("Vertical");
                verticalAction.AddCompositeBinding("1DAxis")
                    .With("Positive", "<Keyboard>/w")
                    .With("Negative", "<Keyboard>/s");
                verticalAction.Enable();
            }
            action = verticalAction;
        }

        return action;
    }

    public static float GetAxisUnity(string axis)
    {
        var action = CreateAxisAction(axis);
        return action != null ? action.ReadValue<float>() : 0f;
    }

    public static float GetAxisRawUnity(string axis)
    {
        var action = CreateAxisAction(axis);
        if (action != null)
        {
            float value = action.ReadValue<float>();
            return value > 0 ? 1 : (value < 0 ? -1 : 0);
        }
        return 0;
    }

    private static bool GetKeyUnity(KeyCode keyCode)
    {
        var key = keyCode.ToString().ToLower();
        var keyboard = Keyboard.current;
        return keyboard != null && keyboard[key].IsPressed();
    }

    private static bool GetButtonUnity(string button)
    {
        var action = new InputAction(button, InputActionType.Button);
        if (button == "Jump")
        {
            action.AddBinding("<Keyboard>/space");
        }
        else if (button == "Fire1")
        {
            action.AddBinding("<Mouse>/leftButton");
        }
        action.Enable();
        return action.triggered;
    }
#else
    private static float GetAxisUnity(string axis)
    {
        return Input.GetAxis(axis);
    }
    private static float GetAxisRawUnity(string axis)
    {
        return Input.GetAxisRaw(axis);
    }
    private static bool GetKeyUnity(KeyCode keyCode)
    {
        return Input.GetKey(keyCode);
    }
    private static bool GetButtonUnity(string button)
    {
        return Input.GetButton(button);
    }
#endif
}
