using UnityEngine;
using System.Collections.Generic;

namespace razz
{
    [System.Serializable]
    public class HitConfig
    {
        public string hitName;
        public KeyCode hitKey;
        public Transform targetTransform;
        public Interactor.FullBodyBipedEffector effector;
        public Color hitColor;

        public float hitDuration;
        public float returnDuration;
        public float returnSpeed;
        public float hitForce = 1.0f;

        public Vector3 startPosition;
        public Vector3 startRotation;
        public Vector3 endPosition;
        public Vector3 endRotation;

        public Vector3 startControlPoint;
        public Vector3 endControlPoint;

        public Vector3 alternateHitPosition;

        public UnityEngine.Events.UnityEvent onHitEnd;

        public EaseType positionEaseType;
        public AnimationCurve animationCurve;
        public EaseType rotationEaseType;
        public EaseType returnEaseType = EaseType.QuadInOut;
    }

    public enum HitState
    {
        Idle,
        MovingToStart,
        Hitting,
        Returning
    }

    [System.Serializable]
    public class HitAnimationData
    {
        public HitConfig hitConfig;
        public HitState state;
        public float elapsed;
        public Vector3 startPos;
        public Quaternion startRot;
        public Vector3 targetPos;
        public Quaternion targetRot;
        public bool eventTriggered;
        public bool isInterrupted;
        public bool useOverrideStart;
    }

    [HelpURL("https://negengames.com/interactor/components.html#hitcontrollercs")]
    public class HitController : MonoBehaviour
    {
        public bool hitsEnabled = true;
        public bool returnInitialPosition = false;
        [HideInInspector] public bool showGizmos = true;
        [HideInInspector] public float handlesSize = 1.0f;

        [SerializeField] private List<HitConfig> hits = new List<HitConfig>();

        public Interactor interactor;
        public Interactor targetInteractor;
        public bool checkInteractorRules = false;
        public Transform hitPivot;
        public bool hitPivotYOnly = false;
        public bool triggerOnInterruptedHits = false;
        public bool smoothInterrupt = false;

        private Dictionary<Transform, HitAnimationData> activeAnimations = new Dictionary<Transform, HitAnimationData>();
        private Dictionary<Transform, (Vector3 localPosition, Quaternion localRotation)> initialTransforms = new Dictionary<Transform, (Vector3, Quaternion)>();
        private Dictionary<Transform, (Vector3 relativePosition, Quaternion relativeRotation)> initialInteractorRelatives = new Dictionary<Transform, (Vector3, Quaternion)>();
        private Vector3 interactorSnapshotPosition;
        private Quaternion interactorSnapshotRotation;
        private HitAnimSwitch hitAnim;

        private void Start()
        {
            hitAnim = GetComponent<HitAnimSwitch>();
        }

        private void Update()
        {
            if (!hitsEnabled) return;

            for (int i = 0; i < hits.Count; i++)
            {
                if (InteractorInput.GetKeyDown(hits[i].hitKey))
                {
                    if (hits[i]?.targetTransform != null)
                    {
                        if (checkInteractorRules && interactor)
                        {
                            GameObject tempObject = new GameObject();
                            Transform tempTranform = tempObject.transform;
                            tempTranform.position = GetWorldPosition(hits[i].startPosition);
                            if (hitPivot)
                            {
                                Vector3 directionToInteractor = (interactor.transform.position - hitPivot.position).normalized;
                                if (hitPivotYOnly)
                                {
                                    Vector3 flatDirection = new Vector3(directionToInteractor.x, 0f, directionToInteractor.z).normalized;
                                    hitPivot.rotation = Quaternion.LookRotation(flatDirection);
                                }
                                else
                                {
                                    hitPivot.rotation = Quaternion.LookRotation(directionToInteractor);
                                }
                            }

                            if (interactor.EffectorCheckOrbitalPosition(tempTranform, (int)hits[i].effector))
                            {
                                StartHitAnimation(hits[i]);
                            }
                            Destroy(tempObject);
                        }
                        else StartHitAnimation(hits[i]);
                    }
                    else Debug.LogWarning("Hit " + i.ToString() + "'s Target Transform is null!", this);
                }
            }
        }

        private void FixedUpdate()
        {
            if (!hitsEnabled) return;

            var transforms = new List<Transform>(activeAnimations.Keys);

            foreach (var transform in transforms)
            {
                UpdateHitAnimation(transform);
            }
        }

