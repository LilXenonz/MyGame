using System.Collections;
using TMPro;
using UnityEngine;

public class FireSystem : MonoBehaviour
{

    public GameObject[] fireWood;

    public GameObject[] PlacedWoods;

    public TextMeshProUGUI Subtext;

    public GameObject TalkPanel;

    private bool CanInteract = true;

    private bool HaveWood = false;

    private int Wood = 0;
    private int PlaceWoodNumber = 0;

    private bool CanStartFire = false;

    public GameObject fire;

    // Update is called once per frame
    void Update()
    {
     
        if (CanInteract == true)
        {

            Ray ray = new Ray(transform.position, transform.forward);

            RaycastHit hit;

            if(Physics.Raycast(ray, out hit, 10f))
            {

                if(hit.collider.CompareTag("Wood"))
                {

                    if(Input.GetMouseButtonDown(0))
                    {
                        
                        if (HaveWood == true)
                        {

                            StartCoroutine(HandFull());


                        }
                        else if (HaveWood == false)
                        {
                            TakeWood();
                        }
                                                

                    }
                }

                else if (hit.collider.CompareTag("CampFire"))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (CanStartFire == false)
                        {

                            if (HaveWood == true)
                            {
                                PlaceWood();
                            }

                            else if(HaveWood == false)
                            {

                                StartCoroutine(CantTakeWood());

                            }

                        }
                        else if (CanStartFire == true)
                        {
                            fire.SetActive(true);
                            CanInteract = false;
                        }
                    }
                }
            }

        }


    }

    void TakeWood()
    {

        HaveWood = true;

        if(Wood == 0)
        {
            fireWood[0].SetActive(false);
            Wood = 1;
        }

        else if(Wood == 1)
        {
            fireWood[1].SetActive(false);
            Wood = 2;

        }

        else if(Wood == 2)
        {
            fireWood[2].SetActive(false);
            Wood = 3;

        }
    }
    void PlaceWood()

    {
        HaveWood = false;


        if (PlaceWoodNumber == 0)
        {
            PlacedWoods[0].SetActive(true);
            PlaceWoodNumber = 1;
        }

        else if(PlaceWoodNumber == 1)
        {
            PlacedWoods[1].SetActive(true);
            PlaceWoodNumber = 2;

        }

        else if(PlaceWoodNumber == 2)
        {
            PlacedWoods[2].SetActive(true);
            PlaceWoodNumber = 3;

            CanStartFire = true;

        }
    }

    IEnumerator CantTakeWood()
    {
        CanInteract = false;

        TalkPanel.SetActive(true);

        Subtext.text = "i dont have wood";

        yield return new WaitForSeconds(1.5f);

        TalkPanel.SetActive(false);
        Subtext.text = "";

        CanInteract = true;

    }

    IEnumerator HandFull()
    {
        CanInteract = false;

        TalkPanel.SetActive(true);

        Subtext.text = "my hand is full";

        yield return new WaitForSeconds(1.5f);

        TalkPanel.SetActive(false);
        Subtext.text = "";

        CanInteract = true;
    }


}
