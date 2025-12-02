using UnityEngine;

public enum FoodType { Cheese, Pepperoni, Dough, Table, None }

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
        if (eventComp != null)
        {
            eventComp.InteractCall();
            return;
        }

        if (foodType != FoodType.None && foodSystem != null)
        {
            switch (foodType)
            {
                case FoodType.Cheese:
                    foodSystem.TakeCheese();
                    break;
                case FoodType.Pepperoni:
                    foodSystem.Takepepperoni();
                    break;
                case FoodType.Dough:
                    foodSystem.TakeDough();
                    break;
                case FoodType.Table:
                    foodSystem.PlaceFood();
                    break;
            }
            return;
        }

        if (itemComp != null)
        {
            if (itemComp.pickupSound != null)
                AudioSource.PlayClipAtPoint(itemComp.pickupSound, transform.position);

            
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
            case FoodType.Cheese: return "take Cheese";
            case FoodType.Pepperoni: return "take Pepperoni";
            case FoodType.Dough: return "take Dough";
            case FoodType.Table: return "place food";
            default: return "Press E";
        }
    }

}
