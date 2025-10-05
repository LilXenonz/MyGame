using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace razz
{
    [HelpURL("https://negengames.com/interactor/components.html#hitreactioncs")]
    public class HitReaction : MonoBehaviour
    {
        [System.Serializable]
        public class HitReactionSettings
        {
            [Tooltip("Enable hit reactions for this body bone")]
            public bool enabled = true;
            [Tooltip("Maximum pull strength applied to the bone (0-1) to apply its parent bones.")]
            public float maxPull = 1.0f;
            [Tooltip("Maximum distance the body part can be displaced in meters")]
            public float maxDistance = 0.3f;
            [Tooltip("Duration in seconds for the initial pull/displacement phase")]
            public float pullDuration = 0.1f;
            [Tooltip("Duration in seconds for the return/release phase")]
            public float releaseDuration = 0.2f;
        }

        [Tooltip("FullBodyIKBehaviour component is required on the same gameobject to enable hit reactions.")]
        public FullBodyIKBehaviour fullBodyIKBehaviour;

        [Header("Hit Reaction Settings")]
        public HitReactionSettings hipsSettings = new HitReactionSettings();
        public HitReactionSettings neckSettings = new HitReactionSettings();
        public HitReactionSettings headSettings = new HitReactionSettings();
        public HitReactionSettings leftKneeSettings = new HitReactionSettings { enabled = false };
        public HitReactionSettings rightKneeSettings = new HitReactionSettings { enabled = false };
        public HitReactionSettings leftFootSettings = new HitReactionSettings { enabled = false };
        public HitReactionSettings rightFootSettings = new HitReactionSettings { enabled = false };
        public HitReactionSettings leftArmSettings = new HitReactionSettings();
        public HitReactionSettings rightArmSettings = new HitReactionSettings();
        public HitReactionSettings leftElbowSettings = new HitReactionSettings();
        public HitReactionSettings rightElbowSettings = new HitReactionSettings();
        public HitReactionSettings leftWristSettings = new HitReactionSettings();
        public HitReactionSettings rightWristSettings = new HitReactionSettings();

        [Header("Global Settings")]
        [Tooltip("Global multiplier to increase or decrease hit reaction for all bones")]
        public float globalForceMultiplier = 1f;
        [Tooltip("Enable debug mode to show hit reaction information in console and direction in sceneview")]
        public bool debugMode = false;

        private Dictionary<FullBodyIK.Effector, Coroutine> activeReactions = new Dictionary<FullBodyIK.Effector, Coroutine>();
        private List<EffectorInfo> effectors = new List<EffectorInfo>();

        private struct EffectorInfo
        {
            public FullBodyIK.Effector effector;
            public FullBodyIK.Bone bone;
            public HitReactionSettings settings;
            public string name;
            public bool isArmOrHips;
        }

        void Start()
        {
            if (fullBodyIKBehaviour == null)
                fullBodyIKBehaviour = GetComponent<FullBodyIKBehaviour>();

            if (fullBodyIKBehaviour == null)
            {
                Debug.LogWarning("HitReaction requires FullBodyIKBehaviour component");
                enabled = false;
                return;
            }

            StartCoroutine(InitializeEffectors());
        }

        IEnumerator InitializeEffectors()
        {
            while (fullBodyIKBehaviour.fullBodyIK == null)
                yield return null;

            FullBodyIK fbik = fullBodyIKBehaviour.fullBodyIK;

            InitializeEffector(fbik.bodyEffectors.hips, fbik.bodyEffectors.hips.bone, "Hips", hipsSettings, true);
            InitializeEffector(fbik.headEffectors.neck, fbik.headEffectors.neck.bone, "Neck", neckSettings);
            InitializeEffector(fbik.headEffectors.head, fbik.headEffectors.head.bone, "Head", headSettings);
            InitializeEffector(fbik.leftLegEffectors.knee, fbik.leftLegEffectors.knee.bone, "LeftKnee", leftKneeSettings);
            InitializeEffector(fbik.rightLegEffectors.knee, fbik.rightLegEffectors.knee.bone, "RightKnee", rightKneeSettings);
            InitializeEffector(fbik.leftLegEffectors.foot, fbik.leftLegEffectors.foot.bone, "LeftFoot", leftFootSettings);
            InitializeEffector(fbik.rightLegEffectors.foot, fbik.rightLegEffectors.foot.bone, "RightFoot", rightFootSettings);
            InitializeEffector(fbik.leftArmEffectors.arm, fbik.leftArmEffectors.arm.bone, "LeftArm", leftArmSettings, true);
            InitializeEffector(fbik.rightArmEffectors.arm, fbik.rightArmEffectors.arm.bone, "RightArm", rightArmSettings, true);
            InitializeEffector(fbik.leftArmEffectors.elbow, fbik.leftArmEffectors.elbow.bone, "LeftElbow", leftElbowSettings);
            InitializeEffector(fbik.rightArmEffectors.elbow, fbik.rightArmEffectors.elbow.bone, "RightElbow", rightElbowSettings);
            InitializeEffector(fbik.leftArmEffectors.wrist, fbik.leftArmEffectors.wrist.bone, "LeftWrist", leftWristSettings);
            InitializeEffector(fbik.rightArmEffectors.wrist, fbik.rightArmEffectors.wrist.bone, "RightWrist", rightWristSettings);

            if (debugMode)
                Debug.Log($"HitReaction initialized with {effectors.Count} effectors");
        }

        void InitializeEffector(FullBodyIK.Effector effector, FullBodyIK.Bone bone, string name, HitReactionSettings settings, bool isArmOrHips = false)
        {
            if (effector == null || bone == null || !settings.enabled)
                return;

            effector.positionEnabled = true;

            if (isArmOrHips)
                effector.pull = 0f;

            if (effector.positionWeight <= 0.01f)
                effector.positionWeight = 0f;

            effectors.Add(new EffectorInfo
            {
                effector = effector,
                bone = bone,
                settings = settings,
                name = name,
                isArmOrHips = isArmOrHips
            });

            if (debugMode)
                Debug.Log($"Added effector: {name}, ArmOrHips: {isArmOrHips}");
        }

        public void Hit(Vector3 hitPoint, Vector3 hitDirection, float hitForce)
        {
            EffectorInfo closestEffector = FindClosestEffector(hitPoint);
            if (closestEffector.effector == null)
                return;

            ApplyHitReaction(closestEffector, hitPoint, hitDirection, hitForce);
        }

        public void HitOnBone(string boneName, Vector3 hitDirection, float hitForce)
        {
            foreach (EffectorInfo info in effectors)
            {
                if (info.name.Equals(boneName, System.StringComparison.OrdinalIgnoreCase))
                {
                    Vector3 bonePosition = info.bone.worldPosition;
                    ApplyHitReaction(info, bonePosition, hitDirection, hitForce);
                    return;
                }
            }

            if (debugMode)
                Debug.LogWarning($"Bone '{boneName}' not found for hit reaction");
        }

        EffectorInfo FindClosestEffector(Vector3 hitPoint)
        {
            float minDistance = float.MaxValue;
            EffectorInfo closest = new EffectorInfo();

            foreach (EffectorInfo info in effectors)
            {
                if (info.bone == null || !info.bone.transformIsAlive)
                    continue;

                Vector3 bonePos = info.bone.worldPosition;
                float distance = Vector3.Distance(hitPoint, bonePos);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = info;
                }
            }

            if (debugMode && closest.effector != null)
                Debug.Log($"Hit reaction: closest bone is {closest.name} at distance {minDistance}");

            return closest;
        }

        void ApplyHitReaction(EffectorInfo info, Vector3 hitPoint, Vector3 hitDirection, float hitForce)
        {
            if (activeReactions.ContainsKey(info.effector))
            {
                StopCoroutine(activeReactions[info.effector]);
                ResetEffector(info.effector);
            }

            Coroutine newReaction = StartCoroutine(HitReactionCoroutine(info, hitPoint, hitDirection, hitForce));
            activeReactions[info.effector] = newReaction;

            if (debugMode)
            {
                Debug.DrawLine(hitPoint, hitPoint + hitDirection.normalized * 0.5f, Color.red, 1.0f);
                Debug.Log($"Applied hit reaction to {info.name} with force {hitForce}");
            }
        }

        void ResetEffector(FullBodyIK.Effector effector)
        {
            foreach (var info in effectors)
            {
                if (info.effector == effector)
                {
                    effector.pull = info.isArmOrHips ? 0f : effector.pull;
                    effector.positionWeight = 0f;
                    break;
                }
            }
        }

        IEnumerator HitReactionCoroutine(EffectorInfo info, Vector3 hitPoint, Vector3 hitDirection, float hitForce)
        {
            FullBodyIK.Effector effector = info.effector;
            HitReactionSettings settings = info.settings;

            Vector3 originalPosition = effector.transform.position;
            float originalPull = effector.pull;
            float originalWeight = effector.positionWeight;

            if (info.isArmOrHips)
                originalPull = 0f;

            Vector3 currentBonePos;
            if (effector.positionWeight > 0.01f)
                currentBonePos = Vector3.Lerp(info.bone.worldPosition, effector.worldPosition, effector.positionWeight);
            else
                currentBonePos = info.bone.worldPosition;

            Vector3 reactionDir = hitDirection.normalized;
            float reactionDistance = Mathf.Min(hitForce * globalForceMultiplier * 0.1f, settings.maxDistance);
            Vector3 targetPosition = currentBonePos + reactionDir * reactionDistance;

            float timer = 0f;
            while (timer < settings.pullDuration)
            {
                float t = timer / settings.pullDuration;
                float easedT = Mathf.SmoothStep(0, 1, t);

                effector.transform.position = Vector3.Lerp(currentBonePos, targetPosition, easedT);
                effector.pull = Mathf.Lerp(originalPull, settings.maxPull, easedT);
                effector.positionWeight = Mathf.Lerp(originalWeight, 1.0f, easedT);

                timer += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(0.05f);

            timer = 0f;
            while (timer < settings.releaseDuration)
            {
                float t = timer / settings.releaseDuration;
                float easedT = Mathf.SmoothStep(0, 1, t);

                effector.transform.position = Vector3.Lerp(targetPosition, originalPosition, easedT);
                effector.pull = Mathf.Lerp(settings.maxPull, originalPull, easedT);
                effector.positionWeight = Mathf.Lerp(1.0f, originalWeight, easedT);

                timer += Time.deltaTime;
                yield return null;
            }

            effector.transform.position = originalPosition;
            effector.pull = originalPull;
            effector.positionWeight = originalWeight;

            if (info.isArmOrHips)
                effector.pull = 0f;

            if (originalWeight <= 0.01f)
                effector.positionWeight = 0f;

            activeReactions.Remove(effector);

            if (debugMode)
                Debug.Log($"Finished hit reaction for {info.name}. Pull: {effector.pull}, Weight: {effector.positionWeight}");
        }

        void OnDisable()
        {
            foreach (var kvp in activeReactions)
            {
                if (kvp.Value != null)
                    StopCoroutine(kvp.Value);

                FullBodyIK.Effector effector = kvp.Key;
                if (effector != null)
                {
                    effector.pull = 0f;
                    effector.positionWeight = 0f;
                }
            }

            foreach (var info in effectors)
            {
                if (info.isArmOrHips && info.effector != null)
                    info.effector.pull = 0f;

                if (info.effector != null)
                    info.effector.positionWeight = 0f;
            }

            activeReactions.Clear();
        }
    }
}
