using UnityEngine;

namespace razz
{
    [HelpURL("https://negengames.com/interactor/components.html#playercontrollercs")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeedMultiplier = 1f;
        public float animSpeedMultiplier = 1f;
        public float charScaleY = 1f;
        public bool debugHeight;

        private Rigidbody _playerRigidbody;
        private Animator _playerAnimator;
        private Transform _playerTransform;
        private Interactor _interactor;
        private float _defaultGroundCheckDistance;
        private const float _half = 0.5f;
        private float _turnAmount;
        private float _forwardAmount;
        private Vector3 _groundNormal;
        private float _capsuleHeight;
        private Vector3 _capsuleCenter;
        private CapsuleCollider _playerCapsuleCollider;
        private Ray _testHeightRay;
        private float _testHeightFloat;
        private bool _lockPlayer;

        public bool playerOnVehicle { get; set; }

        public bool playerCrouching { get; set; }
        public bool playerGrounded { get; set; }

        [SerializeField] private LayerMask m_raycastLayerMaskforRagdoll;
        [SerializeField] private float m_MovingTurnSpeed = 360;
        [SerializeField] private float m_StationaryTurnSpeed = 180;
        [SerializeField] private float m_JumpPower = 12f;
        [Range(1f, 4f)] [SerializeField] private float m_GravityMultiplier = 2f;
        [SerializeField] private float m_RunCycleLegOffset = 0.2f;
        [SerializeField] private float m_GroundCheckDistance = 0.1f;

        private void Start()
        {
            _interactor = GetComponent<Interactor>();
            if (!_interactor) return;

            _playerRigidbody = GetComponent<Rigidbody>();
            _playerTransform = transform;
            _playerAnimator = GetComponent<Animator>();
            _playerCapsuleCollider = GetComponent<CapsuleCollider>();
            _capsuleHeight = _playerCapsuleCollider.height;
            _capsuleCenter = _playerCapsuleCollider.center;

            _playerRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            _defaultGroundCheckDistance = m_GroundCheckDistance;
        }

        public void LockPlayer()
        {
            _lockPlayer = true;
        }
        public void UnlockPlayer()
        {
            _lockPlayer = false;
        }

        public void Move(Vector3 move, bool crouch, bool jump, bool use, bool clicked)
        {
            if (!_interactor) return;
            if (_lockPlayer) return;
            if (playerOnVehicle) return;

            if (move.magnitude > 1f)
                move.Normalize();
            move = transform.InverseTransformDirection(move);
            CheckGroundStatus();
            move = Vector3.ProjectOnPlane(move, _groundNormal);
            float slopeModifier = 2f - GetSlopeModifier(_groundNormal, 45f);
            move *= slopeModifier;

            _turnAmount = Mathf.Atan2(move.x, move.z);
            _forwardAmount = move.z;

            if (move.magnitude == 0 && !crouch && !jump && !use && playerGrounded && !clicked)
                _interactor.playerIdle = true;
            else _interactor.playerIdle = false;

            ApplyExtraTurnRotation();

            if (playerGrounded) HandleGroundedMovement(crouch, jump);
            else HandleAirborneMovement();

            ScaleCapsuleForCrouching(crouch);
            PreventStandingInLowHeadroom();

            UpdateAnimator(move);
        }

        private float GetSlopeModifier(Vector3 groundNormal, float maxSlopeAngle)
        {
            float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
            return 1f - Mathf.InverseLerp(1f, maxSlopeAngle, slopeAngle);
        }

        public void Dash()
        {
            if (!_playerRigidbody) return;

            _playerRigidbody.AddForce(150f, 400f, 0);
        }

        public void ControllerChange()
        {
            if (!_interactor) return;

            if (playerOnVehicle) ExitVehicle();
            else EnterVehicle();
        }

        public void EnterVehicle()
        {
            VehicleController _cc;
            VehicleBasicInput _vehicleinput;
            BikeController _bc;
            BikeBasicInput _bikeinput;
            Rigidbody _rb;
            GameObject enteredVehicle = _interactor.usableMultipleObject;

            if (_cc = enteredVehicle.GetComponent<VehicleController>())
            {
                _vehicleinput = enteredVehicle.GetComponent<VehicleBasicInput>();
                _rb = enteredVehicle.GetComponent<Rigidbody>();

                playerOnVehicle = true;
                _vehicleinput.enabled = true;
                _rb.isKinematic = false;
                _playerCapsuleCollider.enabled = false;
                _playerTransform.parent = _cc.sitPos;
                /*_playerTransform.position = _cc.sitPos.position;
                _playerTransform.rotation = _cc.sitPos.rotation;*/
                _playerRigidbody.isKinematic = true;
            }
            else if (_bc = enteredVehicle.GetComponent<BikeController>())
            {
                _bikeinput = enteredVehicle.GetComponent<BikeBasicInput>();

                playerOnVehicle = true;
                _bikeinput.enabled = true;
                _playerCapsuleCollider.enabled = false;
                _playerTransform.parent = _bc.sitPos;
                /*_playerTransform.position = _bc.sitPos.position;
                _playerTransform.rotation = _bc.sitPos.rotation;*/
                _playerRigidbody.isKinematic = true;
            }
        }

        public void ExitVehicle()
        {
            VehicleBasicInput _vehicleinput;
            BikeBasicInput _bikeinput;
            Rigidbody _rb;
            GameObject enteredVehicle = _interactor.usableMultipleObject;

            if (enteredVehicle.GetComponent<VehicleController>())
            {
                _vehicleinput = enteredVehicle.GetComponent<VehicleBasicInput>();
                _rb = enteredVehicle.GetComponent<Rigidbody>();

                playerOnVehicle = false;
                _vehicleinput.enabled = false;
                _rb.isKinematic = true;
                _playerTransform.parent = null;
                _playerCapsuleCollider.enabled = true;
                _playerRigidbody.isKinematic = false;
            }
            else if (enteredVehicle.GetComponent<BikeController>())
            {
                _bikeinput = enteredVehicle.GetComponent<BikeBasicInput>();

                playerOnVehicle = false;
                _bikeinput.enabled = false;
                _playerTransform.parent = null;
                /*_playerTransform.position += -_playerTransform.forward * 0.02f;*/
                _playerCapsuleCollider.enabled = true;
                _playerRigidbody.isKinematic = false;
            }

            _interactor.playerUsableMultiple = false;
        }

        private void ScaleCapsuleForCrouching(bool crouch)
        {
            if (playerGrounded && crouch)
            {
                if (playerCrouching) return;
                _playerCapsuleCollider.height = _playerCapsuleCollider.height / 2f;
                _playerCapsuleCollider.center = _playerCapsuleCollider.center / 2f;
                playerCrouching = true;
            }
            else
            {
                Ray crouchRay = new Ray(_playerRigidbody.position + Vector3.up * _playerCapsuleCollider.radius * _half, Vector3.up);
                float crouchRayLength = (_capsuleHeight * charScaleY) - _playerCapsuleCollider.radius * _half;

                if (Physics.SphereCast(crouchRay, _playerCapsuleCollider.radius * _half, crouchRayLength, m_raycastLayerMaskforRagdoll.value, QueryTriggerInteraction.Ignore))
                {
                    playerCrouching = true;
                    return;
                }
                _playerCapsuleCollider.height = _capsuleHeight;
                _playerCapsuleCollider.center = _capsuleCenter;
                playerCrouching = false;
            }
        }

        private void OnDrawGizmos()
        {
            if (debugHeight)
            {
                Gizmos.DrawRay(_testHeightRay.origin, Vector3.up * _testHeightFloat);
            }
        }

        private void PreventStandingInLowHeadroom()
        {
            if (!playerCrouching)
            {
                Ray crouchRay = new Ray(_playerRigidbody.position + Vector3.up * _playerCapsuleCollider.radius * _half, Vector3.up);
                float crouchRayLength = (_capsuleHeight * charScaleY) - _playerCapsuleCollider.radius * _half;

                _testHeightRay = crouchRay;
                _testHeightFloat = crouchRayLength;

                if (Physics.SphereCast(crouchRay, _playerCapsuleCollider.radius * _half, crouchRayLength, m_raycastLayerMaskforRagdoll.value, QueryTriggerInteraction.Ignore))
                {
                    playerCrouching = true;
                }
            }
        }

        private void UpdateAnimator(Vector3 move)
        {
            _playerAnimator.SetFloat("Forward", _forwardAmount, 0.1f, Time.deltaTime);
            _playerAnimator.SetFloat("Turn", _turnAmount, 0.1f, Time.deltaTime);
            _playerAnimator.SetBool("Crouch", playerCrouching);
            _playerAnimator.SetBool("OnGround", playerGrounded);
            if (!playerGrounded)
            {
#if UNITY_2023_3_OR_NEWER
                _playerAnimator.SetFloat("Jump", _playerRigidbody.linearVelocity.y);
#else
                _playerAnimator.SetFloat("Jump", _playerRigidbody.velocity.y);
#endif
            }

            float runCycle = Mathf.Repeat(
                    _playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
            float jumpLeg = (runCycle < _half ? 1 : -1) * _forwardAmount;

            if (playerGrounded) _playerAnimator.SetFloat("JumpLeg", jumpLeg);

            if (playerGrounded && move.magnitude > 0)
                _playerAnimator.speed = animSpeedMultiplier;
            else
                _playerAnimator.speed = 1;
        }

        private void HandleAirborneMovement()
        {
            Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
            _playerRigidbody.AddForce(extraGravityForce);
#if UNITY_2023_3_OR_NEWER
            m_GroundCheckDistance = _playerRigidbody.linearVelocity.y < 0 ? _defaultGroundCheckDistance : 0.01f;
#else
            m_GroundCheckDistance = _playerRigidbody.velocity.y < 0 ? _defaultGroundCheckDistance : 0.01f;
#endif
        }

        private void HandleGroundedMovement(bool crouch, bool jump)
        {
            if (jump && !crouch && _playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
            {
#if UNITY_2023_3_OR_NEWER
                _playerRigidbody.linearVelocity = new Vector3(_playerRigidbody.linearVelocity.x, m_JumpPower, _playerRigidbody.linearVelocity.z);
#else
                _playerRigidbody.velocity = new Vector3(_playerRigidbody.velocity.x, m_JumpPower, _playerRigidbody.velocity.z);
#endif

                playerGrounded = false;
                _playerAnimator.applyRootMotion = false;
                m_GroundCheckDistance = 0.1f;
            }
        }

        private void ApplyExtraTurnRotation()
        {
            float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, _forwardAmount);
            transform.Rotate(0, _turnAmount * turnSpeed * Time.deltaTime, 0);
        }

        public void OnAnimatorMove()
        {
            if (!_interactor) return;
            if (_playerRigidbody.isKinematic && _interactor.usableMultipleObject) return;

            if (playerGrounded && Time.deltaTime > 0)
            {
                Vector3 moveForward = transform.forward * _playerAnimator.GetFloat("motionZ") * Time.deltaTime;
                Vector3 v = ((_playerAnimator.deltaPosition + moveForward) * moveSpeedMultiplier) / Time.deltaTime;
#if UNITY_2023_3_OR_NEWER
                v.y = _playerRigidbody.linearVelocity.y;
                _playerRigidbody.linearVelocity = v;
#else
                v.y = _playerRigidbody.velocity.y;
                _playerRigidbody.velocity = v;
#endif
            }
        }

        private void CheckGroundStatus()
        {
            RaycastHit hitInfo;

            if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
            {
                _groundNormal = hitInfo.normal;
                playerGrounded = true;
                _playerAnimator.applyRootMotion = true;
            }
            else
            {
                playerGrounded = false;
                _groundNormal = Vector3.up;
                _playerAnimator.applyRootMotion = false;
            }
        }
    }
}
