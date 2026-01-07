using System.Collections.Generic;
using UnityEngine;

public class FurnitureController : MonoBehaviour
{
    //public GameObject mainObject, placingObject;
    public Collider col;

    public float price;

    public Transform standPoing;

    public List<ShelfSpaceController> shelves;

    public static FurnitureController instance;

    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        /*if(shelves.Count>0)
        {
            StoreController.instance.shelvingCases.Add(this);
        }*/

    }

    public bool AreAllShelvesFull()
    {
        if (shelves == null || shelves.Count == 0)
            return false;

        foreach (ShelfSpaceController shelf in shelves)
        {
            if (shelf == null)
                return false;

            if (!shelf.IsShelfFull())
                return false;
        }

        return true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
