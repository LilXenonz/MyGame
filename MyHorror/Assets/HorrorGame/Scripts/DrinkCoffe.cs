using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DrinkCoffe : MonoBehaviour
{


    public TextMeshProUGUI DrinkText;

    public Animator CoffeAnimator;

    public AudioSource Source;

    public AudioClip CoffeSound;

    private bool CanDrink = true;

    private int DrinkAmount = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        DrinkText.text = "press (space) to drink coffe";

    }

    // Update is called once per frame
    void Update()
    {
        if(CanDrink)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {

                StartCoroutine(DrinkCoffeCO());

            }
        }
       
    }

    IEnumerator DrinkCoffeCO()
    {
        CanDrink = false;

        DrinkText.text = "";

        if (DrinkAmount == 0)
        {

            //drink first time
            CoffeAnimator.SetInteger("C", 1);
        }
        else if (DrinkAmount == 1)
        {
            //drink second time
            CoffeAnimator.SetInteger("C", 2);

        }

        DrinkAmount++; 

        Source.PlayOneShot(CoffeSound);

        yield return new WaitForSeconds(1.5f);

        if(DrinkAmount == 1)
        {
            DrinkText.text = "press (space) to drink coffe";
            CanDrink = true;
        }
        else if (DrinkAmount == 2)
        {
            DrinkText.text = "";
            CanDrink = false;
        }
    }

}
