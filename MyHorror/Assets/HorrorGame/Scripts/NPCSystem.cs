using UnityEngine;
using UnityEngine.AI;

public class NPCSystem : MonoBehaviour
{

    NavMeshAgent agent;

    Animator animator;

    public GameObject ObjectToFollow;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
        float Distance = Vector3.Distance(transform.position, ObjectToFollow.transform.position);

        if(Distance < 5)
        {
            //idle
            agent.isStopped = true;

            animator.SetInteger("C", 0);
        }
        else if(Distance >= 5 && Distance < 12)
        {
            //walkiing
            agent.isStopped = false;
            animator.SetInteger("C", 1);

            agent.speed = 3;

            agent.SetDestination(ObjectToFollow.transform.position);
        }
        else if(Distance >= 12)
        {
            //run
            agent.isStopped = false;
            animator.SetInteger("C", 2);

            agent.speed = 6;

            agent.SetDestination(ObjectToFollow.transform.position);

        }


    }
}
