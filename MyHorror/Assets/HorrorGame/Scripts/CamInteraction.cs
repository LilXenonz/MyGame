using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using UnityStandardAssets.Characters.FirstPerson;

public class CamInteraction : MonoBehaviour
{
    public Text InteractionText;

    private float InteractionDis = 5f;

    public bool CanInteraction = true;


    //look at

    public CinemachineCamera PlayerCam;
    public CinemachineCamera TalkNpcCam;

    public FirstPersonController FpsController;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (CanInteraction == true)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, InteractionDis))
            {
                if (hit.collider.CompareTag("NPC"))
                {
                    InteractionText.text = "Talk To Him";

                    //talk

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        CanInteraction = false;
                        StartCoroutine(TalkToNPC());
                    }
                }
                else
                {
                    InteractionText.text = "";

                }
            }

            else
            {
                InteractionText.text = "";
            }
        }

    }

    IEnumerator TalkToNPC()
    {

        InteractionText.text = "";
        FpsController.enabled = false;
        TalkNpcCam.enabled = true;
        PlayerCam.enabled = false;

        yield return new WaitForSeconds(2f);

        FpsController.enabled = true;
        PlayerCam.enabled = true;
        TalkNpcCam.enabled = false;

        CanInteraction = true;
    }
}


