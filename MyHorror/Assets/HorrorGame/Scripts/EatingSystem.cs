using System.Collections;
using UnityEngine;

public class EatingSystem : MonoBehaviour
{
    [SerializeField] private AudioSource Source;
    [SerializeField] private AudioClip EatSound;

    public Inventory inventory;
    private void Awake()
    {
        //inventory = FindObjectOfType<Inventory>();
    }

    public void EatPizzaVO()
    {
        StartCoroutine(EatPizza());
    }

    IEnumerator EatPizza()
    {

        Source.PlayOneShot(EatSound);


        yield return new WaitForSeconds(1f);

        inventory.RemoveItem();
    }
}
