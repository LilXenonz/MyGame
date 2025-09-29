using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class InteractionSystem : MonoBehaviour
{

    public GameObject SphereTake;

    public GameObject SphereHeld;

    public GameObject SpherePlaced;

    bool CanDoJob = true;
   
    public Text InteractionText;

    //Inventroy

    bool HaveSphere = false; 

    //Inventroy

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        


    }

    // Update is called once per frame
    void Update()
    {

        if (CanDoJob == true) 
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 5f))
            {

                if (hit.collider.CompareTag("BlueSphere") && HaveSphere == false)
                {
                    
                    InteractionText.text = "Take Sphere";

                    if (Input.GetMouseButtonDown(0))
                    {

                        SphereTake.SetActive(false);

                        SphereHeld.SetActive(true);

                        HaveSphere = true;

                    }

                }
                else if (hit.collider.CompareTag("PlaceSphere") && HaveSphere == true)
                {
                    InteractionText.text = "Place Sphere";

                    if (Input.GetMouseButtonDown(0))
                    {

                        SphereHeld.SetActive(false);

                        SpherePlaced.SetActive(true);

                        HaveSphere = false;

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
}
