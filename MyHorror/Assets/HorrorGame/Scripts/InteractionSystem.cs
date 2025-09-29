using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class InteractionSystem : MonoBehaviour
{

    public GameObject SphereTake;

    public GameObject SphereHeld;

    public GameObject SpherePlaced;

    //YellowBox

    public GameObject BoxTake;

    public GameObject BoxHeld;

    public GameObject BoxPlaced;

    //YellowBox

    //Cyllinder

    public GameObject CyllinderTake;

    public GameObject CyllinderHeld;

    public GameObject CyllinderPlaced;

    //Cyllinder

    bool CanDoJob = true;
   
    public Text InteractionText;

    //Inventroy

    bool HaveSphere = false; 
    bool HaveBox = false; 
    bool HaveCyllinder = false;

    //Inventroy

    public AudioSource source;

    public AudioClip TakeClip;
    public AudioClip PlaceClip;


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

                        source.PlayOneShot(TakeClip);

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

                        source.PlayOneShot(PlaceClip);


                    }
                }

                else if (hit.collider.CompareTag("YellowBox") && HaveBox == false)
                {
                    InteractionText.text = "Take Box";

                    if (Input.GetMouseButtonDown(0))
                    {

                        BoxTake.SetActive(false);

                        BoxHeld.SetActive(true);

                        HaveBox = true;

                        source.PlayOneShot(TakeClip);


                    }
                }
                else if (hit.collider.CompareTag("PlaceBox") && HaveBox == true)
                {
                    InteractionText.text = "Place Box";
                    
                    if (Input.GetMouseButtonDown(0))
                    {

                        BoxHeld.SetActive(false);

                        BoxPlaced.SetActive(true);

                        HaveBox = false;

                        source.PlayOneShot(PlaceClip);


                    }
                }
                else if (hit.collider.CompareTag("BlackCyllinder") && HaveCyllinder == false)
                {
                    InteractionText.text = "Take cyllinder";

                    if (Input.GetMouseButtonDown(0))
                    {

                        CyllinderTake.SetActive(false);

                        CyllinderHeld.SetActive(true);

                        HaveCyllinder = true;

                        source.PlayOneShot(TakeClip);


                    }
                }
                else if (hit.collider.CompareTag("PlaceCyllinder") && HaveCyllinder == true)
                {
                    InteractionText.text = "Place cyllinder";

                    if (Input.GetMouseButtonDown(0))
                    {

                        CyllinderHeld.SetActive(false);

                        CyllinderPlaced.SetActive(true);

                        HaveCyllinder = false;

                        source.PlayOneShot(PlaceClip);


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
