using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class CamInteraction : MonoBehaviour
{
    public Text InteractionText;

    private float InteractionDis = 5f;

    public bool CanInteraction = true;


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



        yield return new WaitForSeconds(2f);
    }
}


