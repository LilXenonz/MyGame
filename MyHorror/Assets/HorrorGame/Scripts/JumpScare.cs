using UnityEngine;

public class JumpScare : MonoBehaviour
{

    bool CanInteract = true;

    public Animator DoorAnimator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (CanInteract == true)
        {
            Ray ray1 = new Ray(transform.position, transform.forward);
            RaycastHit hit1;

            if (Physics.Raycast(ray1, out hit1, 10f))
            { 
            
                if(hit1.collider.CompareTag("Door"))
                {
                    if (Input.GetKeyDown(KeyCode.E))
                    { 
                    
                        CanInteract = false;

                        DoorAnimator.SetInteger("C", 1);
        
                    
                    }
            
                }
            }
        }
    }



}
