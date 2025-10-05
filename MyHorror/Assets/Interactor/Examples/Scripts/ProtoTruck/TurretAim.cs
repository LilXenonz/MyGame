using UnityEngine;
using System.Collections;

namespace razz
{
    //Handler class for Turret attack. Targeting both turrets and lights with different timings,
    //firing, playing sound and particle effects.
    public class TurretAim : MonoBehaviour
    {
        private Animation _animation;
        private Transform _target;
        private bool _available = true;
        private AudioSource _audio;

        public Transform turretGun;
        public Transform turretLightBase;
        public Transform turretLight;
        public ParticleSystem particles;
        public bool locked = false;
        public Interactor.FullBodyBipedEffector effector;
        public float force = 1f;

        private void Start()
        {
            if (!(_animation = GetComponent<Animation>()))
            {
                Debug.Log("No animator component on Turret: " + this.name);
            }

            if (turretGun == null || turretLightBase == null || turretLight == null)
            {
                Debug.Log("Turret Aim gameobject or gameobjects are not assigned: " + this.name);
            }

            _audio = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (locked)
            {
                _animation.Play();
                if (_audio != null)
                {
                    _audio.Play();
                }
                if (particles != null)
                {
                    particles.Play();
                }
                PushTarget();
                locked = false;
                _available = true;
            }
        }

        public void Attack(Transform target)
        {
            if (!_available) return;

            _available = false;
            _target = target;
            Vector3 direction = _target.position - turretGun.position;
            Quaternion look = Quaternion.LookRotation(direction, turretGun.up);
            
            LightAim(look);
            Fire(look);
        }

        public void Attack(InteractorObject intObj)
        {
            if (!_available) return;

            InteractorTarget target = intObj.currentInteractor.GetCurrentInteractorTarget(Interactor.FullBodyBipedEffector.Body);
            if (!target) return;

            _available = false;
            _target = target.transform;
            Vector3 direction = _target.position - turretGun.position;
            Quaternion look = Quaternion.LookRotation(direction, turretGun.up);

            LightAim(look);
            Fire(look);
        }

        private void PushTarget()
        {
            Rigidbody targetRigidbody = _target.GetComponent<Rigidbody>();
            if (!targetRigidbody) return;

            targetRigidbody.AddForce((_target.transform.position - turretGun.position) * force, ForceMode.Impulse);
            Debug.DrawLine(_target.transform.position, turretGun.position, Color.red, 3f);
        }

        public void Fire(Quaternion look)
        {
            StartCoroutine(RotateToGlobal(turretGun, look, 1f, Ease.QuadIn));
        }

        public IEnumerator RotateToGlobal(Transform transform, Quaternion target, float duration, Easer ease)
        {
            float elapsed = 0;
            var start = transform.rotation;

            while (elapsed < duration)
            {
                elapsed = Mathf.MoveTowards(elapsed, duration, Time.deltaTime);
                transform.rotation = Quaternion.Lerp(start, target, ease(elapsed / duration));
                yield return 0;
            }

            locked = true;
            transform.rotation = target;
        }

        public void LightAim(Quaternion look)
        {
            StartCoroutine(turretLightBase.RotateToGlobal(look, 0.5f, Ease.CubeIn));
        }
    }
}
