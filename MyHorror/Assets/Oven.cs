using UnityEngine;

public class Oven : MonoBehaviour
{
    public Transform pizzaPlacePosition;
    public Transform cookedPizzaSpawn;
    public float cookTime = 5f;

    private GameObject pizzaInOven;
    private bool isCooking = false;
    private float cookTimer = 0f;

    public RecipeData[] pizzaRecipes;

    public CustomerOrder order;
    public PlaceFood food;

    public Inventory inventory;

    void Update()
    {
        if (isCooking && pizzaInOven != null)
        {
            cookTimer += Time.deltaTime;
            if (cookTimer >= cookTime) FinishCooking();
        }
    }

    public void PlacePizzaInOven()
    {
        if(inventory.CurrentItemID == 13)
        {


            inventory.CurrentItem.transform.parent = null;
            inventory.CurrentItem.transform.position = pizzaPlacePosition.position;
            inventory.CurrentItem.transform.rotation = pizzaPlacePosition.rotation;


            pizzaInOven = food.currentRecipe.uncookedPrefab;

            isCooking = true;
            cookTimer = 0f; 
        }

    }

    void FinishCooking()
    {

        isCooking = false;
        PizzaID uncookedPizzaID = pizzaInOven.GetComponent<PizzaID>();

        Destroy(pizzaInOven);

        pizzaInOven = null;

        GameObject cookedPizza = Instantiate(food.currentRecipe.cookedPrefab, cookedPizzaSpawn.position, cookedPizzaSpawn.rotation);

        PizzaID cookedPizzaID = cookedPizza.AddComponent<PizzaID>();
        cookedPizzaID.pizzaID = food.currentRecipe.cookedPizzaID;
        cookedPizzaID.isCooked = true;
    }


    public GameObject TakeCookedPizza()
    {
        Collider[] colliders = Physics.OverlapSphere(cookedPizzaSpawn.position, 1f);
        foreach (Collider collider in colliders)
        {
            PizzaID pizzaID = collider.GetComponent<PizzaID>();
            if (pizzaID != null && pizzaID.isCooked) return collider.gameObject;
        }
        return null;
    }
}