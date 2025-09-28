using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class NPCSystem : MonoBehaviour
{

    NavMeshAgent agent;

    Animator animator;

    public GameObject ObjectToFollow;

    bool ShouldFollowPlayer = false;

    public GameObject[] randomSpots;

    bool ShouldGoRandomSpot = true;

    public int RandomSpotNumber = 0;

    bool ShouldGoFinalDes = false;

    public GameObject FinalDesGB;

    bool AITalking = false;

    bool AITalkedOnce = false;

    //talk

    public Text SubText;

    public GameObject TalkPanel;

    //talk


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShouldFollowPlayer = false;

        ShouldGoRandomSpot=false;

        ShouldGoFinalDes=true;

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

        if (ShouldGoFinalDes == true && AITalking == false)
        {
         
            

            float DistanceUsToAI = Vector3.Distance(transform.position, ObjectToFollow.transform.position);

            float DistanceFinal = Vector3.Distance(transform.position, FinalDesGB.transform.position);

            if(DistanceUsToAI <= 15)
            {
                if (DistanceFinal < 5)
                {
                    //reached destination

                    agent.isStopped = true;
                    animator.SetInteger("C", 0);
                }
                else if (DistanceFinal >= 5)
                {
                    //should go to destination
                    agent.isStopped = false;
                    animator.SetInteger("C", 1);
                    agent.speed = 3;

                    agent.SetDestination(FinalDesGB.transform.position);

                    AITalkedOnce = false;
                }
            }
            else if (DistanceUsToAI > 15)
            {
                agent.isStopped = true;
                animator.SetInteger("C", 0);

                if(AITalkedOnce == false)
                {

                    StartCoroutine(NPCTalking());

                }

            }

        }

    }

    IEnumerator NPCTalking()
    {
        AITalking = true;

        AITalkedOnce = true;

        TalkPanel.SetActive(true);

        SubText.text = "Move Faste, Come on man";

        yield return new WaitForSeconds(2f);

        TalkPanel.SetActive(false);

        SubText.text = "";

        AITalking = false;

    }

}