        private void UpdateHitAnimation(Transform target)
        {
            if (!activeAnimations.TryGetValue(target, out HitAnimationData animData))
                return;

            switch (animData.state)
            {
                case HitState.MovingToStart:
                    UpdateMovingToStart(target, animData);
                    break;
                case HitState.Hitting:
                    UpdateHitting(target, animData);
                    break;
                case HitState.Returning:
                    UpdateReturning(target, animData);
                    break;
            }
        }

        private void UpdateMovingToStart(Transform target, HitAnimationData animData)
        {
            Vector3 worldStartPos = GetInteractorAdjustedPosition(animData.hitConfig.startPosition);
            Quaternion worldStartRot = GetInteractorAdjustedRotation(animData.hitConfig.startRotation);

            float journeyLength = Vector3.Distance(animData.startPos, worldStartPos);
            if (journeyLength <= 0.001f)
            {
                animData.state = HitState.Hitting;
                animData.elapsed = 0f;
                return;
            }

            float distanceCovered = animData.elapsed * animData.hitConfig.returnSpeed;
            float t = Mathf.Clamp01(distanceCovered / journeyLength);

            target.position = Vector3.Lerp(animData.startPos, worldStartPos, Ease.FromType(animData.hitConfig.returnEaseType)(t));
            target.rotation = Quaternion.Lerp(animData.startRot, worldStartRot, Ease.FromType(animData.hitConfig.returnEaseType)(t));

            animData.elapsed += Time.fixedDeltaTime;

            if (t >= 1f)
            {
                animData.state = HitState.Hitting;
                animData.elapsed = 0f;
            }
        }

        private void UpdateHitting(Transform target, HitAnimationData animData)
        {
            float t = animData.elapsed / animData.hitConfig.hitDuration;
            float positionT = Ease.FromType(animData.hitConfig.positionEaseType)(t, animData.hitConfig.animationCurve);
            float rotationT = Ease.FromType(animData.hitConfig.rotationEaseType)(t);

            Vector3 worldStartPos;
            Quaternion worldStartRot;
            Vector3 worldStartControl;

            if (animData.useOverrideStart)
            {
                worldStartPos = animData.startPos;
                worldStartRot = animData.startRot;
                worldStartControl = worldStartPos;
            }
            else
            {
                worldStartPos = GetInteractorAdjustedPosition(animData.hitConfig.startPosition);
                worldStartRot = GetInteractorAdjustedRotation(animData.hitConfig.startRotation);
                worldStartControl = GetInteractorAdjustedControlPoint(animData.hitConfig.startPosition, animData.hitConfig.startControlPoint);
            }

            Vector3 worldEndPos = GetInteractorAdjustedPosition(animData.hitConfig.endPosition);
            Vector3 worldEndControl = GetInteractorAdjustedControlPoint(animData.hitConfig.endPosition, animData.hitConfig.endControlPoint);
            Quaternion worldEndRot = GetInteractorAdjustedRotation(animData.hitConfig.endRotation);

            target.position = EvaluateBezier(worldStartPos, worldStartControl, worldEndControl, worldEndPos, positionT);
            target.rotation = Quaternion.Lerp(worldStartRot, worldEndRot, rotationT);

            animData.elapsed += Time.fixedDeltaTime;

            if (animData.elapsed >= animData.hitConfig.hitDuration)
            {
                if (!animData.eventTriggered)
                {
                    animData.hitConfig.onHitEnd?.Invoke();
                    TriggerHitReactionForHit(animData.hitConfig);
                    animData.eventTriggered = true;
                }

                animData.useOverrideStart = false; // Reset flag before changing state

                animData.state = HitState.Returning;
                animData.elapsed = 0f;
                animData.startPos = target.position;
                animData.startRot = target.rotation;

                if (returnInitialPosition && initialTransforms.TryGetValue(target, out var initial))
                {
                    animData.targetPos = GetInteractorAdjustedInitialPosition(target);
                    animData.targetRot = GetInteractorAdjustedInitialRotation(target);
                }
                else
                {
                    animData.targetPos = GetInteractorAdjustedPosition(animData.hitConfig.startPosition);
                    animData.targetRot = GetInteractorAdjustedRotation(animData.hitConfig.startRotation);
                }
            }
        }

