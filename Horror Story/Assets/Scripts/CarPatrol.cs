using DialogueEditor;
using System.Collections.Generic;
using UnityEngine;
using static Customer;

public class CarPatrol : MonoBehaviour
{
    public List<NavPoint> entryPoints, creepPoints;
    public NavPoint Parkingpoint = new NavPoint();
    public List<NavPoint> points = new List<NavPoint>();

    private float currentWaitTime;

    public enum CustomerState { entering, creeping, parking, leaving  }
    public CustomerState currentState;
    
    public float moveSpeed;

    public bool creep;

    private void Start()
    {
        points.Clear();
        points.AddRange(GetEntryPoints());

        if (points.Count > 0)
        {
            transform.position = points[0].point.position;

            currentWaitTime = points[0].waitTime;
        }


    }

    private void Update()
    {


        switch (currentState)
        {
            case CustomerState.entering:

                if (points.Count > 0)
                {
                    MoveToPoint();
                }
                else
                {
                    if (creep)
                    {
                        StartCreeping();
                    }
                    else 
                    {
                        GetParkingPoint();
                    }
                }

                break;

            case CustomerState.creeping:

                if (points.Count > 0)
                {
                    MoveToPoint();

                }
                else
                {
                    GetParkingPoint();
                }

                break;

            case CustomerState.parking:

                if (points.Count > 0)
                {
                    MoveToPoint();

                }
                else
                {
                    StartLeaving();
                }

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
    public List<NavPoint> GetEntryPoints()
    {
        List<NavPoint> points = new List<NavPoint>();

        
       points.AddRange(entryPoints);


        return points;
    }

    public List<NavPoint> GetExitPoint()
    {
        List<NavPoint> points = new List<NavPoint>();

        List<NavPoint> temp = new List<NavPoint>();

        
        temp.AddRange(entryPoints);


        for (int i = temp.Count - 1; i >= 0; i--)
        {
            points.Add(temp[i]);
        }

        return points;


    }

    void GetParkingPoint()
    {
        points.Clear();

        points.Add(Parkingpoint);

        currentWaitTime = Parkingpoint.waitTime;

        currentState = CustomerState.parking;


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

    public void MoveToPoint()
    {
        if (points.Count > 0)
        {

            Vector3 targetPosition = new Vector3(points[0].point.position.x, transform.position.y, points[0].point.position.z);

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            transform.LookAt(targetPosition);

            if (Vector3.Distance(transform.position, targetPosition) < 0.25f)
            {
                currentWaitTime -= Time.deltaTime;

                if (currentWaitTime <= 0)
                {
                    StartNextPoint();

                }

            }

        }
        else
        {
            StartNextPoint();
        }

    }


}
