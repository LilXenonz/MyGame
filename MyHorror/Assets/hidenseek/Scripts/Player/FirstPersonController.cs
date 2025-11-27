using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.UI;


public class FirstPersonController : MonoBehaviour {

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
    public float runSpeed;
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

    [Header("Stamina Settings")]
    public Image m_staminaBar;
    public GameObject m_runArrownImage;
    public float m_stamina = 100;
    public float m_maxStamina = 100;
    [HideInInspector]
    public bool m_unlimitedStamina = false;
    public float m_staminaConsumptionSpeed;
    public float m_staminaRecoverySpeed;
    bool m_staminaRecovery;
    [HideInInspector]
    public bool m_running;

    [Header("CameraSettings")]
    [Tooltip("Mouse Sensetivity value")]
    public float mouseSensetivity;
    [Tooltip("Main camera transform")]
    public Transform cameraTransform;
    private float clampX;
    private float clampY;

    [Header("Camera Animations")]
    [Tooltip("Camera animation gameobject")]
    public Animation cameraAnimation;
    [Tooltip("Camera hit animation name")]
    public string cameraHitAnimName;
    [Tooltip("Camera idle animation name")]
    public string cameraIdleAnimName;
    [Tooltip("Camera move animation name")]
    public string cameraMoveAnimName;


    [Header("CrouchSettings")]
    
    private float lerpSpeed  = 10f;
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


    [Header("UI Settigns")]
    [Tooltip("UI stand icon for mobile only")]
    public Image imageStand;
    [Tooltip("UI crouch icon for mobile only")]
    public Image imageCrouch;
    [Tooltip("UI crouch icon for mobile only")]
    public Image imageExitHidePlace;

    [Header("Hide Place Settings")]
    public HidePlace hidePlace;


    [Header("Sounds Settings")]
    private AudioSource AS;
    [Tooltip("Foot steps sounds")]
    public AudioClip[] footSteps;
    [Tooltip("Sound of breaking legs")]
    public AudioClip legBreakSound;
    private bool legBreak;



    private void Awake()
    {
        m_runArrownImage.SetActive(m_running);
        AS = GetComponent<AudioSource>();
        inventory = GetComponent<Inventory>();
        characterController = GetComponent<CharacterController>();
        clampX = 0f;
        moveSpeed = walkSpeed;
        imageStand.enabled = true;
        imageCrouch.enabled = false;
        imageExitHidePlace.enabled = false;

    }

    private void Update()
    {

        canBeCatchen = characterController.isGrounded;
       
        if (!locked)
        {
            CameraRotation();
            HidePlaceExit();
            if (!lockedMovement)
            {
                Movement();
                Controll();
                Stamina();
            }
            
    
        }
    }

    private void Controll()
    {
        if(!characterController.isGrounded && characterController.velocity.y <= -7f && !legBreak)
        {
            legBreak = true;
          
        }else
        {
            if(characterController.isGrounded && legBreak)
            {
                legBreak = false;
                PlayerLegsBreak();
            }
        }


        float newHeight = crouch ? crouchHeight : normalHeight;
        characterController.height = Mathf.Lerp(characterController.height, newHeight, Time.deltaTime * lerpSpeed);

        characterController.center = Vector3.down * (normalHeight - characterController.height) / 2.0f;

        float newCamPos = crouch ? cameraCrouchOffset : cameraNormalOffset;
        Vector3 newPos = new Vector3(cameraTransform.localPosition.x, newCamPos, cameraTransform.localPosition.z);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newPos, Time.deltaTime *  lerpSpeed);

        if (characterController.isGrounded && CrossPlatformInputManager.GetButtonDown("Crouch"))
        {
            SetCrouch();
        }

        if (CrossPlatformInputManager.GetButtonDown("Run"))
        {
            SetRun();
        }

