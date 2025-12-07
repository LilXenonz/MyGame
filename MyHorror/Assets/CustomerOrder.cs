using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.AI;

public class CustomerOrder : MonoBehaviour
{
    public PizzaRecipe currentOrder;
    public RecipeData database;

    public NavMeshAgent customer;
    public Transform orderPoint;
    public Transform spawnPoint;
    public Transform servePoint;
    public Transform[] sitdownPlaces;

    public Animator animator;

    public BoxCollider boxCollider;

    public Friend Talk;

    void Update()
    {
        if (!customer.hasPath && customer.remainingDistance <= 0.1f)
        {
            animator.SetInteger("C", 1);
        }
        else
        {
            animator.SetInteger("C", 0);
            boxCollider.isTrigger = false;
        }
    }

    void Start()
    {
        WalkUp();
        boxCollider.isTrigger = true;

        transform.position = spawnPoint.transform.position;
    }

    public void AddOrder(int id)
    {

        Talk.talkToFriend();

        if (id < database.Items.Count)
        {
            currentOrder = database.Items[id];
        }
    }



    public void WalkUp()
    {
        customer.SetDestination(orderPoint.position);

    }

    public void TakePizza()
    {
        
       int random = Random.Range(0, sitdownPlaces.Length);
        

        customer.SetDestination(sitdownPlaces[random].position);

    }

    public void ReceivePizza(GameObject pizza)
    {

        PizzaID pizzaID = pizza.GetComponent<PizzaID>();
        if (pizzaID.pizzaID == currentOrder.cookedPizzaID)
        {
            
        }

    }


}