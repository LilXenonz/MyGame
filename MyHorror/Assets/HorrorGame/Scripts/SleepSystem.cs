using UnityEngine;
using TMPro;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.SceneManagement;

public class SleepSystem : MonoBehaviour
{

    [Header("Interaction")]
    public bool CanInteract = true;

    [SerializeField] private GameObject Panel;
    [SerializeField] private FirstPersonController FPSCharacter;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI SubText;
    private string Holder;
    private float WirteSpeed = 0.25f;


    // Update is called once per frame
    void Update()
    {
        if (CanInteract == true)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 5f))
            {
                if(hit.collider.CompareTag("Bed"))
                {
                    if(Input.GetKeyDown(KeyCode.E))
                    {

                        StartCoroutine(Sleep());

                    }
                }
            }
        }
    }

    IEnumerator Sleep()
    {

        Panel.SetActive(true);
        CanInteract = false;
        FPSCharacter.enabled = false;

        yield return new WaitForSeconds(1f);

        SubText.text = "";
        Holder = "night 21";
        foreach (char c in Holder)
        {
            SubText.text += c;
            yield return new WaitForSeconds(WirteSpeed);
        }

        yield return new WaitForSeconds(1f);

        //load next scene

        //SceneManager.LoadScene("Level02");

        Debug.Log("Neext scene loaded");

    }
}
