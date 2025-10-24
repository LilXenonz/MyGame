using UnityEngine;

public class SittingController : MonoBehaviour
{

    public bool CanWalk = true;
    [SerializeField] private SitDownManager ManagerScript;


    // Update is called once per frame
    void Update()
    {
        if (CanWalk )
        {

            if(Input.GetKeyDown(KeyCode.Space))
            {
                ManagerScript.GetUp();
            }

        }
    }
}
