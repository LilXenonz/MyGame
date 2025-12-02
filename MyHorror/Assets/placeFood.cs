using UnityEngine;
using UnityEngine.UI;

public class PlaceFood : MonoBehaviour
{
    public GameObject Canvas;

    public GameObject[] Placed;
    public GameObject[] Placed2;
    public GameObject[] Placed3;
    public GameObject[] Placed4;
    public GameObject[] Placed5;
    public GameObject[] Placed6;

    public Toggle Toggle1;
    public Toggle Toggle2;
    public Toggle Toggle3;

    public Text Text1;
    public Text Text2;
    public Text Text3;

    public Text InteractionText;

    public Inventory inventory;

    public int CheeseItemID;
    public int PinnapleeItemID;
    public int PeperoniItemID;
    public int OliveItemID;
    public int HamItemID;
    public int PepperItemID;

    private int cheesePlaced = 0;
    private int PinnapleePlaced = 0;
    private int PeperoniPlaced = 0;
    private int OlivePlaced = 0;
    private int PepperPlaced = 0;


    public void TakeItem()
    {
        CloseCanvas();
    }

    public void PlaceFoodVO()
    {
        if (inventory.CurrentItemID == CheeseItemID)
        {
            if (cheesePlaced == 0)
            {
                cheesePlaced = 1;
                Text1.text = "Eggs(1/2)";

                inventory.RemoveItem();


            }

            if (cheesePlaced == 1)
            {

                cheesePlaced = 2;
                Text2.text = "Eggs(2/2)";

                inventory.RemoveItem();

                return;

            }
        }
        
        if (inventory.CurrentItemID == PinnapleeItemID)
        {
            if (PinnapleePlaced == 0)
            {
                PinnapleePlaced = 1;
                Text1.text = "Pinnaplee(1/2)";

                inventory.RemoveItem();


            }

            if (PinnapleePlaced == 1)
            {

                PinnapleePlaced = 2;
                Text2.text = "Pinnaplee(2/2)";

                inventory.RemoveItem();

                return;

            }
        }

        if (inventory.CurrentItemID == PeperoniItemID)
        {
            if (PeperoniPlaced == 0)
            {
                PeperoniPlaced = 1;
                Text1.text = "Peperoni(1/2)";

                inventory.RemoveItem();


            }

            if (PeperoniPlaced == 1)
            {

                PeperoniPlaced = 2;
                Text2.text = "Peperoni(2/2)";

                inventory.RemoveItem();

                return;

            }
        }

        if (inventory.CurrentItemID == OliveItemID)
        {
            if (OlivePlaced == 0)
            {
                OlivePlaced = 1;
                Text1.text = "Olive(1/2)";

                inventory.RemoveItem();


            }

            if (OlivePlaced == 1)
            {

                OlivePlaced = 2;
                Text2.text = "Oliv(2/2)";

                inventory.RemoveItem();

                return;

            }
        }

        if (inventory.CurrentItemID == PepperItemID)
        {
            if (PepperPlaced == 0)
            {
                PepperPlaced = 1;
                Text1.text = "Pepper(1/2)";

                inventory.RemoveItem();


            }

            if (PepperPlaced == 1)
            {

                PepperPlaced = 2;
                Text2.text = "Pepper(2/2)";

                inventory.RemoveItem();

                return;

            }
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
