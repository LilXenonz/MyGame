using UnityEngine;
using System.Collections.Generic;

public class PlaceFood : MonoBehaviour
{
    public bool hasDough = false;
    public bool hasSauce = false;
    public List<int> placedToppings = new List<int>();

    public GameObject doughVisual;
    public GameObject sauceVisual;
    public GameObject cheeseVisual;
    public GameObject pepperoniVisual;

    public Transform pizzaSpawnPoint;
    public GameObject currentUncookedPizza;

    [HideInInspector]
    public PizzaRecipe currentRecipe;
    private bool pizzaComplete = false;

    private Dictionary<int, GameObject> ingredientVisuals;

    public Inventory inventory;
    public CustomerOrder order;    


    void Start()
    {
        ingredientVisuals = new Dictionary<int, GameObject>()
        {
            {12, doughVisual},
            {11, sauceVisual},
            {2, cheeseVisual},
            {3, pepperoniVisual}
        };
        HideAllVisuals();

        currentRecipe = order.currentOrder;
    }

    public void TryPlaceIngredient()
    {
        if (pizzaComplete) return;

        if (!hasDough) //can place
        {
            if (inventory.CurrentItemID == 12)
            {
                PlaceDough();
                inventory.RemoveItem();
            }
            
            return;
        }

        if (!hasSauce && hasDough)// no sause and dough is placed
        {
            if (inventory.CurrentItemID == 11)
            {
                PlaceSauce();
                inventory.RemoveItem();

            }       

            return;
        }

        if(hasSauce && hasDough)
        {
            if (currentRecipe == null) return;

            bool isValidTopping = false;
            foreach (int toppingID in currentRecipe.toppingIDs)
            {
                if (inventory.CurrentItemID == toppingID) isValidTopping = true;
            }

            if (!isValidTopping) return;

            if (placedToppings.Contains(inventory.CurrentItemID)) return;

            PlaceTopping(inventory.CurrentItemID);

            inventory.RemoveItem();
        }
        
    }

    void PlaceDough()
    {
        if (ingredientVisuals.ContainsKey(12)) ingredientVisuals[12].SetActive(true);
        hasDough = true;
    }

    void PlaceSauce()
    {
        if (ingredientVisuals.ContainsKey(11)) ingredientVisuals[11].SetActive(true);
        hasSauce = true;
    }

    void PlaceTopping(int toppingID)
    {
        if (ingredientVisuals.ContainsKey(toppingID)) ingredientVisuals[toppingID].SetActive(true);
        placedToppings.Add(toppingID);
        CheckIfPizzaComplete();
    }

    void CheckIfPizzaComplete()
    {
        if (currentRecipe == null) return;

        bool allToppingsPlaced = true;
        foreach (int requiredTopping in currentRecipe.toppingIDs)
        {
            if (!placedToppings.Contains(requiredTopping)) allToppingsPlaced = false;
        }

        if (allToppingsPlaced && hasDough && hasSauce)
        {
            pizzaComplete = true;
            SpawnUncookedPizza();
        }
    }

    void SpawnUncookedPizza()
    {
        if (currentRecipe == null) return;

        HideAllVisuals();

        currentUncookedPizza = Instantiate(currentRecipe.uncookedPrefab, pizzaSpawnPoint.position, pizzaSpawnPoint.rotation);

        PizzaID pizzaID = currentUncookedPizza.AddComponent<PizzaID>();
        pizzaID.pizzaID = currentRecipe.uncookedPizzaID;
        pizzaID.isCooked = false;
    }

    public GameObject TakeUncookedPizza()
    {
        if (!pizzaComplete || currentUncookedPizza == null) return null;

        GameObject pizza = currentUncookedPizza;
        currentUncookedPizza = null;
        ResetBoard();
        return pizza;
    }

    void HideAllVisuals()
    {
        foreach (var visual in ingredientVisuals.Values)
        {
            if (visual != null) visual.SetActive(false);
        }
    }

    void ResetBoard()
    {
        hasDough = false;
        hasSauce = false;
        placedToppings.Clear();
        pizzaComplete = false;
        HideAllVisuals();
    }

    public void SetRecipe(PizzaRecipe recipe)
    {
        currentRecipe = recipe;
        ResetBoard();
    }
}