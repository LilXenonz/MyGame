using UnityEngine;

[System.Serializable]
public class PizzaRecipe
{
    public string pizzaName;
    public int doughID = 12;
    public int sauceID = 1;
    public int[] toppingIDs;
    public int uncookedPizzaID;
    public int cookedPizzaID;
    public GameObject uncookedPrefab;
    public GameObject cookedPrefab;
}