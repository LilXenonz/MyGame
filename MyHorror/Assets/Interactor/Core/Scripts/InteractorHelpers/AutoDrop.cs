using UnityEngine;

namespace razz
{
    public class AutoDrop : MonoBehaviour
    {
        [Tooltip("The Interactor that will perform the auto drop")]
        public Interactor interactor;
        [Tooltip("The InteractorObject to auto drop (must have a dropLocation set in pickup settings)")]
        public InteractorObject interactorObject;
        [Tooltip("Force drop at location by destroying and recreating object at drop position. Checks effector orbital rules but ignores dropped state.")]
        public bool forceDrop = false;
        [Tooltip("True if the object has been dropped. To prevent dropping over and over again. You can reset this if you want to re-trigger auto drop.")]
        public bool dropped;
        [Tooltip("Delay in seconds before dropping the object")]
        public float waitSeconds = 0.5f;

        private void Update()
        {
            if (!interactor || !interactor.isActiveAndEnabled) return;
            if (!interactorObject || !interactorObject.isActiveAndEnabled) return;

            if (forceDrop)
            {
                if (interactor.IsInteractingWith(interactorObject)) //If using the object
                    TryToDrop();
            }
            else
            {
                if (!dropped && interactor.IsInteractingWith(interactorObject)) //If not dropped before and using the object
                    TryToDrop();
            }
        }

        private void TryToDrop()
        {
            if (interactorObject.dropLocation) //If object has drop location transform in its pick up settings
            {
                if (forceDrop)
                {
                    if (interactor.EffectorCheckOrbitalPosition(this.transform, (int)interactorObject.childTargets[0].effectorType))
                    {
                        InteractorTarget currentTarget = interactorObject.childTargets[0];
                        Vector3 targetOffset = currentTarget.transform.position - interactorObject.transform.position;
                        Quaternion targetRotOffset = Quaternion.Inverse(interactorObject.transform.rotation) * currentTarget.transform.rotation;

                        // Calculate new object position so target aligns with drop location
                        Vector3 newObjectPos = interactorObject.dropLocation.position - targetOffset;
                        Quaternion newObjectRot = interactorObject.dropLocation.rotation * Quaternion.Inverse(targetRotOffset);

                        // Check if collider is disabled
                        bool colliderWasDisabled = interactorObject.col && !interactorObject.col.enabled;

                        // Force disconnect first
                        interactor.DisconnectAllForThis(interactorObject);

                        // Destroy current object and create new one
                        GameObject newObject = Instantiate(interactorObject.gameObject, newObjectPos, newObjectRot);
                        newObject.name = interactorObject.name; // Remove (Clone) from name
                        Destroy(interactorObject.gameObject);

                        // Reset new object state
                        InteractorObject newIntObj = newObject.GetComponent<InteractorObject>();
                        newIntObj.used = false;

                        // Enable collider if it was disabled
                        if (colliderWasDisabled && newIntObj.col)
                        {
                            newIntObj.col.enabled = true;
                        }

                        dropped = true;
                    }
                }
                else if (interactor.EffectorCheckOrbitalPosition(interactorObject.dropLocation, (int)interactorObject.childTargets[0].effectorType))
                {//Check if drop location is reachable with OrbitalReach for its target effector type

                    if (waitSeconds > 0)
                    { //Wait x seconds before dropping
                        waitSeconds -= Time.deltaTime;
                        return;
                    }

                    interactor.StartStopInteraction(interactorObject); //Drop

                    if (!interactor.IsInteractingWith(interactorObject)) //Chech if dropped successfully
                    {
                        dropped = true;
                    }
                }
            }
        }
    }
}
