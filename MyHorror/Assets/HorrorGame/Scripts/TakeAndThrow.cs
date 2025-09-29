using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;

public class TakeAndThrow : MonoBehaviour
{

    bool CanInteract = true;

    public Transform HeldPoint;

    [SerializeField] float ThrowForce = 10f;

    private GameObject HeldObject;
    private Rigidbody HeldRB;   

    public Text InteractionText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (CanInteract == true)
        {
            if (HeldObject == null)
            {
                Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 5f))
                {

                    if (hit.collider.CompareTag("Throwable"))
                    {

                        InteractionText.text = "Take";

                        if (Input.GetMouseButtonDown(0))
                        {

                            InteractionText.text = "";


                            //pick up

                            HeldObject = hit.collider.gameObject;
                            HeldRB = HeldObject.GetComponent<Rigidbody>();

                            HeldObject.transform.SetParent(HeldPoint);
                            HeldObject.transform.localPosition = Vector3.zero;
                            HeldObject.transform.localRotation = Quaternion.identity;

                            HeldRB.useGravity = false;
                            HeldRB.isKinematic = true;

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
            else
            {

                if(Input.GetMouseButtonDown(1))
                {
                    //drop

                    HeldObject.transform.SetParent(null);
                    HeldRB.useGravity = true;
                    HeldRB.isKinematic = false;

                    HeldObject = null;
                    HeldRB = null;

                }

                if (Input.GetMouseButtonDown(0))
                {
                    //Throw

                    HeldObject.transform.SetParent(null);

                    HeldRB.useGravity = true;
                    HeldRB.isKinematic = false;

                    HeldRB.AddForce(Camera.main.transform.forward * ThrowForce, ForceMode.Impulse);

                    HeldObject = null;
                    HeldRB = null;

                }

            }
        }

    }
}
