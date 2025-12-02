using UnityEngine;
using UnityEngine.UI;

public class FoodSystem : MonoBehaviour
{
    public GameObject Canvas;

    public GameObject[] CheesePlaced;

    public Toggle cheeseToggle;
    public Toggle pepperoniToggle;
    public Toggle breadtoggle;

    public Text CheeseText;
    public Text InteractionText;

    public Inventory inventory;

    public int cheeseItemID;
    public int PepperoniItemID;
    public int breadItemID;

    private int cheesePlaced = 0;

    private void Awake()
    {
        inventory = FindObjectOfType<Inventory>();
    }

    public void TakeCheese()
    {
        CloseCanvas();
    }

    public void Takepepperoni()
    {
        CloseCanvas();
    }

    public void TakeDough()
    {
        CloseCanvas();
    }

    public void PlaceFood()
    {
        if (inventory == null)
        {
            OpenCanvas();
            return;
        }

        if (inventory.CurrentItemID == PepperoniItemID)
        {
            breadtoggle.isOn = true;
            inventory.RemoveItem();
            return;
        }

        if (inventory.CurrentItemID == breadItemID)
        {
            pepperoniToggle.isOn = true;
            inventory.RemoveItem();
            return;
        }

        if (inventory.CurrentItemID == cheeseItemID)
        {
            if (CheesePlaced == null || CheesePlaced.Length == 0)
            {
                inventory.RemoveItem(); 
                return;
            }

            if (cheesePlaced < CheesePlaced.Length)
            {
                GameObject next = CheesePlaced[cheesePlaced];
                 next.SetActive(true);


                cheesePlaced++;

                if (CheeseText != null)
                    CheeseText.text = $"Cheese ({cheesePlaced}/{CheesePlaced.Length})";
            }

            inventory.RemoveItem();

            if (cheesePlaced >= CheesePlaced.Length)
            {
                if (cheeseToggle != null) cheeseToggle.isOn = true;
            }

            OpenCanvas();
            return;
        }

        OpenCanvas();
    }

    private void CloseCanvas()
    {
        if (Canvas != null && Canvas.activeSelf) Canvas.SetActive(false);
    }

    private void OpenCanvas()
    {
        if (Canvas != null && !Canvas.activeSelf) Canvas.SetActive(true);
    }
}
