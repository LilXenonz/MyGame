using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
using OldKeyCode = UnityEngine.KeyCode;
using NewKey = UnityEngine.InputSystem.Key;
#endif

namespace razz
{
    public static class InteractorInput
    {//Input wrapper class to both handle new and old Input systems

        private static bool _workOutOfFocus;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER

        public static void SetWorkOutOfFocus(bool value)
        {
            _workOutOfFocus = value;
        }

        public static float GetHorizontal()
        {
            if (!_workOutOfFocus && !Application.isFocused) return 0;
            return InteractorAiInput.GetAxis("Horizontal");
        }

        public static float GetVertical()
        {
            if (!_workOutOfFocus && !Application.isFocused) return 0;
            return InteractorAiInput.GetAxis("Vertical");
        }

        public static Vector2 GetMousePosition()
        {
#if UNITY_ANDROID || UNITY_IOS
            return Vector2.zero;
#else
            if (!_workOutOfFocus && !Application.isFocused) return Vector2.zero;
            return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#endif
        }

        public static int GetTouchCount()
        {
            return Touchscreen.current != null ? (Touchscreen.current.primaryTouch.press.isPressed ? 1 : 0) : 0;
        }

        public static Vector2 GetTouchPosition()
        {
            if (!_workOutOfFocus && !Application.isFocused) return Vector2.zero;
            return Touchscreen.current != null ? Touchscreen.current.primaryTouch.position.ReadValue() : Vector2.zero;
        }

        public static bool GetLeftClick()
        {
            if (!_workOutOfFocus && !Application.isFocused) return false;
#if UNITY_ANDROID || UNITY_IOS
            return false;
#else
            return Mouse.current.leftButton.wasPressedThisFrame;
#endif
        }

        public static bool GetRightClickRelease()
        {
            if (!_workOutOfFocus && !Application.isFocused) return false;
#if UNITY_ANDROID || UNITY_IOS
            return false;
#else
            return Mouse.current.rightButton.wasReleasedThisFrame;
#endif
        }

        public static float GetMouseX()
        {
            if (!_workOutOfFocus && !Application.isFocused) return 0;
#if UNITY_ANDROID || UNITY_IOS
            return 0;
#else
            return Mouse.current.delta.x.ReadValue() * Time.deltaTime * 8f;
#endif
        }

        public static float GetMouseY()
        {
            if (!_workOutOfFocus && !Application.isFocused) return 0;
#if UNITY_ANDROID || UNITY_IOS
            return 0;
#else
            return Mouse.current.delta.y.ReadValue() * Time.deltaTime * 8f;
#endif
        }

        public static float GetMouseWheel()
        {
            if (!_workOutOfFocus && !Application.isFocused) return 0;
#if UNITY_ANDROID || UNITY_IOS
            return 0;
#else
            return Mouse.current?.scroll.y.ReadValue() ?? 0f;
#endif
        }

        public static bool GetKey(OldKeyCode keyCode)
        {
            if (!_workOutOfFocus && !Application.isFocused) return false;

            var keyboard = Keyboard.current;
            if (keyboard == null) return false;

            NewKey newKey = ConvertKeyCode(keyCode);
            if (newKey == NewKey.None) return false;

            var control = keyboard[newKey];
            return control != null && control.isPressed;
        }

        public static bool GetKeyDown(OldKeyCode keyCode)
        {
            if (!_workOutOfFocus && !Application.isFocused) return false;

            var keyboard = Keyboard.current;
            if (keyboard == null) return false;

            NewKey newKey = ConvertKeyCode(keyCode);
            if (newKey == NewKey.None) return false;

            var control = keyboard[newKey];
            return control != null && control.wasPressedThisFrame;
        }

