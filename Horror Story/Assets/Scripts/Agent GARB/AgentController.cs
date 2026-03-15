using UnityEngine;
using UnityEngine.AI;


public class AgentController : MonoBehaviour
{
    public NavMeshAgent agent;

    public int destanationGoal = 40;

    private void Start()
    {
        Vector3 target = transform.position + Vector3.forward * destanationGoal;
        agent.SetDestination(target);
    }
}
