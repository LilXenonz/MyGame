using UnityEngine;
namespace razz
{
    public class MoveButton : MonoBehaviour
    {
        public Vector3 moveAmount;
        public KeyCode moveKey = KeyCode.C;

        private Vector3 cachedPosition;
        private bool isMoved = false;

        void Update()
        {
            bool keyPressed = InteractorInput.GetKey(moveKey);

            if (keyPressed && !isMoved)
            {
                cachedPosition = transform.position;
                transform.position += moveAmount;
                isMoved = true;
            }
            else if (!keyPressed && isMoved)
            {
                transform.position = cachedPosition;
                isMoved = false;
            }
        }
    }
}
