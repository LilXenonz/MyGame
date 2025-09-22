using UnityEngine;

public class LookAtFunc : MonoBehaviour
{
    public Animator animator;

    public bool IKActive = false;

    public Transform LookAtObj = null;

    public float LookWeight = 2f;


    private void OnAnimatorIK(int layerIndex)
    {
        if (animator)
        {
            if (IKActive)
            {

                if(LookAtObj != null)
                {
                    animator.SetLookAtWeight(LookWeight);
                    animator.SetLookAtPosition(LookAtObj.position);
                }

            }

            else
            {
                animator.SetLookAtWeight(0);
            }
        }
    }

}
