using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CarInteraction : MonoBehaviour
{

    public Text SubText;
    private string holder;
    private float texttypertime = 0.025f;

    private bool CanInteract = true;

    //dialogue

    public GameObject TalkPanel;
    public Text InteractionText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if(CanInteract == true)
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            RaycastHit hit;

            if(Physics.Raycast(ray, out hit, 5f))
            {
                if (hit.collider.CompareTag("Friend"))
                {
                    InteractionText.text = "Talk To Ahmad";

                    if(Input.GetKeyDown(KeyCode.E))
                    {
                        StartCoroutine(TalkToAhmad());
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

    IEnumerator TalkToAhmad()
    {

        CanInteract = false;

        InteractionText.text = "";

        TalkPanel.SetActive(true);

        SubText.text = "Me: ";
        holder = "Are you okey ?";

        foreach (char c in holder)
        {
            SubText.text += c;
            yield return new WaitForSeconds(texttypertime);
        }

        yield return new WaitForSeconds(1.5f);

        SubText.text = "Ahmed: ";
        holder = "Yeah im fine";

        foreach (char c in holder)
        {
            SubText.text += c;
            yield return new WaitForSeconds(texttypertime);
        }

        yield return new WaitForSeconds(1.5f);

        SubText.text = "";
        CanInteract = true;
        TalkPanel.SetActive(false);

    }
}