        if (CrossPlatformInputManager.GetButtonDown("Drop"))
        {
            GameManager.inventory.DropItem();
        }
    }

    private void PlayerLegsBreak()
    {
        GameManager.ScreenBlood(1);
        lockedMovement = true;
        crouch = true;
        moveSpeed = crouchSpeed;
        imageStand.enabled = false;
        imageCrouch.enabled = true;
        characterController.height = crouchHeight;
        cameraTransform.localPosition = new Vector3(0f, cameraCrouchOffset, 0f);
        AS.PlayOneShot(legBreakSound);
        StartCoroutine(WaitLegsFix());
    }

    public void Hide(int state)
    {
        if (state == 1)
        {
            if(GameManager.enemy.seePlayer)
            {
                GameManager.enemy.SendHidePlace();
            }
            StopAllCoroutines();
            imageExitHidePlace.enabled = true;
            lockedMovement = true;
            crouch = true;        
            characterController.height = crouchHeight;
            cameraTransform.localPosition = new Vector3(0f, cameraCrouchOffset, 0f);
        }else
        {
            imageExitHidePlace.enabled = false;
            lockedMovement = false;
            crouch = false;
            moveSpeed = walkSpeed;
            characterController.height = normalHeight;
            cameraTransform.localPosition = new Vector3(0f, cameraNormalOffset, 0f);
            imageStand.enabled = true;
            imageCrouch.enabled = false;
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
            cameraTransform.localPosition = new Vector3(0f, cameraNormalOffset, 0f);
            crouch = false;
            moveSpeed = walkSpeed;
            imageStand.enabled = true;
            imageCrouch.enabled = false;
        }

        if(state == 2)
        {
            cameraAnimation.Play(cameraHitAnimName);
            GameManager.ScreenFade(2);
        }

        if(state == 3)
        {
            cameraAnimation.Play(camHitName);
            GameManager.inventory.DropItem();
            locked = true;           
            crouch = false;
            moveSpeed = walkSpeed;
            imageStand.enabled = true;
            imageCrouch.enabled = false;
            GameManager.ScreenFade(3);
        }

        if(state == 4)
        {
            GameManager.ScreenBlood(0);
        }

       

    }

    private void Movement()
    {
        float inputX = CrossPlatformInputManager.GetAxis("Horizontal") * moveSpeed;
        float inputY = CrossPlatformInputManager.GetAxis("Vertical") * moveSpeed;

        Vector3 forvardMove = transform.forward * inputY;
        Vector3 sideMove = transform.right * inputX;
        characterController.SimpleMove(forvardMove + sideMove);


        if(characterController.velocity.magnitude > 0.5f)
        {
            playerMoving = true;
            cameraAnimation.Play(cameraMoveAnimName);
            cameraAnimation[cameraMoveAnimName].speed = moveSpeed / 4f;       
        }
        else
        {
            playerMoving = false;
            cameraAnimation.Play(cameraIdleAnimName);         
        }


    }
    public void SetRun()
    {

        if (!m_running && !m_staminaRecovery)
        {
            m_running = true;
            m_runArrownImage.SetActive(m_running);

            if (!crouch)
            {
                moveSpeed = runSpeed;
            }
        }
        else
        {
            m_running = false;
            m_runArrownImage.SetActive(m_running);

            if (!crouch)
            {
                moveSpeed = walkSpeed;
            }
            else
            {
                moveSpeed = crouchSpeed;
            }
        }
    }

    private void Stamina()
    {

        m_staminaBar.fillAmount = m_stamina / 100f;

        if (m_running && !crouch)
        {
            if (characterController.velocity.magnitude > 3.5f && !m_unlimitedStamina)
            {
                m_stamina -= m_staminaConsumptionSpeed * Time.deltaTime;
                if (m_stamina <= 0)
                {
                    m_running = false;
                    m_runArrownImage.SetActive(m_running);
                    m_staminaRecovery = true;
                    m_staminaBar.color = Color.red;
                    if (!crouch)
                    {
                        moveSpeed = walkSpeed;
                    }
                    else
                    {
                        moveSpeed = crouchSpeed;
                    }
                }
            }
            else
            {
                if (m_stamina < m_maxStamina)
                {
                    m_stamina += m_staminaRecoverySpeed * Time.deltaTime;
                    if (m_stamina >= m_maxStamina)
                    {
                        m_staminaRecovery = false;
                        m_staminaBar.color = Color.green;
                    }
                }
            }
        }
        else
        {
            if (m_stamina < m_maxStamina)
            {
                m_stamina += m_staminaRecoverySpeed * Time.deltaTime;
                if (m_stamina >= m_maxStamina)
                {
                    m_staminaBar.color = Color.green;
                    m_staminaRecovery = false;
                }
            }
        }
    }

    private void CameraRotation()
    {
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
            imageStand.enabled = false;
            imageCrouch.enabled = true;
        }
        else
        {


            if (CheckDistance() > normalHeight)
            {
                crouch = false;
                moveSpeed = walkSpeed;
                imageStand.enabled = true;
                imageCrouch.enabled = false;
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
        if(hidePlace)
        {
            if (CrossPlatformInputManager.GetButtonDown("Unhide"))
            {
                hidePlace.ExitHidePlace();
            }
        }
    }

    public void FootStepPlay()
    {
        int r = Random.Range(1, footSteps.Length);
        AS.volume = moveSpeed / 6;
        AS.PlayOneShot(footSteps[r]);
    }

    private IEnumerator WaitLegsFix()
    {
        yield return new WaitForSeconds(legsFixTime);
        lockedMovement = false;
        GameManager.ScreenBlood(0);

    }

    private bool isLerpingRotation = false;
    private Quaternion startRotation;
    private Quaternion targetRotation;
    private float lerpTimer = 0f;
    private float lerpDuration = 0f;

    public void LerpRotation(Transform lookAtTarget, float duration, bool isPlayerLocked)
    {
        if (isLerpingRotation) return;

        StartCoroutine(LerpRotationCoroutine(lookAtTarget, duration, isPlayerLocked));
    }

    private IEnumerator LerpRotationCoroutine(Transform lookAtTarget, float duration, bool isPlayerLocked)
    {
        isLerpingRotation = true;
        bool wasLocked = locked;
        locked = isPlayerLocked;

        // Store both body and camera rotations
        startRotation = transform.rotation;
        Quaternion startCamRotation = cameraTransform.rotation;

        // Calculate direction to target (include Y axis for vertical look)
        Vector3 directionToTarget = lookAtTarget.position - cameraTransform.position;

        // Create target rotation for both body (horizontal) and camera (full rotation)
        Quaternion targetBodyRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));
        targetRotation = Quaternion.LookRotation(directionToTarget);

        lerpTimer = 0f;
        lerpDuration = duration;

        while (lerpTimer < lerpDuration)
        {
            lerpTimer += Time.deltaTime;
            float t = lerpTimer / lerpDuration;

            t = t * t * (3f - 2f * t);

            // Rotate body for horizontal movement
            transform.rotation = Quaternion.Slerp(startRotation, targetBodyRotation, t);
            // Rotate camera for vertical movement
            cameraTransform.rotation = Quaternion.Slerp(startCamRotation, targetRotation, t);

            yield return null;
        }

        // Final rotations
        transform.rotation = targetBodyRotation;
        cameraTransform.rotation = targetRotation;

        if (!wasLocked)
        {
            locked = false;
        }

        isLerpingRotation = false;
    }

}



