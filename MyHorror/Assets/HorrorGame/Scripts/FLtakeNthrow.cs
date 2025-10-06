using UnityEngine;

public class FLtakeNthrow : MonoBehaviour
{

    private bool CanInteract = true;

    [SerializeField] private Transform HoldPoint;
    [SerializeField] float force = 10f;

    private GameObject HeldObject;
    private Rigidbody HeldRB;

    [SerializeField] private CamHorizontal MovecamHorizontal;  
    [SerializeField] private FlashLightSystem FlashlightSystem;  


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MovecamHorizontal.enabled = false;
        FlashlightSystem.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {


        if (CanInteract )
        {
            if (HeldObject == null)
            {
                if (Input.GetMouseButton(0))
                {
                    Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, 5f))
                    {
                        if(hit.collider.CompareTag("FlashLight"))
                        {
                            //pick
                            HeldObject = hit.collider.gameObject;
                            HeldRB = HeldObject.GetComponent<Rigidbody>();

                            HeldObject.transform.SetParent(HoldPoint);
                            HeldObject.transform.localPosition = Vector3.zero;
                            HeldObject.transform.localRotation = Quaternion.identity;

                            HeldRB.useGravity = false;
                            HeldRB.isKinematic = true;

                            FlashlightSystem.enabled = true;
                            MovecamHorizontal.enabled = true;

                        }
                    }
                }
            }
            else
            {

                if (Input.GetKeyDown(KeyCode.G)) 
                {

                    //throw

                    HeldObject.transform.SetParent(null);

                    HeldRB.useGravity = true;

                    HeldRB.isKinematic = false;

                    HeldRB.AddForce(Camera.main.transform.forward * force, ForceMode.Impulse);

                    HeldObject = null;
                    HeldRB = null;

                    FlashlightSystem.enabled = false;
                    MovecamHorizontal.enabled = false;


                }


            }
        }
    }
}
