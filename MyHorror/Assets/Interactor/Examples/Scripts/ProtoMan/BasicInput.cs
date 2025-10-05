using UnityEngine;

namespace razz
{
    [HelpURL("https://negengames.com/interactor/components.html#basicinputcs")]
    [DisallowMultipleComponent]
    public class BasicInput : MonoBehaviour
    {
        public KeyCode useKey = KeyCode.E;
        public bool updateOnFixedUpdate = true;

        private PlayerController _playerController;
        private Interactor _interactor;
        private Transform _camTransform;
        private Vector3 _camForward;
        private Vector3 _move;
        private bool _jump;
        private bool _crouch;
        private InventoryRenderer _invRend;

        [HideInInspector] public bool use;
        [HideInInspector] public bool usable;
        [HideInInspector] public bool changable;
        [HideInInspector] public bool click;

        private void Start()
        {
            if (Camera.main != null)
            {
                _camTransform = Camera.main.transform;
            }
            _playerController = GetComponent<PlayerController>();
            _interactor = GetComponent<Interactor>();
        }

        public void SetUse()
        {//Animation events can use
            use = true;
        }

        private void Update()
        {
            if (!_interactor) return;

            if (InteractorInput.GetKeyDown(KeyCode.Tab))
            {
#if UNITY_2023_1_OR_NEWER
                if (!_invRend) _invRend = FindFirstObjectByType<InventoryRenderer>();
#else
                if (!_invRend) _invRend = FindObjectOfType<InventoryRenderer>();
#endif
                if (_invRend)
                {
                    if (_invRend.invOpen) _invRend.HideInventory();
                    else _invRend.ShowInventory();
                }
            }

            _crouch = InteractorInput.GetKey(KeyCode.C);

            if (!_jump)
            {
                _jump = InteractorInput.GetKeyDown(KeyCode.Space);
            }

            if (!use)
            {
                use = InteractorInput.GetKeyDown(useKey);
            }

            if (!click && _interactor.playerUsable)
            {
                click = InteractorInput.GetLeftClick();
            }

            //Press P to stop all interactions
            if (InteractorInput.GetKeyDown(KeyCode.P))
            {
                _interactor.DisconnectAll();
                Debug.Log("Stopped all interactions.", _interactor);
            }

            //Press Y to move player upwards if its stuck
            if (InteractorInput.GetKeyDown(KeyCode.Y))
            {
                GetComponent<PlayerController>().Dash();
            }

            if (!updateOnFixedUpdate) UpdateMovement();
        }

        private void FixedUpdate()
        {
            if (updateOnFixedUpdate) UpdateMovement();
        }

        private void UpdateMovement()
        {
            if (!_interactor) return;

            float h = InteractorInput.GetHorizontal();
            float v = InteractorInput.GetVertical();

            if (_interactor.playerPushing)
            {
                h = 0;
                v = Mathf.Clamp01(v);
            }

            if (_camTransform != null)
            {
                _camForward = Vector3.Scale(_camTransform.forward, new Vector3(1, 0, 1)).normalized;
                _move = (v * _camForward + h * _camTransform.right).normalized * Mathf.Max(Mathf.Abs(h), Mathf.Abs(v));
            }
            else
            {
                _move = (v * Vector3.forward + h * Vector3.right).normalized * Mathf.Max(Mathf.Abs(h), Mathf.Abs(v));
            }

            if (InteractorAiInput.GetKey(KeyCode.LeftShift))
            {
                _move *= 2f;
            }
            else if (_interactor.playerPushing)
            {
                _move *= 0.4f;
            }
            else
            {
                _move *= 0.5f;
            }

            if (use)
            {
                _interactor.StartStopInteractions(false);
            }

            if (click)
            {
                _interactor.StartStopInteractions(true);
            }

            _playerController.Move(_move, _crouch, _jump, use, click);
            _jump = false;
            _crouch = false;
            use = false;
            click = false;
        }
    }
}
