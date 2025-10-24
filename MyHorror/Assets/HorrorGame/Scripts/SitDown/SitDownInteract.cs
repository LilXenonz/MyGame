using UnityEngine;

public class SitDownInteract : MonoBehaviour
{

    
    public bool CanInteract = true;
    [SerializeField] private SitDownManager GameManagerScript;


    void Update()
    {

        if (CanInteract == true)
        {

            if (Input.GetMouseButton(0))
            {
                Ray ray = new Ray(transform.position, transform.forward);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 5f))
                {
                    if (hit.collider.CompareTag("Chair"))
                    {

                        //sit on chair
                        GameManagerScript.SitDown();

                    }

                }

            }

        }
    }
}
