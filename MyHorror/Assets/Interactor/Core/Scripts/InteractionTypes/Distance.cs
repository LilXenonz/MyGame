using UnityEngine;

namespace razz
{
    [CreateAssetMenu(fileName = "DistanceSettings", menuName = "Interactor/DistanceSettings")]
    public class Distance : InteractionTypeSettings
    {
        [Tooltip("When the object enters this range, it will be activated without any other effector rules check.")]
        public float range = 1.8f;

        public override void Init(InteractorObject interactorObject)
        {
            base.Init(interactorObject);
            //Second max priority to keep distance interaction on second top of interaction list order.

            if (_intObj.interactionType == InteractionTypes.DistanceCrosshair)
            {
                _intObj.priority = int.MaxValue - 1;
            }
        }
    }
}
