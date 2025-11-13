using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [Serializable]
    public class MouseLook
    {
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;
        public bool smooth;
        public float smoothTime = 5f;
        public bool lockCursor = true;

        // Added clamping system from PlayerController
        [Header("Clamping Settings")]
        public bool clampByY = false;
        public Vector2 clampXaxis = new Vector2(-90f, 90f);
        public Vector2 clampYaxis = new Vector2(0f, 0f);

        private Quaternion m_CharacterTargetRot;
        private Quaternion m_CameraTargetRot;
        private bool m_cursorIsLocked = true;

        // Added clamping accumulators from PlayerController
        private float m_ClampX;
        private float m_ClampY;

        public void Init(Transform character, Transform camera)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
            m_ClampX = 0f;
            m_ClampY = 0f;
        }

        public void LookRotation(Transform character, Transform camera)
        {
            float mouseX = CrossPlatformInputManager.GetAxis("Mouse X") * XSensitivity;
            float mouseY = CrossPlatformInputManager.GetAxis("Mouse Y") * YSensitivity;

            // Use the same clamping system as PlayerController
            m_ClampX += mouseY;
            m_ClampY += mouseX;

            // Vertical clamping (X-axis)
            if (m_ClampX > clampXaxis.y)
            {
                m_ClampX = clampXaxis.y;
                mouseY = 0.0f;
                ClampXAxis(camera, clampXaxis.y);
            }
            else if (m_ClampX < clampXaxis.x)
            {
                m_ClampX = clampXaxis.x;
                mouseY = 0.0f;
                ClampXAxis(camera, clampXaxis.x);
            }

            // Horizontal clamping (Y-axis) - optional
            if (clampByY)
            {
                if (m_ClampY > clampYaxis.y)
                {
                    m_ClampY = clampYaxis.y;
                    mouseX = 0.0f;
                    ClampYAxis(character, clampYaxis.y);
                }
                else if (m_ClampY < clampYaxis.x)
                {
                    m_ClampY = clampYaxis.x;
                    mouseX = 0.0f;
                    ClampYAxis(character, clampYaxis.x);
                }
            }

            // Apply rotation using the same method as PlayerController
            if (smooth)
            {
                camera.localRotation *= Quaternion.Euler(-mouseY, 0f, 0f);
                character.localRotation *= Quaternion.Euler(0f, mouseX, 0f);

                // Additional smoothing if needed
                camera.localRotation = Quaternion.Slerp(camera.localRotation, camera.localRotation, smoothTime * Time.deltaTime);
                character.localRotation = Quaternion.Slerp(character.localRotation, character.localRotation, smoothTime * Time.deltaTime);
            }
            else
            {
                camera.Rotate(Vector3.left * mouseY);
                character.Rotate(Vector3.up * mouseX);
            }

            UpdateCursorLock();
        }

        // Added clamping methods from PlayerController
        private void ClampXAxis(Transform camera, float value)
        {
            Vector3 camEuler = camera.eulerAngles;
            camEuler.x = value;
            camera.eulerAngles = camEuler;
        }

        private void ClampYAxis(Transform character, float value)
        {
            Vector3 charEuler = character.eulerAngles;
            charEuler.y = value;
            character.eulerAngles = charEuler;
        }

        // Public methods to control clamping (for your GameManager)
        public void SetClampXAxis(Vector2 clampRange)
        {
            clampXaxis = clampRange;
        }

        public void SetClampYAxis(Vector2 clampRange)
        {
            clampYaxis = clampRange;
        }

        public void SetClampByY(bool enable)
        {
            clampByY = enable;
        }

        public void ResetClamping()
        {
            m_ClampX = 0f;
            m_ClampY = 0f;
        }

        public void SetCursorLock(bool value)
        {
            lockCursor = value;
            if (!lockCursor)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void UpdateCursorLock()
        {
            if (lockCursor)
                InternalLockUpdate();
        }

        private void InternalLockUpdate()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                m_cursorIsLocked = false;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                m_cursorIsLocked = true;
            }

            if (m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (!m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}