        private void UpdateReturning(Transform target, HitAnimationData animData)
        {
            float t = animData.elapsed / animData.hitConfig.returnDuration;
            float positionT = Ease.FromType(animData.hitConfig.returnEaseType)(t);
            float rotationT = Ease.FromType(animData.hitConfig.returnEaseType)(t);

            if (returnInitialPosition && initialTransforms.ContainsKey(target))
            {
                Vector3 currentReturnTarget = GetInteractorAdjustedInitialPosition(target);
                Quaternion currentReturnTargetRot = GetInteractorAdjustedInitialRotation(target);

                target.position = Vector3.Lerp(animData.startPos, currentReturnTarget, positionT);
                target.rotation = Quaternion.Lerp(animData.startRot, currentReturnTargetRot, rotationT);
            }
            else
            {
                Vector3 adjustedEndControl = GetInteractorAdjustedControlPoint(animData.hitConfig.endPosition, animData.hitConfig.endControlPoint);
                Vector3 adjustedStartControl = GetInteractorAdjustedControlPoint(animData.hitConfig.startPosition, animData.hitConfig.startControlPoint);

                target.position = EvaluateBezier(
                    animData.startPos,
                    animData.startPos + (adjustedEndControl - GetInteractorAdjustedPosition(animData.hitConfig.endPosition)),
                    animData.targetPos + (adjustedStartControl - GetInteractorAdjustedPosition(animData.hitConfig.startPosition)),
                    animData.targetPos,
                    positionT
                );
                target.rotation = Quaternion.Lerp(animData.startRot, animData.targetRot, rotationT);
            }

            animData.elapsed += Time.fixedDeltaTime;

            if (animData.elapsed >= animData.hitConfig.returnDuration)
            {
                if (returnInitialPosition && initialTransforms.TryGetValue(target, out var finalInitial))
                {
                    target.localPosition = finalInitial.localPosition;
                    target.localRotation = finalInitial.localRotation;
                }
                else
                {
                    target.position = GetInteractorAdjustedPosition(animData.hitConfig.startPosition);
                    target.rotation = GetInteractorAdjustedRotation(animData.hitConfig.startRotation);
                }

                activeAnimations.Remove(target);
            }
        }

        public void EnableHits()
        {
            hitsEnabled = true;
            if (hitAnim && hitAnim.enabled)
            {
                hitAnim.StartHitAnimation();
            }
        }

        public void DisableHits()
        {
            hitsEnabled = false;
            if (hitAnim && hitAnim.enabled)
            {
                hitAnim.StopHitAnimation();
            }
        }

        private Vector3 GetWorldPosition(Vector3 localPos)
            => transform.TransformPoint(localPos);

        private Quaternion GetWorldRotation(Vector3 localEuler)
            => transform.rotation * Quaternion.Euler(localEuler);

        private Vector3 GetInteractorAdjustedPosition(Vector3 localPos)
        {
            if (interactor == null)
                return GetWorldPosition(localPos);

            Vector3 worldPos = transform.TransformPoint(localPos);
            Vector3 offset = worldPos - interactorSnapshotPosition;
            return interactor.transform.position + interactor.transform.rotation * Quaternion.Inverse(interactorSnapshotRotation) * offset;
        }

        private Vector3 GetInteractorAdjustedControlPoint(Vector3 basePos, Vector3 controlOffset)
        {
            if (interactor == null)
                return GetWorldPosition(basePos + controlOffset);

            Vector3 worldPos = transform.TransformPoint(basePos + controlOffset);
            Vector3 offset = worldPos - interactorSnapshotPosition;
            return interactor.transform.position + interactor.transform.rotation * Quaternion.Inverse(interactorSnapshotRotation) * offset;
        }

        private Quaternion GetInteractorAdjustedRotation(Vector3 localEuler)
        {
            if (interactor == null)
                return GetWorldRotation(localEuler);

            Quaternion worldRot = transform.rotation * Quaternion.Euler(localEuler);
            Quaternion relativeRot = Quaternion.Inverse(interactorSnapshotRotation) * worldRot;
            return interactor.transform.rotation * relativeRot;
        }

