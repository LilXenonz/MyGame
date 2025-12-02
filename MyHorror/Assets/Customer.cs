using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))]
public class Customer : MonoBehaviour
{
    [Header("Order (editable in inspector)")]
    [Tooltip("Human readable order name")]
    public string orderName = "Mozzarella Pizza";

    [Tooltip("Human readable step labels (optional). E.g. Dough, Sauce, Cheese")]
    public string[] stepNames;

    [Tooltip("Item IDs required for each step. Must match your ItemsDatabase IDs and length should match stepNames (or can be used alone).")]
    public int[] stepItemIDs;

    [Header("Spawn & Movement")]
    [Tooltip("Optional spawn position (if null, use current transform)")]
    public Transform spawnPoint;
    [Tooltip("Where the customer walks to (counter)")]
    public Transform counterSpot;
    [Tooltip("Where the customer leaves to")]
    public Transform exitSpot;
    [Tooltip("If assigned, the NavMeshAgent will be used. Otherwise the customer will lerp to the target.")]
    public NavMeshAgent agent;
    [Tooltip("Move speed used for non-NavMesh fallback")]
    public float fallbackMoveSpeed = 2f;

    [Header("Patience")]
    [Tooltip("Seconds the customer will wait at the counter before leaving angrily")]
    public float patienceSeconds = 35f;

    // internal state
    private bool reachedCounter = false;
    private bool isWaiting = false;
    private int currentStepIndex = 0; // next step to complete
    private Coroutine waitCoroutine;

    private void Awake()
    {
        // if there's a NavMeshAgent on the same GameObject and agent not set, grab it
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        // apply spawn point if set
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }
    }

    private void Start()
    {
        // Start moving to counter
        MoveToCounter();
    }

    private void MoveToCounter()
    {
        if (counterSpot == null)
        {
            Debug.LogWarning("[Customer] counterSpot not set. Customer will not move.");
            return;
        }

        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(counterSpot.position);
        }
        else
        {
            // no NavMeshAgent -> simple coroutine move
            StartCoroutine(FallbackMoveTo(counterSpot.position));
        }
    }

    private IEnumerator FallbackMoveTo(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > 0.2f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, fallbackMoveSpeed * Time.deltaTime);
            yield return null;
        }
        OnReachedCounter();
    }

    private void Update()
    {
        // if using NavMesh agent, detect arrival
        if (!reachedCounter && agent != null && counterSpot != null)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.05f)
            {
                OnReachedCounter();
            }
        }
    }

    private void OnReachedCounter()
    {
        if (reachedCounter) return;
        reachedCounter = true;
        Debug.Log($"[Customer] Arrived at counter. Order: {orderName}");
        // Announce order in console (or show UI here)
        Debug.Log(GetOrderDescription());

        // start patience countdown
        if (!isWaiting)
        {
            waitCoroutine = StartCoroutine(PatienceTimer());
            isWaiting = true;
        }
    }

    private IEnumerator PatienceTimer()
    {
        float t = 0f;
        while (t < patienceSeconds)
        {
            t += Time.deltaTime;
            yield return null;
        }
        // patience ran out
        Debug.Log($"[Customer] Left angry: {orderName} (no pizza in time)");
        Leave();
    }

    private void Leave()
    {
        // stop patience coroutine
        if (waitCoroutine != null) StopCoroutine(waitCoroutine);

        // move to exit spot and destroy
        if (exitSpot != null)
        {
            if (agent != null)
            {
                agent.SetDestination(exitSpot.position);
                Destroy(gameObject, 6f);
            }
            else
            {
                StartCoroutine(FallbackMoveAndDestroy(exitSpot.position, 6f));
            }
        }
        else
        {
            Destroy(gameObject, 1f);
        }
    }

    private IEnumerator FallbackMoveAndDestroy(Vector3 target, float destroyDelay)
    {
        while (Vector3.Distance(transform.position, target) > 0.2f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, fallbackMoveSpeed * Time.deltaTime);
            yield return null;
        }
        Destroy(gameObject, destroyDelay);
    }

    // -----------------------
    // Interaction API (call these from InteractCallEvent or CamInteraction)
    // -----------------------

    // Called when the player looks at customer and presses E (hook to InteractCallEvent)
    public void OnPlayerInteract()
    {
        // show the customer's order in console — you can replace this with any UI
        Debug.Log($"[Customer] Player interacted. Order: {orderName} Step {currentStepIndex + 1}/{stepItemIDs.Length}: {GetCurrentStepName()} (id {GetCurrentStepID()})");
    }

    // Try to advance the order using the player's currently held item.
    // Returns true if the step was accepted, false otherwise.
    // Use this if your interaction flow is: pick up ingredient -> look at customer -> press E to give ingredient.
    public bool TryGiveHeldItemToCustomer(Inventory playerInventory)
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("[Customer] TryGiveHeldItemToCustomer: playerInventory null");
            return false;
        }

        int heldID = playerInventory.CurrentItemID;
        int expectedID = GetCurrentStepID();

        if (heldID == expectedID)
        {
            Debug.Log($"[Customer] Accepted ingredient for step {currentStepIndex + 1}: {GetCurrentStepName()}");
            // optionally remove the item from player if you want to consume it:
            // playerInventory.RemoveItem();
            currentStepIndex++;

            // if finished all steps -> customer is served and leaves happily
            if (currentStepIndex >= stepItemIDs.Length)
            {
                OnOrderComplete();
            }
            return true;
        }
        else
        {
            Debug.Log($"[Customer] Wrong ingredient. Expected {expectedID} but holding {heldID}");
            return false;
        }
    }

    // Serve a pizza by providing an array of itemIDs (complete pizza)
    // This checks equality of the entire order (same length and same ids in order).
    public bool ServeWithItemIDSequence(int[] providedSequence)
    {
        if (providedSequence == null) return false;
        if (providedSequence.Length != stepItemIDs.Length) return false;

        for (int i = 0; i < stepItemIDs.Length; i++)
        {
            if (providedSequence[i] != stepItemIDs[i]) return false;
        }

        OnOrderComplete();
        return true;
    }

    // Called when customer's full order is completed
    private void OnOrderComplete()
    {
        Debug.Log($"[Customer] Served: {orderName} - Thank you!");
        // stop patience
        if (waitCoroutine != null) StopCoroutine(waitCoroutine);

        // optionally give reward, increase score, etc.

        // leave after short delay
        Invoke(nameof(Leave), 1.0f);
    }

    // -----------------------
    // Helpers
    // -----------------------
    public string GetOrderDescription()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"Order: {orderName}");

        for (int i = 0; i < stepItemIDs.Length; i++)
        {
            var name = (stepNames != null && i < stepNames.Length && !string.IsNullOrEmpty(stepNames[i])) ? stepNames[i] : $"ItemID {stepItemIDs[i]}";
            sb.AppendLine($"{i + 1}. {name} (id {stepItemIDs[i]})");
        }

        return sb.ToString();
    }

    public int GetCurrentStepID()
    {
        if (stepItemIDs == null || stepItemIDs.Length == 0 || currentStepIndex >= stepItemIDs.Length) return -1;
        return stepItemIDs[currentStepIndex];
    }

    public string GetCurrentStepName()
    {
        if (stepNames != null && currentStepIndex < stepNames.Length && !string.IsNullOrEmpty(stepNames[currentStepIndex]))
            return stepNames[currentStepIndex];
        int id = GetCurrentStepID();
        return id >= 0 ? $"ItemID {id}" : "None";
    }
}
