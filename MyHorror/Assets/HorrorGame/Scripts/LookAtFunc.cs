using UnityEngine;

public class LookAtFunc : MonoBehaviour
{
    public Animator animator;
    public Animator CopAnimator;

    public bool IKActive = false;

    public Transform LookAtObj = null;

    public float LookWeight = 0f;

    public CamInteraction CamInteract;

    private void OnAnimatorIK(int layerIndex)
    {
        if (this.gameObject.GetComponent<Animator>())
        {
            if (IKActive)
            {

                if(LookAtObj != null)
                {
                    LookWeight = Mathf.Lerp(LookWeight, 1, Time.deltaTime * 2);

                }

            }

            else
            {
                LookWeight = Mathf.Lerp(LookWeight, 0, Time.deltaTime * 2);
            }

            if (CamInteract.TalkToNpcBool == true)
            {

            animator.SetLookAtPosition(LookAtObj.position);
            animator.SetLookAtWeight(LookWeight);

            }
            else if(CamInteract.TalkToCopBool == true)
            {
                CopAnimator.SetLookAtPosition(LookAtObj.position);
                CopAnimator.SetLookAtWeight(LookWeight);
            }

        }
    }

}
