using System.Collections;
using UnityEngine;

public class SitDownManager : MonoBehaviour
{

    [SerializeField] private GameObject FPSController;
    [SerializeField] private GameObject SitDownCam;
    [SerializeField] private GameObject BlackTransition;
    [SerializeField] private SitDownInteract InteractScript;
    [SerializeField] private SittingController SitScript;

    private void Start()
    {
        FPSController.SetActive(true);
        BlackTransition.SetActive(false);
        SitDownCam.SetActive(false);
    }

    public void SitDown()
    {

        StartCoroutine(SitDownCO());

    }


    IEnumerator SitDownCO()
    {

        InteractScript.CanInteract = false;
        BlackTransition.SetActive(true);
        yield return new WaitForSeconds(1f);
        SitDownCam.SetActive(true);
        FPSController.SetActive(false);
        yield return new WaitForSeconds(1.5f);

        BlackTransition.SetActive(false);
        InteractScript.CanInteract = true;

    }

    public void GetUp()
    {
        StartCoroutine (GetUpCO());
    }

    IEnumerator GetUpCO()
    {
        SitScript.CanWalk = false;
        BlackTransition.SetActive(true);
        yield return new WaitForSeconds(1f);
        FPSController.SetActive(true);
        SitDownCam.SetActive(false);
        yield return new WaitForSeconds(1.5f);

        BlackTransition.SetActive(false);
        SitScript.CanWalk = true;

    }

}
