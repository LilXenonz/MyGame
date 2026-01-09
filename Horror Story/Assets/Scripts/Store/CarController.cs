using DialogueEditor;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DialogueEditor;
using Unity.Cinemachine;

public class CarController : MonoBehaviour
{
    public LookAtFunc lookat;

    public InputActionReference lookAction;
    private float HoriRot, VertRot;
    public float LookSpeed;
    public Camera Camera;
    public float minLookAngle, maxLookAngle;

    public float InteractionRange;

    public LayerMask DriveLayer;

    public NPCConversation conversation;

    public bool lockPlayer = false;

    //public CinemachineCamera playerCam;
    public CinemachineCamera NPCCam;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if(lockPlayer == false)
        {
            Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

            HoriRot += lookInput.x * Time.deltaTime * LookSpeed;
            transform.rotation = Quaternion.Euler(0f, HoriRot, 0f);

            VertRot -= lookInput.y * Time.deltaTime * LookSpeed;
            VertRot = Mathf.Clamp(VertRot, minLookAngle, maxLookAngle);
            Camera.transform.localRotation = Quaternion.Euler(VertRot, 0f, 0f);
        }
        


        Ray ray = Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        RaycastHit hit;
        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Physics.Raycast(ray, out hit, InteractionRange, DriveLayer))
            {
                //CustomerDialogueManager.instance.StartDialogue(0, lookat, onFinishedDialogue);
                ConversationManager.Instance.StartConversation(conversation);
            }

        }     

    }

    private void onDialogue()
    {
        NPCCam.Priority = 5;
        lockPlayer = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        lookat.IKActive = true;
    }

    private void offDialogue()
    {
        NPCCam.Priority = 0;
        lockPlayer = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        lookat.IKActive = false;
    }

    private void OnEnable()
    {
        ConversationManager.OnConversationStarted += ConversationStart;
        ConversationManager.OnConversationEnded += ConversationEnd;
    }

    private void OnDisable()
    {
        ConversationManager.OnConversationStarted -= ConversationStart;
        ConversationManager.OnConversationEnded -= ConversationEnd;
    }

    private void ConversationStart()
    {
        onDialogue();
    }

    private void ConversationEnd()
    {
        offDialogue();
    }

}