        private static NewKey ConvertKeyCode(OldKeyCode keyCode)
        {
            switch (keyCode)
            {
                case OldKeyCode.A: return NewKey.A;
                case OldKeyCode.B: return NewKey.B;
                case OldKeyCode.C: return NewKey.C;
                case OldKeyCode.D: return NewKey.D;
                case OldKeyCode.E: return NewKey.E;
                case OldKeyCode.F: return NewKey.F;
                case OldKeyCode.G: return NewKey.G;
                case OldKeyCode.H: return NewKey.H;
                case OldKeyCode.I: return NewKey.I;
                case OldKeyCode.J: return NewKey.J;
                case OldKeyCode.K: return NewKey.K;
                case OldKeyCode.L: return NewKey.L;
                case OldKeyCode.M: return NewKey.M;
                case OldKeyCode.N: return NewKey.N;
                case OldKeyCode.O: return NewKey.O;
                case OldKeyCode.P: return NewKey.P;
                case OldKeyCode.Q: return NewKey.Q;
                case OldKeyCode.R: return NewKey.R;
                case OldKeyCode.S: return NewKey.S;
                case OldKeyCode.T: return NewKey.T;
                case OldKeyCode.U: return NewKey.U;
                case OldKeyCode.V: return NewKey.V;
                case OldKeyCode.W: return NewKey.W;
                case OldKeyCode.X: return NewKey.X;
                case OldKeyCode.Y: return NewKey.Y;
                case OldKeyCode.Z: return NewKey.Z;

                case OldKeyCode.Space: return NewKey.Space;
                case OldKeyCode.Escape: return NewKey.Escape;
                case OldKeyCode.Return: return NewKey.Enter;
                case OldKeyCode.Backspace: return NewKey.Backspace;
                case OldKeyCode.Tab: return NewKey.Tab;
                case OldKeyCode.LeftShift: return NewKey.LeftShift;
                case OldKeyCode.RightShift: return NewKey.RightShift;
                case OldKeyCode.LeftControl: return NewKey.LeftCtrl;
                case OldKeyCode.RightControl: return NewKey.RightCtrl;
                case OldKeyCode.LeftAlt: return NewKey.LeftAlt;
                case OldKeyCode.RightAlt: return NewKey.RightAlt;

                case OldKeyCode.UpArrow: return NewKey.UpArrow;
                case OldKeyCode.DownArrow: return NewKey.DownArrow;
                case OldKeyCode.LeftArrow: return NewKey.LeftArrow;
                case OldKeyCode.RightArrow: return NewKey.RightArrow;

                case OldKeyCode.Alpha0: return NewKey.Digit0;
                case OldKeyCode.Alpha1: return NewKey.Digit1;
                case OldKeyCode.Alpha2: return NewKey.Digit2;
                case OldKeyCode.Alpha3: return NewKey.Digit3;
                case OldKeyCode.Alpha4: return NewKey.Digit4;
                case OldKeyCode.Alpha5: return NewKey.Digit5;
                case OldKeyCode.Alpha6: return NewKey.Digit6;
                case OldKeyCode.Alpha7: return NewKey.Digit7;
                case OldKeyCode.Alpha8: return NewKey.Digit8;
                case OldKeyCode.Alpha9: return NewKey.Digit9;

                default: return NewKey.None;
            }
        }

#else
        public static void SetWorkOutOfFocus(bool value)
        {
            _workOutOfFocus = value;
        }

        public static float GetHorizontal()
        {
            return InteractorAiInput.GetAxis("Horizontal");
        }

        public static float GetVertical()
        {
            return InteractorAiInput.GetAxis("Vertical");
        }

        public static Vector2 GetMousePosition()
        {
#if UNITY_ANDROID || UNITY_IOS
            return Vector2.zero;
#else
            return Input.mousePosition;
#endif
        }

        public static int GetTouchCount()
        {
            return Input.touchCount;
        }

        public static Vector2 GetTouchPosition()
        {
            return Input.touchCount > 0 ? Input.GetTouch(0).position : Vector2.zero;
        }

        public static bool GetLeftClick()
        {
#if UNITY_ANDROID || UNITY_IOS
            return false;
#else
            return Input.GetKeyDown(KeyCode.Mouse0);
#endif
        }

        public static bool GetRightClickRelease()
        {
#if UNITY_ANDROID || UNITY_IOS
            return false;
#else
            return Input.GetKeyUp(KeyCode.Mouse1);
#endif
        }

        public static float GetMouseX()
        {
#if UNITY_ANDROID || UNITY_IOS
            return 0;
#else
            return Input.GetAxis("Mouse X");
#endif
        }

        public static float GetMouseY()
        {
#if UNITY_ANDROID || UNITY_IOS
            return 0;
#else
            return Input.GetAxis("Mouse Y");
#endif
        }

        public static float GetMouseWheel()
        {
#if UNITY_ANDROID || UNITY_IOS
            return 0;
#else
            return Input.GetAxis("Mouse ScrollWheel");
#endif
        }

        public static bool GetKey(KeyCode keyCode)
        {
            return Input.GetKey(keyCode);
        }

        public static bool GetKeyDown(KeyCode keyCode)
        {
            return Input.GetKeyDown(keyCode);
        }
#endif
    }
}
