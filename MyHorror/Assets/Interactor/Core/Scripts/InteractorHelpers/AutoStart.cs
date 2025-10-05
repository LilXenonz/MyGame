using UnityEngine;

namespace razz
{
    public class AutoStart : MonoBehaviour
    {
        [Tooltip("The Interactor that will perform the auto start")]
        public Interactor interactor;
        [Tooltip("The InteractorObject to auto interact with")]
        public InteractorObject interactorObject;
        [Tooltip("True if the interaction has been triggered. To prevent interacting over and over again. You can reset this if you want to re-trigger auto start.")]
        public bool interacted;
        [Tooltip("Delay in seconds before starting the interaction")]
        public float waitSeconds = 0.5f;

        private void Update()
        {
            if (!interactor || !interactor.isActiveAndEnabled) return;
            if (!interactorObject || !interactorObject.isActiveAndEnabled) return;

            if (interacted) return; //Skip if interaction started before

            if (CheckTargetValid()) //Check if conditions are alright
            {
                if (interactor.IsInteractable(interactorObject)) //Check if interactorTarget is in legit position
                {
                    if (waitSeconds > 0)
                    { //Wait x seconds before starting
                        waitSeconds -= Time.deltaTime;
                        return;
                    }

                    interactor.StartStopInteraction(interactorObject); //Try to start, usually for a few frames because InteractorObject should also set some flags
                    if (interactor.IsInteractingWith(interactorObject)) //If interaction started, set interacted to prevent spam
                    {
                        interacted = true;
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
    }
}
