using UnityEngine;

public class Trigger : MonoBehaviour
{

    public TakeAndThrow TakeScript;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Throwable"))
        {
            if(TakeScript.HandIsfull == false)
            {
               
                Debug.Log("cube entered");

            }
        }
    }
}
