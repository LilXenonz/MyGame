using UnityEngine;

public class DonePizza : MonoBehaviour
{
    public Inventory Inventory;

    public Transform PlacePoint;

    public CustomerOrder Customer;


    public void PlacePizzaInOven()
    {

        Inventory.CurrentItem.transform.parent = null;
        Inventory.CurrentItem.transform.position = PlacePoint.position;
        Inventory.CurrentItem.transform.rotation = PlacePoint.rotation;


        Customer.TakePizza();
    }
}
