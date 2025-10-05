using UnityEngine;

namespace razz
{
    public class AutoPickDrop : MonoBehaviour
    {
        [Tooltip("The Interactor that will perform the auto pick/drop")]
        public Interactor interactor;
        [Tooltip("The InteractorObject to auto interact with")]
        public InteractorObject interactorObject;
        [Tooltip("True if currently interacting with the object. Toggles between pick/drop states.")]
        public bool interacted;

        [Header("Activation Area")]
        [Tooltip("Use a custom trigger area instead of global proximity checking")]
        public bool useActivationArea = false;
        [Tooltip("Size of the activation trigger area in local units")]
        public Vector3 areaSize = Vector3.one;
        [Tooltip("Offset of the activation area from this transform")]
        public Vector3 areaOffset = Vector3.zero;

        private bool interactorInArea = false;
        private bool canPerformAction = true;
        private BoxCollider areaCollider;
        private Transform areaTransform;

        private void Start()
        {
            SetupActivationArea();
        }

        private void SetupActivationArea()
        {
            if (!useActivationArea) return;

            GameObject areaObject = new GameObject("ActivationArea");
            areaObject.transform.SetParent(null, false);
            areaObject.transform.position = transform.position + areaOffset;
            areaObject.transform.rotation = Quaternion.identity;

            areaTransform = areaObject.transform;

            areaCollider = areaObject.AddComponent<BoxCollider>();
            areaCollider.size = areaSize;
            areaCollider.isTrigger = true;

            AreaTrigger trigger = areaObject.AddComponent<AreaTrigger>();
            trigger.autoPickDrop = this;
        }

        private void Update()
        {
            if (!interactor || !interactor.isActiveAndEnabled) return;
            if (!interactorObject || !interactorObject.isActiveAndEnabled) return;

            if (useActivationArea && areaTransform != null)
            {
                areaTransform.rotation = Quaternion.identity;
            }

            if (useActivationArea)
            {
                if (interactorInArea && canPerformAction)
                {
                    if (!interacted && CheckTargetValid() && interactor.IsInteractable(interactorObject))
                    {
                        interactor.StartStopInteraction(interactorObject);
                        if (interactor.IsInteractingWith(interactorObject))
                        {
                            interacted = true;
                            canPerformAction = false;
                        }
                    }
                    else if (interacted)
                    {
                        interactor.StartStopInteraction(interactorObject);
                        if (!interactor.IsInteractingWith(interactorObject))
                        {
                            interacted = false;
                            canPerformAction = false;
                        }
                    }
                }
            }
            else
            {
                if (interacted) return; //Skip if interaction started before
                if (CheckTargetValid()) //Check if conditions are alright
                {
                    if (interactor.IsInteractable(interactorObject)) //Check if interactorTarget is in legit position
                    {
                        interactor.StartStopInteraction(interactorObject); //Try to start, usually for a few frames because InteractorObject should also set some flags
                        if (interactor.IsInteractingWith(interactorObject)) //If interaction started, set interacted to prevent spam
                        {
                            interacted = true;
                        }
                    }
                }
            }
        }

        private bool CheckTargetValid()
        {
            if (!interactor || !interactorObject) return false;
            if (!interactor.CheckInteraction(interactorObject)) return false;
            if (interactorObject.used) return false;
            return true;
        }

        public void OnInteractorEnter()
        {
            interactorInArea = true;
        }

        public void OnInteractorExit()
        {
            interactorInArea = false;
            canPerformAction = true;
        }

        private void OnDrawGizmosSelected()
        {
            if (!useActivationArea) return;

            Gizmos.color = interactorInArea ? Color.red : Color.green;
            Vector3 worldPos = transform.position + areaOffset;
            Gizmos.DrawWireCube(worldPos, areaSize);
        }

        private void OnValidate()
        {
            if (useActivationArea && areaCollider != null)
            {
                areaCollider.size = areaSize;
                areaTransform.position = transform.position + areaOffset;
            }
        }
    }

    public class AreaTrigger : MonoBehaviour
    {
        public AutoPickDrop autoPickDrop;

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Interactor>() && other is CapsuleCollider)
            {
                autoPickDrop.OnInteractorEnter();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<Interactor>() && other is CapsuleCollider)
            {
                autoPickDrop.OnInteractorExit();
            }
        }
    }
}
