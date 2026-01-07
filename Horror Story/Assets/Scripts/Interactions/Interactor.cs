using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;

public class Interactor : MonoBehaviour
{

    public TextMeshProUGUI InteractionText;

    [Header("Interact Settings")]
    [Tooltip("Distance of ray to interact")]
    public float rayDistance;
    [Tooltip("Layers to interact (default as obstacle)")]
    public LayerMask interactLayers;
    [Tooltip("Tags for interact")]
    public string interactTag;
    [Tooltip("Inventory script")]
    public Inventory inventory;
    [Header("UI Settings")]
    //[Tooltip("UI interactButton for mobile only")]
    //public Image interactButton;
    private FirstPersonController player;

    private void Awake()
    {
        player = inventory.gameObject.GetComponent<FirstPersonController>();
    }

    private void Update()
    {
        if (RayCastCheck() != null)
        {

            GameObject hitObj = RayCastCheck();


            InteractionText.text = "PREES E";

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (RayCastCheck().GetComponent<InteractCallEvent>())
                {
                    RayCastCheck().GetComponent<InteractCallEvent>().InteractCall();
                }
                else
                  if (RayCastCheck().GetComponent<Item>())
                {
                    AudioSource.PlayClipAtPoint(RayCastCheck().GetComponent<Item>().pickupSound, transform.position);
                    inventory.AddItem(RayCastCheck().GetComponent<Item>().itemID, RayCastCheck());

                }
            }

        }
        else
        {
            InteractionText.text = "";

        }

    }

    private GameObject RayCastCheck()
    {
        RaycastHit hit;
        Vector3 fwd = transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(transform.position, fwd, out hit, rayDistance, interactLayers))
        {
            if (hit.transform.gameObject.tag == interactTag)
            {
                return hit.transform.gameObject;
            }


        }

        return null;
    }

}
