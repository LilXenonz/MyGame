using UnityEngine;
using UnityEngine.AI;

public class NPCSystem : MonoBehaviour
{

    NavMeshAgent agent;

    Animator animator;

    public GameObject ObjectToFollow;

    bool ShouldFollowPlayer = false;

    public GameObject[] randomSpots;

    bool ShouldGoRandomSpot = true;

    public int RandomSpotNumber = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

        if(Input.GetKeyDown(KeyCode.F))
        {
            ShouldFollowPlayer = true;
            ShouldGoRandomSpot = false;

            RandomSpotNumber = 0;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            ShouldFollowPlayer = false;
            ShouldGoRandomSpot = true;

            RandomSpotNumber = 2;
        }




        if (ShouldFollowPlayer == true)
        {
            float Distance = Vector3.Distance(transform.position, ObjectToFollow.transform.position);

            if (Distance < 5)
            {
                //idle
                agent.isStopped = true;

                animator.SetInteger("C", 0);
            }
            else if (Distance >= 5 && Distance < 12)
            {
                //walkiing
                agent.isStopped = false;
                animator.SetInteger("C", 1);

                agent.speed = 3;

                agent.SetDestination(ObjectToFollow.transform.position);
            }
            else if (Distance >= 12)
            {
                //run
                agent.isStopped = false;
                animator.SetInteger("C", 2);

                agent.speed = 6;

                agent.SetDestination(ObjectToFollow.transform.position);

            }
        } 
       

        if(ShouldGoRandomSpot == true)
        {
            if (RandomSpotNumber == 1)
            {
                
                float Distance = Vector3.Distance(transform.position, randomSpots[0].transform.position);

                agent.SetDestination(randomSpots[0].transform.position);

                if(Distance < 2)
                {
                    agent.isStopped = true;
                    animator.SetInteger("C", 0);
                }
                else if (Distance <= 2 && Distance <= 6)
                {
                    agent.isStopped = false;
                    animator.SetInteger("C", 1);

                    agent.speed = 3;
                }
                else if(Distance > 6)
                {
                    agent.isStopped=false;
                    animator.SetInteger("C", 2);
                    agent.speed = 6;


                }
            }
            else if(RandomSpotNumber == 1)
            {
                
                float Distance = Vector3.Distance(transform.position, randomSpots[1].transform.position);

                agent.SetDestination(randomSpots[1].transform.position);

                if (Distance < 2)
                {
                    agent.isStopped = true;
                    animator.SetInteger("C", 0);
                }
                else if (Distance <= 2 && Distance <= 6)
                {
                    agent.isStopped = false;
                    animator.SetInteger("C", 1);
                }
                else if (Distance > 6)
                {
                    agent.isStopped = false;
                    animator.SetInteger("C", 2);

                }
            }
            else if (RandomSpotNumber == 2)
            {

                float Distance = Vector3.Distance(transform.position, randomSpots[2].transform.position);

                agent.SetDestination(randomSpots[2].transform.position);

                if (Distance < 2)
                {
                    agent.isStopped = true;
                    animator.SetInteger("C", 0);
                }
                else if (Distance <= 2 && Distance <= 6)
                {
                    agent.isStopped = false;
                    animator.SetInteger("C", 1);
                }
                else if (Distance > 6)
                {
                    agent.isStopped = false;
                    animator.SetInteger("C", 2);

                }
            }
        }

    }
}
