using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class NoteInteract : MonoBehaviour
{
    public GameObject LookAt;
    private bool CanDrink = true;

    // Update is called once per frame
    void Update()
    {
        if (CanDrink)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {

                LookJump();

            }
        }

    }

    void LookJump()
    {
        LookAt.SetActive(true);
    }

}
