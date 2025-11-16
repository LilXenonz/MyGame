using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using UnityEngine.UI;

public class FirstPersonController : MonoBehaviour
{

    [Header("GeneralSettings")]
    [Tooltip("Object with GameControll.cs script")]
    public GameManager GameManager;
    [HideInInspector]
    public Inventory inventory;

    [Header("PlayerSettings")]
    [Tooltip("Player walk speed")]
    public float walkSpeed;
    [Tooltip("Player crouch speed")]
    public float crouchSpeed;
    [Tooltip("Player run speed")]
    public float runSpeed = 7f;
    [HideInInspector]
    public bool locked;
    [HideInInspector]
    public bool lockedMovement;
    [HideInInspector]
    public bool canBeCatchen;
    private CharacterController characterController;
    private float moveSpeed;
    [HideInInspector]
    public bool playerMoving;
    private bool m_IsWalking = true;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;

    [Header("CameraSettings")]
    [Tooltip("Mouse Sensetivity value")]
    public float mouseSensetivity;
    [Tooltip("Main camera transform")]
    public Transform cameraTransform;
    public Camera CameraGB;
    private float clampX;
    private float clampY;

    [Header("HeadBob Settings")]
    [SerializeField] private bool m_UseHeadBob = true;
    [SerializeField] private float m_StepInterval;
    [SerializeField] private float m_RunstepLenghten = 0.7f;
    private Vector3 m_OriginalCameraPosition;
    private float m_StepCycle;
    private float m_NextStep;
    private CurveControlledBob m_HeadBob = new CurveControlledBob();

    // store headbob baseline (whatever DoHeadBob(0) returns) so we can compute relative offset
    private Vector3 m_HeadBobBaseline;

    [Header("FOV Kick Settings")]
    [SerializeField] private bool m_UseFovKick = true;
    [SerializeField] private FOVKick m_FovKick = new FOVKick();

    [Header("CrouchSettings")]
    private float lerpSpeed = 10f;
    [Tooltip("Player character controller normal height")]
    public float normalHeight;
    [Tooltip("Player character controller crouch height")]
    public float crouchHeight;
    [Tooltip("Player camera normal offset")]
    public float cameraNormalOffset;
    [Tooltip("Player camera crouch offset")]
    public float cameraCrouchOffset;
    [Tooltip("Player obstacle layers")]
    public LayerMask obstacleLayers;
    [Tooltip("Clamp camera by Y axis")]
    public bool clampByY;
    public Vector2 clampXaxis;
    public Vector2 clampYaxis;
    [Tooltip("Time takes to repair broken legs")]
    public float legsFixTime;
    [HideInInspector]
    public bool crouch = false;

    [Header("Hide Place Settings")]
    public HidePlace hidePlace;

    [Header("Sounds Settings")]
    private AudioSource AS;
    [Tooltip("Foot steps sounds")]
    public AudioClip[] footSteps;
    [Tooltip("Sound of breaking legs")]
    public AudioClip legBreakSound;
    [Tooltip("Land sound")]
    public AudioClip landSound;
    private bool legBreak;

    [Header("Rotation Settings")]
    public bool m_BlockLook = false;
    public bool m_BlockMovement = false;
    private Coroutine m_LerpCoroutine = null;

    // --- camera smoothing / targets ---
    private float m_CrouchOffsetTarget;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        AS = GetComponent<AudioSource>();
        inventory = GetComponent<Inventory>();
        characterController = GetComponent<CharacterController>();
        clampX = 0f;
        moveSpeed = walkSpeed;

        // Initialize camera baseline & headbob system
        m_OriginalCameraPosition = cameraTransform.localPosition;
        m_CrouchOffsetTarget = cameraNormalOffset; // start at standing offset

        m_HeadBob.Setup(CameraGB, m_StepInterval);
        // store baseline returned by DoHeadBob(0) so we can compute relative offsets later
        m_HeadBobBaseline = m_HeadBob.DoHeadBob(0f);

        m_StepCycle = 0f;
        m_NextStep = m_StepCycle / 2f;

