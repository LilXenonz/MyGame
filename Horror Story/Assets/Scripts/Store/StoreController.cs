using System.Collections.Generic;
using UnityEngine;

public class StoreController : MonoBehaviour
{
    public static StoreController instance;

    public List<FurnitureController> shelvingCases = new List<FurnitureController>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
