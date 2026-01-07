using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    public LookAtFunc lookat;

    public InputActionReference lookAction;
    private float HoriRot, VertRot;
    public float LookSpeed;
    public Camera Camera;
    public float minLookAngle, maxLookAngle;

    public float InteractionRange;

    public ConversationData conversation;

    public LayerMask DriveLayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        HoriRot += lookInput.x * Time.deltaTime * LookSpeed;
        transform.rotation = Quaternion.Euler(0f, HoriRot, 0f);

        VertRot -= lookInput.y * Time.deltaTime * LookSpeed;
        VertRot = Mathf.Clamp(VertRot, minLookAngle, maxLookAngle);
        Camera.transform.localRotation = Quaternion.Euler(VertRot, 0f, 0f);


        Ray ray = Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        RaycastHit hit;
        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Physics.Raycast(ray, out hit, InteractionRange, DriveLayer))
            {
                CustomerDialogueManager.instance.StartDialogue(0, lookat, onFinishedDialogue);
            }

        }     

    }

    private void onFinishedDialogue()
    {
        Debug.Log("wsp");
        
    }
}
