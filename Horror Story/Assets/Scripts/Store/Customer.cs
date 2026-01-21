using DialogueEditor;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class Customer : MonoBehaviour
{

    public List<NavPoint> points = new List<NavPoint>();

    public float moveSpeed;
    private float currentWaitTime;

    public Animator anim;

    public enum CustomerState { entering, browsing, queing, atCheckout, leaving, creeping }
    public CustomerState currentState;

    public int maxBrowsPoints = 5;
    private int browsePointsRemain;

    public float browseTime;

    public FurnitureController currentShelfCase;

    public GameObject shoppingBag;
    private bool hasGrabbed;
    public float waitAfterGrabbing = 0.5f;

    private List<StockObject> stockInBag = new List<StockObject>();

    private Vector3 queuePoint;

    public LookAtFunc lookAtScript;

    private float currentSpeed;

    public NPCConversation conversation;


    private void Start()
    {
        currentSpeed = moveSpeed;

        points.Clear();
        points.AddRange(GetEntryPoints());

        if (points.Count > 0)
        {
            transform.position = points[0].point.position;

            currentWaitTime = points[0].waitTime;
        }

        //points.AddRange (CustomerManager.instance.GetExitPoint());


    }

    private void Update()
    {
        /*if(points.Count > 0)
        {
            MoveToPoint();
        }*/

        switch (currentState)
        {
            case CustomerState.entering:

                if (points.Count > 0)
                {
                    MoveToPoint();
                }
                else
                {
                    StartCreeping();
                }

                break;

            case CustomerState.creeping:

                if (points.Count > 0)
                {
                    MoveToPoint();
                    moveSpeed = 1;
                    lookAtScript.IKActive = true;

                    Vector3 lookDir = lookAtScript.LookAtObj.position - transform.position;
                    lookDir.y = 0; 

                    if (lookDir.sqrMagnitude > 0.01f)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(lookDir);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 2f);
                    }

                }
                else
                {
                    //StartLeaving();

                    lookAtScript.IKActive = false;
                    moveSpeed = currentSpeed;


                    currentState = CustomerState.browsing;

                    browsePointsRemain = Random.Range(1, maxBrowsPoints + 1);

                    browsePointsRemain = Mathf.Clamp(browsePointsRemain, 1, StoreController.instance.shelvingCases.Count);

                    GetBrowsePoint();
                }

                break;


            case CustomerState.browsing:

                MoveToPoint();

                if (points.Count == 0)
                {
                    if (hasGrabbed == false)
                    {
                        GrabStock();
                    }
                    else
                    {
                        hasGrabbed = false;

                        browsePointsRemain--;
                        if (browsePointsRemain > 0)
                        {
                            GetBrowsePoint();
                        }
                        else
                        {
                            //StartLeaving();
                            if (stockInBag.Count > 0)
                            {
                                Checkout.instance.AddCustomerToQueue(this);

                                currentState = CustomerState.queing;
                            }
                            else
                            {
                                StartLeaving();
                            }

                        }
                    }



                }

                break;


            case CustomerState.queing:

                transform.position = Vector3.MoveTowards(transform.position, queuePoint, moveSpeed * Time.deltaTime);

                if (Vector3.Distance(transform.position, queuePoint) > 0.1f)
                {
                    anim.SetBool("isMoving", true);
                }
                else
                {
                    anim.SetBool("isMoving", false);
                }

                break;


            case CustomerState.atCheckout:

                break;


            case CustomerState.leaving:

                if (points.Count > 0)
                {
                    MoveToPoint();
                }
                else
                {
                    Destroy(gameObject);
                    ObjectiveManager.instance.CompleteCurrentObjective();
                }

                break;
        }
    }



    public void MoveToPoint()
    {
        if (points.Count > 0)
        {

            bool isMoving = true;

            Vector3 targetPosition = new Vector3(points[0].point.position.x, transform.position.y, points[0].point.position.z);

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            transform.LookAt(targetPosition);

            if (Vector3.Distance(transform.position, targetPosition) < 0.25f)
            {
                isMoving = false;

                currentWaitTime -= Time.deltaTime;

                if (currentWaitTime <= 0)
                {
                    StartNextPoint();

                }

            }

            anim.SetBool("isMoving", isMoving);
        }
        else
        {
            StartNextPoint();
        }

    }

    public void StartNextPoint()
    {
        if (points.Count > 0)
        {
            points.RemoveAt(0);

            if (points.Count > 0)
            {
                currentWaitTime = points[0].waitTime;
            }
        }

    }

    public void StartLeaving()
    {
        currentState = CustomerState.leaving;

        points.Clear();
        points.AddRange(GetExitPoint());
    }

    public void StartCreeping()
    {
        currentState = CustomerState.creeping;

        points.Clear();
        points.AddRange(creepPoints);
    }

    void GetBrowsePoint()
    {
        points.Clear();

        int selectedShelf = Random.Range(0, StoreController.instance.shelvingCases.Count);

        points.Add(new NavPoint());
        points[0].point = StoreController.instance.shelvingCases[selectedShelf].standPoing;

        points[0].waitTime = browseTime * Random.Range(0.75f, 1.25f);

        currentWaitTime = points[0].waitTime;

        currentShelfCase = StoreController.instance.shelvingCases[selectedShelf];

    }

    public void GrabStock()
    {

        hasGrabbed = true;

        int shelf = Random.Range(0, currentShelfCase.shelves.Count);

        StockObject stock = currentShelfCase.shelves[shelf].GetStock();

        if (stock != null)
        {
            stock.transform.SetParent(shoppingBag.transform);
            stockInBag.Add(stock);
            stock.PlaceInBag();

            shoppingBag.SetActive(true);

            points.Clear();
            points.Add(new NavPoint());
            points[0].point = currentShelfCase.standPoing;
            points[0].waitTime = waitAfterGrabbing * Random.Range(0.75f, 1.25f);
            currentWaitTime = points[0].waitTime;
        }


    }

    public void UpdateQueuePoint(Vector3 newPoint)
    {
        queuePoint = newPoint;
        transform.LookAt(queuePoint);
    }

    public float GetTotalSpent()
    {
        float total = 0f;

        foreach (StockObject stock in stockInBag)
        {
            total += stock.info.price;
        }

        return total;
    }

    //public List<Customer> customersToSpawn = new List<Customer>();

    public List<NavPoint> entryPointsLeft, entryPointsRight, creepPoints;




    public List<NavPoint> GetEntryPoints()
    {
        List<NavPoint> points = new List<NavPoint>();

        if (Random.value < 0.5f)
        {
            points.AddRange(entryPointsLeft);
        }
        else
        {
            points.AddRange(entryPointsRight);

        }

        return points;
    }

    public List<NavPoint> GetExitPoint()
    {
        List<NavPoint> points = new List<NavPoint>();

        List<NavPoint> temp = new List<NavPoint>();

        if (Random.value < 0.5f)
        {
            temp.AddRange(entryPointsLeft);
        }
        else
        {
            temp.AddRange(entryPointsRight);

        }

        for (int i = temp.Count - 1; i >= 0; i--)
        {
            points.Add(temp[i]);
        }

        return points;


    }


    
}


    [System.Serializable]
public class NavPoint
{
    public Transform point;
    public float waitTime;
}
