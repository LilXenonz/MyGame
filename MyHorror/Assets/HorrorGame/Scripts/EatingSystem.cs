using System.Collections;
using UnityEngine;

public class EatingSystem : MonoBehaviour
{
    private bool CanInteract = true;
    private int SliceAte = 0;
    [SerializeField] private GameObject[] PizzaSlices;
    [SerializeField] private AudioSource Source;
    [SerializeField] private AudioClip EatSound;


    // Update is called once per frame
    void Update()
    {
        
        if (CanInteract == true)
        {

            if(Input.GetMouseButtonDown(0))
            {

                Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                RaycastHit hit;

                if(Physics.Raycast(ray, out hit, 5f))
                {
                    if (hit.collider.CompareTag("Pizza"))
                    {
                        if (SliceAte >= 4) return;
                        

                        StartCoroutine(EatPizza());

                    }
                }

            }

        }

    }

    IEnumerator EatPizza()
    {

        CanInteract = false;

        Source.PlayOneShot(EatSound);

        if (SliceAte == 0)
        {
            SliceAte = 1;
            PizzaSlices[0].SetActive(false);

        }
        else if (SliceAte == 1)
        {
            SliceAte = 2;
            PizzaSlices[1].SetActive(false);

        }
        else if (SliceAte == 2)
        {
            SliceAte = 3;
            PizzaSlices[2].SetActive(false);

        }
        else if (SliceAte == 3)
        {
            SliceAte = 4;
            PizzaSlices[3].SetActive(false);

        }


        yield return new WaitForSeconds(1f);

        CanInteract = true;
    }
}
