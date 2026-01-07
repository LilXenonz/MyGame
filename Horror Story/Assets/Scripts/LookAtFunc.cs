using UnityEngine;

public class LookAtFunc : MonoBehaviour
{
    public Animator animator;

    public bool IKActive = false;

    public Transform LookAtObj;

    public float LookWeight = 0f;


    private void OnAnimatorIK(int layerIndex)
    {
        if (this.gameObject.GetComponent<Animator>())
        {
            if (IKActive)
            {

                if (LookAtObj != null)
                {
                    LookWeight = Mathf.Lerp(LookWeight, 1, Time.deltaTime * 2);

                }

            }

            else
            {
                LookWeight = Mathf.Lerp(LookWeight, 0, Time.deltaTime * 2);
            }

            animator.SetLookAtPosition(LookAtObj.position);
            animator.SetLookAtWeight(LookWeight);

        }
    }

}
