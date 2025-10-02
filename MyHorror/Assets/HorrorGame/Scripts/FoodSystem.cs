using Unity.VisualScripting;
using UnityEngine;

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
                    if (Canvas.activeSelf == false)
                    {
                        Canvas.SetActive(true);

                    }

                }
                else if (hit.collider.CompareTag("Eggs"))
                {
                    if (Canvas.activeSelf == true)
                    {
                        Canvas.SetActive(false);
                        
                    }
                    if (Input.GetMouseButtonDown(0))
                    {
                        //take eggs
                        TakeEggs();
                    }


                }
                if (hit.collider.CompareTag("Pepper"))
                {
                    if (Canvas.activeSelf == true)
                    {
                        Canvas.SetActive(false);

                       
                    }
                    if (Input.GetMouseButtonDown(0))
                    {
                        //take eggs
                        TakePepper();
                    }
                }
                if (hit.collider.CompareTag("Salt"))
                {
                    if (Canvas.activeSelf == true)
                    {
                        Canvas.SetActive(false);
                        
                        
                    }
                    if (Input.GetMouseButtonDown(0))
                    {
                        //take eggs
                        TakeSalt();
                    }
                }
            }

            else
            {
                if (Canvas.activeSelf == true)
                {
                    Canvas.SetActive(false);

                }
            }
        }

    }

    private void TakeEggs()
    {
        Debug.Log("hej");
 
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
        Inventory.HaveSalt = true;
        Salt.SetActive(false);
    }
    private void TakePepper()
    {
        Inventory.HavePepper = true;
        Pepper.SetActive(false);
    }
    private void PlaceEggs()
    {


    }

   
}
