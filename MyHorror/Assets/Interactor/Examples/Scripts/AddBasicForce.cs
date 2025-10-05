using UnityEngine;
using System.Collections;

namespace razz
{
    public class AddBasicForce : MonoBehaviour
    {
        public Vector3 rotation = new Vector3(0, 0, 0);
        public Vector3 force = new Vector3(0, 1000f, 0);
        public float duration = 0.8f;
        public ForceMode forceMode;
        public EaseType easeType;

        private Rigidbody _rb;
        private Vector3 _startPos;
        private Quaternion _startRot;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _startPos = transform.position;
            _startRot = transform.rotation;
        }

        public void ClickForce()
        {
            if (_rb)
            {
                bool kinematic = _rb.isKinematic;
                _rb.isKinematic = false;

                if (rotation != Vector3.zero)
                {
                    _rb.AddTorque(rotation, forceMode);
                }
                if (force != Vector3.zero)
                {
                    _rb.AddForce(transform.localRotation * force, forceMode);
                }

                _rb.isKinematic = kinematic;
            }
            else
            {
                if (rotation != Vector3.zero)
                    StartCoroutine(transform.RotateTo(Quaternion.Euler(rotation), duration, Ease.FromType(easeType)));
                if (force != Vector3.zero)
                    StartCoroutine(transform.MoveTo(force, duration, Ease.FromType(easeType)));
            }
        }

        IEnumerator ApplyTorqueAndReset(Vector3 torque, ForceMode mode, float delay = 0.1f)
        {
            bool kinematic = _rb.isKinematic;
            _rb.isKinematic = false;
            _rb.AddTorque(torque, mode);
            yield return new WaitForSeconds(delay);
            _rb.isKinematic = kinematic;
        }

        public void ClickForce(Transform direction)
        {
            Quaternion rot = Quaternion.identity;
            if (direction)
            {
                rot = Quaternion.LookRotation(transform.position - direction.position);
            }

            if (_rb)
            {
                _rb.AddForce(rot * force, forceMode);
            }
            else
            {
                if (rotation != Vector3.zero)
                    StartCoroutine(transform.RotateTo(Quaternion.Euler(rotation), duration, Ease.FromType(easeType)));
                if (force != Vector3.zero)
                    StartCoroutine(transform.MoveTo(rot * force, duration, Ease.FromType(easeType)));
            }
        }

        public void SetForceZ(float forceZ)
        {
            force.z = forceZ;
        }

        public void ResetToStart()
        {
            transform.position = _startPos;
            transform.rotation = _startRot;
        }
    }
}
