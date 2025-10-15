using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Auto Player Parent")]
    public class AutoPlayerParent : MonoBehaviour, ICharacterControllerHit
    {
        public Transform Parent;

        public void OnCharacterControllerEnter(CharacterController controller)
        {
            Transform parent = Parent != null ? Parent : transform;
        }

        public void OnCharacterControllerExit()
        {
        }
    }
}