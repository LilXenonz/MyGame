using UnityEngine;

public enum FoodType { Eggs, Salt, Pepper, Pan, None }

[RequireComponent(typeof(Collider))]
public class FoodInteractable : MonoBehaviour
{
    public FoodType foodType = FoodType.None;
    public FoodSystem foodSystem;
    public string customLabel = "";

    private Item itemComp;
    private InteractCallEvent eventComp;

    private void Awake()
    {
        itemComp = GetComponent<Item>();
        eventComp = GetComponent<InteractCallEvent>();
        foodSystem = FindObjectOfType<FoodSystem>();
    }

    public void OnInteract()
    {
        // event still has priority if it's explicitly an event on this object
        if (eventComp != null)
        {
            eventComp.InteractCall();
            return;
        }

        // If this is a food action (Eggs/Salt/Pepper/Pan) -> handle it first
        if (foodType != FoodType.None && foodSystem != null)
        {
            switch (foodType)
            {
                case FoodType.Eggs:
                    foodSystem.TakeEggs();
                    break;
                case FoodType.Salt:
                    foodSystem.TakeSalt();
                    break;
                case FoodType.Pepper:
                    foodSystem.TakePepper();
                    break;
                case FoodType.Pan:
                    foodSystem.PlaceEggs();
                    break;
            }
            return;
        }

        // Otherwise if this is a plain item, play pickup sound (CamInteraction will call inventory.AddItem)
        if (itemComp != null)
        {
            if (itemComp.pickupSound != null)
                AudioSource.PlayClipAtPoint(itemComp.pickupSound, transform.position);

            // Optional: if you want FoodInteractable to also add to the inventory itself, you could
            // call the inventory here; but current flow expects CamInteraction to call inventory.AddItem
            return;
        }

        Debug.Log($"No action for {name}");
    }

    public string GetInteractionText()
    {
        if (!string.IsNullOrEmpty(customLabel)) return customLabel;
        if (itemComp != null) return "Take " + (itemComp.itemName ?? "Item");
        if (eventComp != null) return "Use";
        switch (foodType)
        {
            case FoodType.Eggs: return "take eggs";
            case FoodType.Salt: return "take salt";
            case FoodType.Pepper: return "take pepper";
            case FoodType.Pan: return "place food";
            default: return "Press E";
        }
    }

}