        // Initialize FOV kick
        if (m_UseFovKick)
        {
            m_FovKick.Setup(cameraTransform.GetComponent<Camera>());
        }
    }

    private void Update()
    {
        canBeCatchen = characterController.isGrounded;

        if (!locked)
        {
            CameraRotation();
            HidePlaceExit();
            if (!lockedMovement && !m_BlockMovement)
            {
                Movement();
                Controll();
            }
        }

        // Handle landing sound
        if (!m_PreviouslyGrounded && characterController.isGrounded)
        {
            PlayLandingSound();
        }
        m_PreviouslyGrounded = characterController.isGrounded;
    }

    private void FixedUpdate()
    {
        // Fixed update for physics-based movement
        if (!locked && !lockedMovement && !m_BlockMovement)
        {
            ProgressStepCycle(moveSpeed);
            // camera updates moved to LateUpdate to avoid race conflicts
        }
    }

    private void Controll()
    {
        if (!characterController.isGrounded && characterController.velocity.y <= -7f && !legBreak)
        {
            legBreak = true;
        }
        else
        {
            if (characterController.isGrounded && legBreak)
            {
                legBreak = false;
                PlayerLegsBreak();
            }
        }

        float newHeight = crouch ? crouchHeight : normalHeight;
        characterController.height = Mathf.Lerp(characterController.height, newHeight, Time.deltaTime * lerpSpeed);

        characterController.center = Vector3.down * (normalHeight - characterController.height) / 2.0f;

        // Instead of writing cameraTransform.localPosition here, only update the target.
        m_CrouchOffsetTarget = crouch ? cameraCrouchOffset : cameraNormalOffset;

        if (characterController.isGrounded && CrossPlatformInputManager.GetButtonDown("Crouch"))
        {
            SetCrouch();
        }

        if (CrossPlatformInputManager.GetButtonDown("Drop"))
        {
            GameManager.inventory.DropItem();
        }

        // Handle run/walk toggle
#if !MOBILE_INPUT
        if (Input.GetKeyDown(KeyCode.LeftShift) && !crouch)
        {
            m_IsWalking = !m_IsWalking;
            moveSpeed = m_IsWalking ? walkSpeed : runSpeed;

            // Trigger FOV kick when changing speed
            if (m_UseFovKick && characterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }
        }
#endif
    }

    private void PlayerLegsBreak()
    {
        GameManager.ScreenBlood(1);
        lockedMovement = true;
        crouch = true;
        moveSpeed = crouchSpeed;
        m_IsWalking = true;
        characterController.height = crouchHeight;

        // Set target and snap baseline so headbob doesn't pop
        m_CrouchOffsetTarget = cameraCrouchOffset;
        cameraTransform.localPosition = new Vector3(m_OriginalCameraPosition.x, m_CrouchOffsetTarget + m_OriginalCameraPosition.y, m_OriginalCameraPosition.z);
        m_OriginalCameraPosition = cameraTransform.localPosition;

        // re-setup headbob and recompute baseline
        m_HeadBob.Setup(CameraGB, m_StepInterval);
        m_HeadBobBaseline = m_HeadBob.DoHeadBob(0f);

        AS.PlayOneShot(legBreakSound);
        StartCoroutine(WaitLegsFix());
    }

    public void Hide(int state)
    {
        if (state == 1)
        {
            if (GameManager.enemy.seePlayer)
            {
                GameManager.enemy.SendHidePlace();
            }
            StopAllCoroutines();
            lockedMovement = true;
            crouch = true;
            characterController.height = crouchHeight;

            // Set target and snap baseline
            m_CrouchOffsetTarget = cameraCrouchOffset;
            cameraTransform.localPosition = new Vector3(m_OriginalCameraPosition.x, m_CrouchOffsetTarget + m_OriginalCameraPosition.y, m_OriginalCameraPosition.z);
            m_OriginalCameraPosition = cameraTransform.localPosition;

            // re-setup headbob baseline
            m_HeadBob.Setup(CameraGB, m_StepInterval);
            m_HeadBobBaseline = m_HeadBob.DoHeadBob(0f);
        }
        else
        {
            lockedMovement = false;
            crouch = false;
            moveSpeed = walkSpeed;
            m_IsWalking = true;
            characterController.height = normalHeight;

            // Set target and snap baseline to normal
            m_CrouchOffsetTarget = cameraNormalOffset;
            cameraTransform.localPosition = new Vector3(m_OriginalCameraPosition.x, m_CrouchOffsetTarget + m_OriginalCameraPosition.y, m_OriginalCameraPosition.z);
            m_OriginalCameraPosition = cameraTransform.localPosition;

            // re-setup headbob baseline
            m_HeadBob.Setup(CameraGB, m_StepInterval);
            m_HeadBobBaseline = m_HeadBob.DoHeadBob(0f);
        }
    }

    public void CatchPlayer(int state, string camHitName)
    {
        if (state == 1)
        {
            StopAllCoroutines();
            GameManager.inventory.DropItem();
            locked = true;
            characterController.height = normalHeight;

            // Snap camera to normal baseline
            m_CrouchOffsetTarget = cameraNormalOffset;
            cameraTransform.localPosition = new Vector3(m_OriginalCameraPosition.x, m_CrouchOffsetTarget + m_OriginalCameraPosition.y, m_OriginalCameraPosition.z);
            m_OriginalCameraPosition = cameraTransform.localPosition;

            // re-setup headbob baseline
            m_HeadBob.Setup(CameraGB, m_StepInterval);
            m_HeadBobBaseline = m_HeadBob.DoHeadBob(0f);

            crouch = false;
            moveSpeed = walkSpeed;
            m_IsWalking = true;
        }

        if (state == 2)
        {
            GameManager.ScreenFade(2);
        }

        if (state == 3)
        {
            GameManager.inventory.DropItem();
            locked = true;
            crouch = false;
            moveSpeed = walkSpeed;
            m_IsWalking = true;

            // Snap camera to normal baseline
            m_CrouchOffsetTarget = cameraNormalOffset;
            cameraTransform.localPosition = new Vector3(m_OriginalCameraPosition.x, m_CrouchOffsetTarget + m_OriginalCameraPosition.y, m_OriginalCameraPosition.z);
            m_OriginalCameraPosition = cameraTransform.localPosition;

            // re-setup headbob baseline
            m_HeadBob.Setup(CameraGB, m_StepInterval);
            m_HeadBobBaseline = m_HeadBob.DoHeadBob(0f);

            GameManager.ScreenFade(3);
        }

        if (state == 4)
        {
            GameManager.ScreenBlood(0);
        }
    }

    private void Movement()
    {
        if (m_BlockMovement) return;

        float inputX = CrossPlatformInputManager.GetAxis("Horizontal") * moveSpeed;
        float inputY = CrossPlatformInputManager.GetAxis("Vertical") * moveSpeed;

        Vector3 forvardMove = transform.forward * inputY;
        Vector3 sideMove = transform.right * inputX;

        // Use SimpleMove for gravity handling
        characterController.SimpleMove(forvardMove + sideMove);

        // Update player moving state
        if (characterController.velocity.magnitude > 0.5f)
        {
            playerMoving = true;
        }
        else
        {
            playerMoving = false;
        }
    }

    private void CameraRotation()
    {
        if (m_BlockLook) return;

        float mouseX = CrossPlatformInputManager.GetAxis("Mouse X") * (mouseSensetivity * 2) * Time.deltaTime;
        float mouseY = CrossPlatformInputManager.GetAxis("Mouse Y") * (mouseSensetivity * 2) * Time.deltaTime;

        clampX += mouseY;
        clampY += mouseX;

        if (clampX > clampXaxis.y)
        {
            clampX = clampXaxis.y;
            mouseY = 0.0f;
            ClampXAxis(clampXaxis.x);
        }
        else if (clampX < clampXaxis.x)
        {
            clampX = clampXaxis.x;
            mouseY = 0.0f;
            ClampXAxis(clampXaxis.y);
        }

        if (clampByY)
        {
            if (clampY > clampYaxis.y)
            {
                clampY = clampYaxis.y;
                mouseX = 0.0f;
                ClampYAxis(clampYaxis.y);
            }
            else if (clampY < clampYaxis.x)
            {
                clampY = clampYaxis.x;
                mouseX = 0.0f;
                ClampYAxis(clampYaxis.x);
            }
        }

        cameraTransform.Rotate(Vector3.left * mouseY);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void ProgressStepCycle(float speed)
    {
        if (characterController.velocity.sqrMagnitude > 0 && (CrossPlatformInputManager.GetAxis("Horizontal") != 0 || CrossPlatformInputManager.GetAxis("Vertical") != 0))
        {
            m_StepCycle += (characterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                         Time.fixedDeltaTime;
        }

        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        m_NextStep = m_StepCycle + m_StepInterval;

        PlayFootStepAudio();
    }

    private void PlayFootStepAudio()
    {
        if (!characterController.isGrounded)
        {
            return;
        }
        // pick & play a random footstep sound from the array,
        // excluding sound at index 0
        int n = Random.Range(1, footSteps.Length);
        AS.clip = footSteps[n];
        AS.PlayOneShot(AS.clip);
        // move picked sound to index 0 so it's not picked next time
        footSteps[n] = footSteps[0];
        footSteps[0] = AS.clip;
    }

    private void PlayLandingSound()
    {
        if (landSound != null)
        {
            AS.clip = landSound;
            AS.Play();
            m_NextStep = m_StepCycle + .5f;
        }
    }

    // NOTE: All camera-local-position writes are done here (LateUpdate) to avoid race conditions with FixedUpdate/Update.
    private void LateUpdate()
    {
        UpdateCameraPositionSmooth(moveSpeed);
    }

    // Compute headbob relative offset (DoHeadBob() - baseline) then add to baseline + crouch offset.
    private void UpdateCameraPositionSmooth(float speed)
    {
        // Base local position (original baseline x,z and baseline y)
        Vector3 basePos = new Vector3(m_OriginalCameraPosition.x, m_OriginalCameraPosition.y + m_CrouchOffsetTarget, m_OriginalCameraPosition.z);

        if (!m_UseHeadBob)
        {
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, basePos, Time.deltaTime * lerpSpeed);
            return;
        }

        if (characterController.velocity.magnitude > 0 && characterController.isGrounded)
        {
            // Get current headbob value and subtract baseline to get the *relative* bob offset
            Vector3 currentBob = m_HeadBob.DoHeadBob(characterController.velocity.magnitude +
                                                    (speed * (m_IsWalking ? 1f : m_RunstepLenghten)));

            Vector3 bobRelative = currentBob - m_HeadBobBaseline;

            Vector3 targetPos = basePos + bobRelative;

            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, targetPos, Time.deltaTime * lerpSpeed);
        }
        else
        {
            // Not moving: lerp back to base position
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, basePos, Time.deltaTime * lerpSpeed);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }
        body.AddForceAtPosition(characterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }

    public void LerpRotation(Transform target, float duration = 0.5f, bool keepLookLocked = false)
    {
        if (target == null) return;

        // Stop previous rotation coroutine if running
        if (m_LerpCoroutine != null)
        {
            StopCoroutine(m_LerpCoroutine);
            m_LerpCoroutine = null;
        }

        m_LerpCoroutine = StartCoroutine(DoLerpRotation(target, duration, keepLookLocked));
    }

    private IEnumerator DoLerpRotation(Transform target, float duration, bool keepLookLocked = false)
    {
        if (target == null)
            yield break;

        // Block normal mouse look
        m_BlockLook = true;

        // Store starting rotations
        Quaternion startRoot = transform.rotation;
        Quaternion startCamLocal = cameraTransform.localRotation;

        // Compute direction from camera to target to reduce parallax
        Vector3 directionToTarget = target.position - cameraTransform.position;
        if (directionToTarget.sqrMagnitude < 0.0001f)
        {
            m_BlockLook = keepLookLocked;
            m_LerpCoroutine = null;
            yield break;
        }

        Quaternion lookRot = Quaternion.LookRotation(directionToTarget.normalized);
        Vector3 euler = lookRot.eulerAngles;

        // Yaw (root) is only around Y axis
        Quaternion targetRoot = Quaternion.Euler(0f, euler.y, 0f);

        // Camera local pitch: set local X rotation to pitch. (Assumes camera local yaw/roll are negligible.)
        Quaternion targetCamLocal = Quaternion.Euler(euler.x, 0f, 0f);

        // Snap immediately if duration <= 0
        if (duration <= 0f)
        {
            transform.rotation = targetRoot;
            cameraTransform.localRotation = targetCamLocal;
            m_BlockLook = keepLookLocked;
            m_LerpCoroutine = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // smoother easing
            float eased = t * t * (3f - 2f * t);

            transform.rotation = Quaternion.Slerp(startRoot, targetRoot, eased);
            cameraTransform.localRotation = Quaternion.Slerp(startCamLocal, targetCamLocal, eased);

            yield return null;
        }

        // Ensure final values
        transform.rotation = targetRoot;
        cameraTransform.localRotation = targetCamLocal;

        // keep or release lock
        m_BlockLook = keepLookLocked;

        m_LerpCoroutine = null;
    }

    private void ClampXAxis(float value)
    {
        Vector3 camEuler = cameraTransform.eulerAngles;
        camEuler.x = value;
        cameraTransform.eulerAngles = camEuler;
    }

    private void ClampYAxis(float value)
    {
        Vector3 camEuler = transform.eulerAngles;
        camEuler.y = value;
        transform.eulerAngles = camEuler;
    }

    private void SetCrouch()
    {
        if (!crouch)
        {
            crouch = true;
            moveSpeed = crouchSpeed;
            m_IsWalking = true;
        }
        else
        {
            if (CheckDistance() > normalHeight)
            {
                crouch = false;
                moveSpeed = walkSpeed;
                m_IsWalking = true;
            }
        }
    }

    private float CheckDistance()
    {
        Vector3 pos = transform.position + characterController.center - new Vector3(0, characterController.height / 2, 0);
        RaycastHit hit;
        if (Physics.SphereCast(pos, characterController.radius, transform.up, out hit, 10, obstacleLayers))
        {
            return hit.distance;
        }
        else
            return 3;
    }

    private void HidePlaceExit()
    {
        if (hidePlace)
        {
            if (CrossPlatformInputManager.GetButtonDown("Unhide"))
            {
                hidePlace.ExitHidePlace();
            }
        }
    }

    private IEnumerator WaitLegsFix()
    {
        yield return new WaitForSeconds(legsFixTime);
        lockedMovement = false;
        GameManager.ScreenBlood(0);
    }
}
