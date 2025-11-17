using UnityEngine;
using UnityEngine.UI;

public class FoodSystem : MonoBehaviour
{
    public GameObject Canvas;

    public GameObject[] BrokenEggs;

    public Toggle EggsToggle;
    public Toggle SaltToggle;
    public Toggle PepperToggle;

    public Text EggsText;
    public Text InteractionText;

    public Inventory inventory;

    // IDs must match your ItemsDatabase / Item components
    public int eggItemID = 6;
    public int pepperItemID = 7;
    public int saltItemID = 8;

    private int eggsPlaced = 0;

    private void Awake()
    {
        if (inventory == null)
            inventory = FindObjectOfType<Inventory>();

        if (inventory == null)
            Debug.LogError("[FoodSystem] Inventory is null. Assign it in inspector.");
    }

    public void TakeEggs()
    {
        // With your setup the world egg object already has Item component + prefab,
        // so you don't need to create items here. Keep behaviour minimal.
        CloseCanvas();
    }

    public void TakeSalt()
    {
        CloseCanvas();
    }

    public void TakePepper()
    {
        CloseCanvas();
    }

    // Called when interacting with the pan
    public void PlaceEggs()
    {
        if (inventory == null)
        {
            Debug.LogError("[FoodSystem.PlaceEggs] Inventory null.");
            OpenCanvas();
            return;
        }

        // Handle pepper
        if (inventory.CurrentItemID == pepperItemID)
        {
            if (PepperToggle != null) PepperToggle.isOn = true;
            else Debug.LogWarning("[FoodSystem] PepperToggle not assigned.");
            inventory.RemoveItem();
            return;
        }

        // Handle salt
        if (inventory.CurrentItemID == saltItemID)
        {
            if (SaltToggle != null) SaltToggle.isOn = true;
            else Debug.LogWarning("[FoodSystem] SaltToggle not assigned.");
            inventory.RemoveItem();
            return;
        }

        // Handle eggs: place one broken egg per egg item in inventory.
        if (inventory.CurrentItemID == eggItemID)
        {
            // safety checks
            if (BrokenEggs == null || BrokenEggs.Length == 0)
            {
                Debug.LogWarning("[FoodSystem.PlaceEggs] BrokenEggs not set or empty.");
                inventory.RemoveItem(); // still remove so player doesn't get stuck
                return;
            }

            // Activate next broken egg visual if available
            if (eggsPlaced < BrokenEggs.Length)
            {
                GameObject next = BrokenEggs[eggsPlaced];
                if (next != null)
                    next.SetActive(true);
                else
                    Debug.LogWarning($"[FoodSystem.PlaceEggs] BrokenEggs[{eggsPlaced}] is null.");

                eggsPlaced++;

                // update UI text if assigned
                if (EggsText != null)
                    EggsText.text = $"Eggs ({eggsPlaced}/{BrokenEggs.Length})";
            }

            // Remove the egg item immediately after placing one egg
            inventory.RemoveItem();

            // If we've completed all egg slots, toggle and (optionally) show result
            if (eggsPlaced >= BrokenEggs.Length)
            {
                if (EggsToggle != null) EggsToggle.isOn = true;
                Debug.Log("[FoodSystem] All eggs placed.");
            }

            // Show canvas to display pan UI/results
            OpenCanvas();
            return;
        }

        // Not holding anything relevant -> open the pan UI
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
