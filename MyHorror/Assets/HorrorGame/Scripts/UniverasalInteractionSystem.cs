using UnityEngine;
using UnityEngine.UI;

public class UniverasalInteractionSystem : MonoBehaviour
{

    bool CanInteract = true;

    public Transform HeldPoint;

    private Transform PlacePoint;

    private GameObject HeldObject;

    public Text Interactiontext;

    public AudioSource Source;

    public AudioClip TakeClip;

    public AudioClip PlaceClip;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if(CanInteract == true)
        {
            if(HeldObject == null)
            {
                Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                RaycastHit hit;

                if(Physics.Raycast(ray, out hit, 5f))
                {
                    if(hit.collider.CompareTag("Pickable"))
                    {

                        Interactiontext.text = "Take";

                        if(Input.GetMouseButton(0))
                        {

                            Source.PlayOneShot(TakeClip);

                            Interactiontext.text = "";

                            //pick up

                            HeldObject = hit.collider.gameObject;

                            HeldObject.transform.SetParent(HeldPoint);

                            HeldObject.transform.localPosition = Vector3.zero;

                            HeldObject.transform.localRotation = Quaternion.identity;


                        }

                    }
                    else
                    {
                        Interactiontext.text = "";
                    }
                }
                else
                {
                    Interactiontext.text = "";
                }


            }
            else
            {

                Ray ray2 = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                RaycastHit hit2;

                if (Physics.Raycast(ray2, out hit2, 5f))
                {
                    if (hit2.collider.CompareTag("PlaceOn"))
                    {

                        Interactiontext.text = "Place";


                        if (Input.GetMouseButton(0))
                        {

                            Source.PlayOneShot(PlaceClip);


                            Interactiontext.text = "";


                            //place

                            PlacePoint = hit2.collider.transform;

                            HeldObject.transform.SetParent(PlacePoint);
                            HeldObject.transform.localPosition = new Vector3(0, 0.64f, 0);
                            HeldObject.transform.localRotation = Quaternion.identity;

                            HeldObject = null;
                            PlacePoint = null;

                        }

                    }
                    else
                    {
                        Interactiontext.text = "";

                    }


                }
                else
                {
                    Interactiontext.text = "";
                }


            }



        }
        
    }
}
