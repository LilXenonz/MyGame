using UnityEngine;
using UnityEngine.Events;

public class OntriggerEnter : MonoBehaviour
{
    public UnityEvent interactEvent;

    
    private void OnTriggerEnter()
    {
        interactEvent.Invoke();
    }
}
