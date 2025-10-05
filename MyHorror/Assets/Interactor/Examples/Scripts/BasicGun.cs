using UnityEngine;

namespace razz
{
    public class BasicGun : MonoBehaviour
    {
        [Tooltip("Transform to shoot from towards its forward. If not assigned, uses this object's transform.")]
        public Transform gunTransform;
        [Tooltip("Maximum shooting distance in meters.")]
        public float maxRange = 20f;
        [Tooltip("Which layers the gun can hit.")]
        public LayerMask hitLayers = -1;
        [Tooltip("Color of the laser sight line.")]
        public Color laserColor = Color.red;
        [Tooltip("Key to press for shooting.")]
        public KeyCode shootKey = KeyCode.Mouse0;
        [Tooltip("Force applied to hit objects with InteractorIK component.")]
        public float shootForce = 10f;
        [Tooltip("Sound played when shooting.")]
        public AudioClip shootSound;
        [Tooltip("Speed of gun rotation when using H/B/N/M keys.")]
        public float rotationSpeed = 20f;

        private LineRenderer lineRenderer;
        private AudioSource audioSource;
        private RaycastHit lastGunHit;
        private bool hasValidTarget;

        private void Start()
        {
            if (gunTransform == null)
                gunTransform = transform;

            SetupLineRenderer();
            SetupAudioSource();
        }

        private void SetupLineRenderer()
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.02f;
            lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
            lineRenderer.material.color = laserColor;
            lineRenderer.positionCount = 2;
        }

        private void SetupAudioSource()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        private void Update()
        {
            HandleRotation();
            UpdateGun();

            if (InteractorInput.GetKeyDown(shootKey))
                Shoot();
        }

        private void HandleRotation()
        {
            float rotAmount = rotationSpeed * Time.deltaTime;

            if (InteractorInput.GetKey(KeyCode.H))
                gunTransform.Rotate(-rotAmount, 0, 0);
            if (InteractorInput.GetKey(KeyCode.J))
                gunTransform.Rotate(0, -rotAmount, 0);
            if (InteractorInput.GetKey(KeyCode.B))
                gunTransform.Rotate(rotAmount, 0, 0);
            if (InteractorInput.GetKey(KeyCode.K))
                gunTransform.Rotate(0, rotAmount, 0);
        }

        private void UpdateGun()
        {
            Vector3 origin = gunTransform.position;
            Vector3 direction = gunTransform.forward;
            lineRenderer.SetPosition(0, origin);

            if (Physics.Raycast(origin, direction, out RaycastHit hit, maxRange, hitLayers, QueryTriggerInteraction.Ignore))
            {
                lastGunHit = hit;
                hasValidTarget = true;
                lineRenderer.SetPosition(1, hit.point);
            }
            else
            {
                hasValidTarget = false;
                lineRenderer.SetPosition(1, origin + direction * maxRange);
            }
        }

        public void Shoot()
        {
            if (shootSound != null && audioSource != null)
                audioSource.PlayOneShot(shootSound);

            if (!hasValidTarget)
                return;

            InteractorIK interactorIK = lastGunHit.collider.GetComponentInParent<InteractorIK>();
            if (interactorIK != null)
                interactorIK.TriggerHitReaction(lastGunHit.collider, lastGunHit.point, gunTransform.forward, shootForce);

            //Or you can get HitReaction component and call 
            //hitReaction.Hit(hitPoint, hitDirection, hitForce);
            //instead. This script uses interactorIK.TriggerHitReaction()
            //because it can be compatible with Final IK reaction calls.
            //And it doesn't require collider. (It is for Final IK too)
        }
    }
}
