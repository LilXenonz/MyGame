using UnityEngine;

public class Door : MonoBehaviour
{
    public bool isDoorOpen = false;

    public Animator DoorAnimator;

    public void OpenDoor()
    {
        if (isDoorOpen == false)
        {
            isDoorOpen = true;
            DoorAnimator.SetTrigger("Open");
            DoorAnimator.SetBool("isOpen", true);
        }
        else if (isDoorOpen == true) 
        {

            DoorAnimator.SetBool("isOpen", false);
            isDoorOpen = false;

        }
    }
}
