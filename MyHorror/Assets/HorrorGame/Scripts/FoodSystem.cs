using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FoodSystem : MonoBehaviour
{

    public GameObject Canvas;

    public GameObject Pepper;
    public GameObject Salt;

    public GameObject[] Eggs;

    public GameObject[] BrokenEggs;

    private bool CanInteract = true;

    public FoodInventory Inventory;

    public GameObject EggPack;

    public Toggle EggsToggle;
    public Toggle SaltToggle;
    public Toggle PepperToggle;

    public Text EggsText;

    private bool HandFull = false;

    public Text InteractionText;


    // Update is called once per frame
    void Update()
    {

        if (CanInteract == true)
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 5f))
            {
                if (hit.collider.CompareTag("Pan"))
                {

                    //InteractionText.text = "place food";


                    if (Canvas.activeSelf == false)
                    {
                        Canvas.SetActive(true);

                    }
                    if (Input.GetMouseButtonDown(0) && HandFull == true)
                    {
                        //take eggs
                        PlaceEggs();
                    }
                }
                else if (hit.collider.CompareTag("Eggs"))
                {
                    InteractionText.text = "take eggs";


                    if (Canvas.activeSelf == true)
                    {
                        Canvas.SetActive(false);
                        
                    }
                    if (Input.GetMouseButtonDown(0) && HandFull == false)
                    {
                        //take eggs
                        TakeEggs();
                    }


                }
                else if (hit.collider.CompareTag("Pepper"))
                {

                    InteractionText.text = "take pepper";

                    if (Canvas.activeSelf == true)
                    {
                        Canvas.SetActive(false);

                       
                    }
                    if (Input.GetMouseButtonDown(0) && HandFull == false)
                    {
                        //take eggs
                        TakePepper();
                    }

                }

                else if(hit.collider.CompareTag("Salt"))
                {
                    InteractionText.text = "take salt";


                    if (Canvas.activeSelf == true)
                    {
                        Canvas.SetActive(false);
                        
                        
                    }
                    if (Input.GetMouseButtonDown(0) && HandFull == false)
                    {
                        //take eggs
                        TakeSalt();
                    }
                }

                else
                {
                    InteractionText.text = "";

                }
            }

            else
            {
                InteractionText.text = "";


                if (Canvas.activeSelf == true)
                {
                    Canvas.SetActive(false);

                }

            }
        }

    }

    private void TakeEggs()
    {
        HandFull = true;

        if(Inventory.NumberEggs==0)
        {
            Inventory.NumberEggs = 1;
            Eggs[0].SetActive(false);
        }
        else if (Inventory.NumberEggs == 1 )
        {
            Inventory.NumberEggs = 2;
            Eggs[1].SetActive(false);
            EggPack.SetActive(false);
        }

    }

    private void TakeSalt()
    {
        HandFull = true;

        Inventory.HaveSalt = true;
        Salt.SetActive(false);
    }
    private void TakePepper()
    {
        HandFull = true;

        Inventory.HavePepper = true;
        Pepper.SetActive(false);
    }
    private void PlaceEggs()
    {
        HandFull = false;

        if(Inventory.HavePepper == true)
        {
            Inventory.HavePepper = false;

            PepperToggle.isOn = true;
        }

        if(Inventory.HaveSalt == true)
        {
            Inventory.HaveSalt = false;

            SaltToggle.isOn = true;
        }

        if(Inventory.NumberEggs == 1)
        {
            BrokenEggs[0].SetActive(true);
            EggsText.text = "Eggs (1/2)";
        }

        if(Inventory.NumberEggs == 2)
        {
            BrokenEggs[1].SetActive(true);
            EggsText.text = "Eggs (2/2)";
            EggsToggle.isOn = true;
        }
    }


}
