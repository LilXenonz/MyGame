using UnityEngine;

namespace razz
{
    //Simplified BasicInput.cs just for starting interactions
    public class BasicInputInteract : MonoBehaviour
    {
        public KeyCode useKey = KeyCode.E;

        private Interactor _interactor;
        private bool _use;
        private bool _click;

        private void Start()
        {
            _interactor = GetComponent<Interactor>();
        }

        public bool GetUse()
        {
            return InteractorInput.GetKeyDown(useKey);
        }

        public bool GetLeftClick()
        {
            return InteractorInput.GetKeyDown(KeyCode.Mouse0);
        }

        private void Update()
        {
            if (!_interactor) return;

            if (!_use)
            {
                _use = GetUse();
            }

            if (!_click && _interactor.playerUsable)
            {
                _click = GetLeftClick();
            }
        }

        private void FixedUpdate()
        {
            if (!_interactor) return;

            if (_use)
            {
                _interactor.StartStopInteractions(false);
            }

            if (_click)
            {
                _interactor.StartStopInteractions(true);
            }

            _use = false;
            _click = false;
        }
    }
}