        private Vector3 GetInteractorAdjustedInitialPosition(Transform target)
        {
            if (interactor == null || !initialInteractorRelatives.ContainsKey(target))
            {
                var initial = initialTransforms[target];
                return target.parent != null ? target.parent.TransformPoint(initial.localPosition) : initial.localPosition;
            }

            var relative = initialInteractorRelatives[target];
            return interactor.transform.position + interactor.transform.rotation * relative.relativePosition;
        }

        private Quaternion GetInteractorAdjustedInitialRotation(Transform target)
        {
            if (interactor == null || !initialInteractorRelatives.ContainsKey(target))
            {
                var initial = initialTransforms[target];
                return target.parent != null ? target.parent.rotation * initial.localRotation : initial.localRotation;
            }

            var relative = initialInteractorRelatives[target];
            return interactor.transform.rotation * relative.relativeRotation;
        }

        public void StartHitAnimation(HitConfig hit)
        {
            if (hit?.targetTransform == null) return;

            if (interactor != null)
            {
                interactorSnapshotPosition = interactor.transform.position;
                interactorSnapshotRotation = interactor.transform.rotation;
            }

            if (returnInitialPosition && !initialTransforms.ContainsKey(hit.targetTransform))
            {
                var target = hit.targetTransform;
                initialTransforms[target] = (target.localPosition, target.localRotation);

                if (interactor != null)
                {
                    Vector3 targetWorldPos = target.parent != null ? target.parent.TransformPoint(target.localPosition) : target.position;
                    Quaternion targetWorldRot = target.parent != null ? target.parent.rotation * target.localRotation : target.rotation;

                    Vector3 relativePos = Quaternion.Inverse(interactor.transform.rotation) * (targetWorldPos - interactor.transform.position);
                    Quaternion relativeRot = Quaternion.Inverse(interactor.transform.rotation) * targetWorldRot;

                    initialInteractorRelatives[target] = (relativePos, relativeRot);
                }
            }

            if (activeAnimations.TryGetValue(hit.targetTransform, out HitAnimationData existingAnim))
            {
                if (triggerOnInterruptedHits && !existingAnim.eventTriggered)
                {
                    existingAnim.hitConfig.onHitEnd?.Invoke();
                    TriggerHitReactionForHit(existingAnim.hitConfig);
                }

                existingAnim.hitConfig = hit;
                existingAnim.elapsed = 0f;
                existingAnim.startPos = hit.targetTransform.position;
                existingAnim.startRot = hit.targetTransform.rotation;
                existingAnim.eventTriggered = false;
                existingAnim.isInterrupted = true;

                if (smoothInterrupt)
                {
                    existingAnim.state = HitState.Hitting;
                    existingAnim.useOverrideStart = true;
                }
                else
                {
                    existingAnim.state = HitState.MovingToStart;
                    existingAnim.useOverrideStart = false;
                }
            }
            else
            {
                var animData = new HitAnimationData
                {
                    hitConfig = hit,
                    state = HitState.Hitting,
                    elapsed = 0f,
                    startPos = hit.targetTransform.position,
                    startRot = hit.targetTransform.rotation,
                    eventTriggered = false,
                    isInterrupted = false,
                    useOverrideStart = false
                };

                activeAnimations[hit.targetTransform] = animData;
            }
        }

        private void TriggerHitReactionForHit(HitConfig hit)
        {
            if (!interactor || !targetInteractor) return;

            Vector3 worldEndPos = GetInteractorAdjustedPosition(hit.endPosition);
            if (hit.alternateHitPosition != Vector3.zero)
            {
                worldEndPos = GetInteractorAdjustedPosition(hit.alternateHitPosition);
            }
            Vector3 worldStartPos = GetInteractorAdjustedPosition(hit.startPosition);

            Vector3 hitDirection = (worldEndPos - worldStartPos).normalized;

            targetInteractor.interactorIK.TriggerHitReaction(worldEndPos, hitDirection, hit.hitForce);
        }

        private Vector3 EvaluateBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1f - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;
            return uuu * p0 + 3f * uu * t * p1 + 3f * u * tt * p2 + ttt * p3;
        }

        public void ResetControlPoints(int hitIndex)
        {
            if (hitIndex < 0 || hitIndex >= hits.Count) return;
            hits[hitIndex].startControlPoint = new Vector3(0, 0.1f, 0f);
            hits[hitIndex].endControlPoint = new Vector3(0, 0.1f, 0f);
        }
    }
}
