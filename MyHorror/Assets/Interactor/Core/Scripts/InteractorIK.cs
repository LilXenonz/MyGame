using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace razz
{
	[HelpURL("https://negengames.com/interactor/components.html#interactorikcs")]
	[DisallowMultipleComponent]
	public class InteractorIK : MonoBehaviour
	{
		public enum IKPart
		{
			LeftFoot = 0,
			RightFoot = 1,
			LeftHand = 2,
			RightHand = 3,
			Body = 4,
		};

		//Interactor checks this for integration automation.
		public static short defaultFiles = 0;

		[SerializeField] private Animator _animator;
		public Animator Animator
		{
			get
			{
				if (_animator == null)
				{
					_animator = GetComponentInChildren<Animator>();
					if (_animator == null) Debug.LogWarning("Animator component could not found. Please assign it to InteractorIK manually.", this);
					else Debug.Log("Assign Animator to InteractorIK manually for best practice.", this);
				}
				return _animator;
			}
			set { _animator = value; }
		}

		public bool isHumanoid = true;
		public IKParts[] ikParts;
		[HideInInspector] public Interactor interactor;
		[HideInInspector] public FullBodyIKBehaviour fullBodyIKBehaviour;
        [HideInInspector] public HitReaction hitReaction;

        [HideInInspector] public bool lookEnabled = true;
		[HideInInspector] public Transform lookTarget;
		[HideInInspector] public float lookWeight;
		[HideInInspector] public Transform headBone;
		[HideInInspector] public Vector3 lastHeadDirection;

		//Used for holding IKParts' ikpart values as effector type int (FullBodyBipedEffector)
		private int[] _effectorOrder;
		private bool _useLateFixedUpdate = false;
		private bool _lateFixedUpdating = false;

		private float _lElbowH;
		private float _lElbowV;
		private float _rElbowH;
		private float _rElbowV;

		private void OnValidate()
		{
			if (ikParts == null || isHumanoid) return;

			for (int i = 0; i < ikParts.Length; i++)
			{
				ikParts[i].Validate();
			}
		}
		private void OnEnable()
		{
			CheckLateFixedUpdate();
		}
		private void OnDisable()
		{
			if (_lateFixedUpdating) StopCoroutine("LateFixedUpdate");
		}

		private void Start()
		{
			if (ikParts.Length == 0) return;
			if (!Animator) return;

			//10 long array is enough to hold all parts
			_effectorOrder = new int[10];
			CheckLateFixedUpdate();

			for (int i = 0; i < ikParts.Length; i++)
			{
				//ikParts[i].Init(Animator, isHumanoid);
				bool success = ikParts[i].Init(Animator, isHumanoid, this);
				if (!success) continue;
				if ((int)ikParts[i].part > 3) continue; //Skip other than hands and feet

				if (ikParts[i].matchChildBones)
				{
					ikParts[i].childBones = ikParts[i].boneTransform.GetComponentsInChildren<Transform>();

					//Remove excluded transfrom hierarchy from actual bone transforms
					if (ikParts[i].excludeFromBones.Length > 0)
					{
						List<Transform> transformRemoval = new List<Transform>();

						for (int a = 0; a < ikParts[i].excludeFromBones.Length; a++)
						{
							Transform[] excludedTransforms;
							if (ikParts[i].excludeFromBones[a])
							{
								excludedTransforms = ikParts[i].excludeFromBones[a].GetComponentsInChildren<Transform>();
								transformRemoval.AddRange(excludedTransforms);
							}
						}

						List<Transform> newChildBones = new List<Transform>();
						for (int j = 0; j < ikParts[i].childBones.Length; j++)
						{
							if (!transformRemoval.Contains(ikParts[i].childBones[j]))
								newChildBones.Add(ikParts[i].childBones[j]);
						}
						ikParts[i].childBones = newChildBones.ToArray();
					}
				}
			}
			SetEffectorOrder();

			if (!interactor) interactor = transform.root.GetComponentInChildren<Interactor>();
			if (interactor.fbikEnabled && !fullBodyIKBehaviour) SetInteractorFBIK();
		}

		private void CheckLateFixedUpdate()
        {
#if UNITY_2023_1_OR_NEWER
			if (Animator && Animator.updateMode == AnimatorUpdateMode.Fixed && !_lateFixedUpdating)
#else
			if (Animator && Animator.updateMode == AnimatorUpdateMode.AnimatePhysics && !_lateFixedUpdating)
#endif
			{
				_useLateFixedUpdate = true;
				StartCoroutine("LateFixedUpdate");
			}
		}

		public void SetInteractorFBIK()
		{
			if (!interactor) interactor = transform.root.GetComponentInChildren<Interactor>();
			if (!fullBodyIKBehaviour && interactor.fbikEnabled)
			{
				if (!(fullBodyIKBehaviour = GetComponent<FullBodyIKBehaviour>()))
				{
					if (Application.isPlaying)
					{
						Debug.Log("Interactor Full Body IK is enabled, but InteractorIK does not have a Full Body IK Behavior component assigned. To resolve this, press the Auto button on the Interactor or right click on Interactor IK and select Create Full Body IK while not in Play mode to automatically configure the required settings. Full Body IK has been disabled.", this);
						interactor.fbikEnabled = false;
					}
					else
					{
						Debug.Log("Interactor Full Body IK is enabled, but InteractorIK does not have a Full Body IK Behavior component assigned. Adding Full Body IK component. You can remove it and disable the Full Body IK feature on Interactor.", this);
						CreateFullBodyIK();
					}
				}
			}
		}

		public void TriggerHitReaction(Vector3 hitPoint, Vector3 hitDirection, float hitForce)
		{
			if (!hitReaction)
			{
				if (!(hitReaction = GetComponent<HitReaction>()))
				{
					Debug.LogWarning("Hit Reaction not found on this InteractorIK gameObject. Please add and set up the component to enable hit reactions.", this);
					return;
				}
				else hitReaction.fullBodyIKBehaviour = fullBodyIKBehaviour;
            }

			hitReaction.Hit(hitPoint, hitDirection, hitForce);
		}
        public void TriggerHitReaction(Collider collider, Vector3 hitPoint, Vector3 hitDirection, float hitForce)
        {
            if (!hitReaction)
            {
                if (!(hitReaction = GetComponent<HitReaction>()))
                {
                    Debug.LogWarning("Hit Reaction not found on this InteractorIK gameObject. Please add and set up the component to enable hit reactions.", this);
                    return;
                }
                else hitReaction.fullBodyIKBehaviour = fullBodyIKBehaviour;
            }

            hitReaction.Hit(hitPoint, hitDirection, hitForce);
        }

        public void SetHeadBone(Transform head)
		{
			headBone = head;
		}

		//Caching ikparts' part ints as FullBodyBipedEffector ints, so we dont have to check every call
		//which part is for which effector type
		private void SetEffectorOrder()
		{
			for (int i = 0; i < ikParts.Length; i++)
			{
				_effectorOrder[AvatorGoalToEffector(ikParts[i].part)] = i;
			}
		}

		private int EffectorToIKpart(Interactor.FullBodyBipedEffector effector)
		{
			int i = _effectorOrder[(int)effector];

			if (ikParts[i] == null)
			{
				Debug.LogWarning("Interactor has " + effector + ", but InteractorIK has not that part.", this);
				return -1;
			}
			return i;
		}

		public void StartInteraction(Interactor.FullBodyBipedEffector effector, InteractorTarget interactorTarget, InteractorObject interactorObject)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].StartInteraction(interactorTarget, interactorObject);

            if (interactor.fbikEnabled && fullBodyIKBehaviour)
            {
				if (ikParts[i].enabled && ikParts[i].currentTarget == interactorTarget)
				{
					FullBodyIK fbik = fullBodyIKBehaviour.fullBodyIK;
					if (ikParts[i].part == IKPart.LeftHand)
					{
						_lElbowH = fbik.leftArmEffectors.elbowPoleHorizontal;
						_lElbowV = fbik.leftArmEffectors.elbowPoleVertical;
					}
					else if (ikParts[i].part == IKPart.RightHand)
					{
						_rElbowH = fbik.rightArmEffectors.elbowPoleHorizontal;
						_rElbowV = fbik.rightArmEffectors.elbowPoleVertical;
					}
				}
			}
		}

		public void PauseInteraction(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].PauseInteraction();
		}

		public void ResumeInteraction(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].ResumeInteraction();
		}

		public void ResumeInteractionWithoutReset(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].ResumeInteractionWithoutReset();
		}

		public void ResetAfterResume(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].ResetAfterResume();
		}

		public void ResumeAll()
		{
			for (int i = 0; i < ikParts.Length; i++)
			{
				if (ikParts[i].pause)
				{
					ikParts[i].ResumeInteraction();
				}
			}
		}

		public void ReverseInteraction(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].ReverseInteraction();
		}

		public void ReverseAll()
		{
			for (int i = 0; i < ikParts.Length; i++)
			{
				if (ikParts[i].enabled)
				{
					ikParts[i].ReverseInteraction();
				}
			}
		}

		public void StopInteraction(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].StopInteraction();
		}

		public void StopAll()
		{
			for (int i = 0; i < ikParts.Length; i++)
			{
				if (ikParts[i].enabled)
				{
					ikParts[i].StopInteraction();
				}
			}
		}

		public float GetProgress(Interactor.FullBodyBipedEffector effector)
		{//0 to 1f target path, 1f is target, 1f to 2f back path
			int i = EffectorToIKpart(effector);
			if (i < 0) return 0;

			return ikParts[i].GetProgress();
		}

		public Transform GetTargetTransform(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return null;
			if (!ikParts[i].currentTarget) return null;

			return ikParts[i].currentTarget.transform;
		}

		public bool IsPaused(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return false;

			return ikParts[i].IsPaused();
		}

		public bool IsInInteraction(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return false;

			return ikParts[i].enabled;
		}

		public bool AnyInInteraction()
		{
			for (int i = 0; i < ikParts.Length; i++)
			{
				if (ikParts[i].enabled) return true;
			}
			return false;
		}

		public Transform GetBone(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return null;

			return ikParts[i].boneTransform;
		}

		//Converting FullBodyBipedEffector int (Coming from Interactor) to AvatarGoal int (used by Unity IK)
		private int EffectorToAvatarGoal(Interactor.FullBodyBipedEffector effector)
		{
			switch ((int)effector)
			{
				case 0:
					return 4;
				case 1:
					return 5;
				case 2:
					return 6;
				case 3:
					return 7;
				case 4:
					return 8;
				case 5:
					return 2;
				case 6:
					return 3;
				case 7:
					return 0;
				case 8:
					return 1;
				default:
					return -1;
			}
		}
		//Converting AvatarGoal int (used by Unity IK) to FullBodyBipedEffector int (Coming from Interactor)
		public static int AvatorGoalToEffector(IKPart part)
		{
			switch ((int)part)
			{
				case 0:
					return 7;
				case 1:
					return 8;
				case 2:
					return 5;
				case 3:
					return 6;
				case 4:
					return 0;
				case 5:
					return 1;
				case 6:
					return 2;
				case 7:
					return 3;
				case 8:
					return 4;
				default:
					return -1;
			}
		}

		public void ChangeIKPartTarget(Interactor.FullBodyBipedEffector effector, InteractorTarget newTarget, InteractorObject newInteractorObject)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].currentTarget = newTarget;
			ikParts[i].targetDuration = newInteractorObject.targetDuration;
			ikParts[i].backDuration = newInteractorObject.backDuration;
			ikParts[i].pause = newInteractorObject.pauseOnInteraction;
			ikParts[i].easer = Ease.FromType(newInteractorObject.easeType);
			ikParts[i].currentTarget.PrepareTarget(ikParts[i].positionBeforeIK);
		}

		public void ChangeIKPartWeight(Interactor.FullBodyBipedEffector effector, float newWeight)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].SetNewWeight(newWeight);
		}

		public Vector3 GetPositionBeforeIK(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return Vector3.zero;

			return ikParts[i].positionBeforeIK;
		}
		public Quaternion GetRotationBeforeIK(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return Quaternion.identity;

			return ikParts[i].rotationBeforeIK;
		}

		public void OnAnimatorIK(int layerIndex)
		{//Needs to be called by Animator. If Animator is on different object (which is wrong), put AnimatorCallback.cs on that object and assign InteractorIK.
			if (isHumanoid)
			{
				for (int i = 0; i < ikParts.Length; i++)
				{
					CacheOriginals(ikParts[i]);

					if (ikParts[i].enabled && ikParts[i].currentTarget)
					{
						ikParts[i].UpdateWeight();
						if (interactor.fbikEnabled && fullBodyIKBehaviour) SetAnimatorFBIKPos(ikParts[i]);
						else SetAnimatorIKPos(ikParts[i]);

						if (ikParts[i].currentTarget && ikParts[i].fixWristDeformation && ikParts[i].currentTarget.setRotation)
							SetAnimatorIKRot(ikParts[i]);
					}

					if (ikParts[i].resetted)
					{
						ikParts[i].weight = 0;
						if (interactor.fbikEnabled && fullBodyIKBehaviour) SetAnimatorFBIKPos(ikParts[i]);
						else SetAnimatorIKPos(ikParts[i]);

						if (ikParts[i].currentTarget && ikParts[i].fixWristDeformation && ikParts[i].currentTarget.setRotation)
							SetAnimatorIKRot(ikParts[i]);

						ikParts[i].resetted = false;
					}
				}

				if (!lookEnabled) return;

				if (lookTarget != null && lookWeight > 0)
				{
					SetLook(lookTarget, lookWeight);
				}
			}
		}

		private void LateUpdate()
		{
			if (_useLateFixedUpdate) return;

			if (interactor.fbikEnabled && fullBodyIKBehaviour) fullBodyIKBehaviour.fullBodyIK.Update();
			else if (interactor.fbikEnabled && !fullBodyIKBehaviour) SetInteractorFBIK();

			CalculateAfterAnim();
		}

		private IEnumerator LateFixedUpdate()
		{
			_lateFixedUpdating = true;
			while (_useLateFixedUpdate)
			{
				yield return new WaitForFixedUpdate();

				if (interactor.fbikEnabled && fullBodyIKBehaviour) fullBodyIKBehaviour.fullBodyIK.Update();
				else if (interactor.fbikEnabled && !fullBodyIKBehaviour) SetInteractorFBIK();

				CalculateAfterAnim();
			}
			_lateFixedUpdating = false;
		}

		private void CalculateAfterAnim()
		{
			if (!isHumanoid) //TwoBoneIK
			{
				for (int i = 0; i < ikParts.Length; i++)
				{
					if (ikParts[i].enabled && ikParts[i].currentTarget)
					{
						ikParts[i].UpdateWeight();
						ikParts[i].positionBeforeIK = ikParts[i].boneTransform.position;
						ikParts[i].rotationBeforeIK = ikParts[i].boneTransform.rotation;
						ikParts[i].SolveIKPart();
					}
				}

				if (lookTarget != null && lookWeight > 0)
				{
					SetLookAlternativeHeadBone(lookTarget, lookWeight);
				}
			}

			for (int i = 0; i < ikParts.Length; i++)
			{
				if (ikParts[i].enabled && ikParts[i].currentTarget)
				{
					Quaternion startRotation;
					if (interactor.fbikEnabled && fullBodyIKBehaviour)
						startRotation = ikParts[i].rotationBeforeIK;
					else startRotation = ikParts[i].boneTransform.rotation;

					//We're changing bone rotation here instead of SetAnimatorIKRot because
					//SetIKRotation needs a direction then it calculates target rotation,
					//not the goal rotation itself. We already have final bone rotation.
					//But SetIKRotation also fixes wrist rotation deformations.
					ikParts[i].boneTransform.rotation = ikParts[i].currentTarget.GetRotation(startRotation, ikParts[i].weight, ikParts[i].HalfDone());

					//To pass correct hand rotations for arm twists
                    if (interactor.fbikEnabled && fullBodyIKBehaviour && fullBodyIKBehaviour.fullBodyIK.settings.rollEnabled)
                    {
						FullBodyIK fbik = fullBodyIKBehaviour.fullBodyIK;
						if (ikParts[i].part == IKPart.LeftHand)
                        {
							fbik.leftArmEffectors.elbowPoleHorizontal = Mathf.Lerp(_lElbowH, ikParts[i].currentTarget.elbowPoleHorizontal, ikParts[i].weight);
							fbik.leftArmEffectors.elbowPoleVertical = Mathf.Lerp(_lElbowV, ikParts[i].currentTarget.elbowPoleVertical, ikParts[i].weight);
							fbik.leftArmBones.handRoll.transform.rotation = ikParts[i].boneTransform.rotation;
                        }
                        else if (ikParts[i].part == IKPart.RightHand)
                        {
							fbik.rightArmEffectors.elbowPoleHorizontal = Mathf.Lerp(_rElbowH, ikParts[i].currentTarget.elbowPoleHorizontal, ikParts[i].weight);
							fbik.rightArmEffectors.elbowPoleVertical = Mathf.Lerp(_rElbowV, ikParts[i].currentTarget.elbowPoleVertical, ikParts[i].weight);
							fbik.rightArmBones.handRoll.transform.rotation = ikParts[i].boneTransform.rotation;
                        }
                    }

                    if (ikParts[i].matchChildBones) //Global toggle on InteractorIK
					{
						if ((int)ikParts[i].part > 3) continue; //Skip other than hands and feet

						//This is for matching the effector bones to children bones of target. 
						//Its LateUpdate because it needs to be after Unity animation jobs done.
						SetChildBones(ikParts[i]);
					}
				}
				else if (ikParts[i].matchChildBones && ikParts[i].waitForReset)
				{
					if ((int)ikParts[i].part > 3) continue;

					SetChildBones(ikParts[i]);
				}
			}

			//Cache the last head forward direction for LookAtTarget look ending process.
			//We need it here because it needs to be after IK and look updates.
			if (headBone) lastHeadDirection = headBone.forward;
		}

		private void CacheOriginals(IKParts ikPart)
		{
			if ((int)ikPart.part < 4)
			{
				if (!ikPart.boneTransform) return;

				ikPart.positionBeforeIK = ikPart.boneTransform.position;
				ikPart.rotationBeforeIK = ikPart.boneTransform.rotation;
			}
			else
			{
				ikPart.positionBeforeIK = Animator.bodyPosition;
				ikPart.rotationBeforeIK = Animator.bodyRotation;
			}
		}

		private void SetAnimatorFBIKPos(IKParts ikPart)
		{
			switch ((int)ikPart.part)
			{
				case 0://Left Foot
					{
						fullBodyIKBehaviour.fullBodyIK.leftLegEffectors.foot.transform.position = ikPart.weightedPosition;
						fullBodyIKBehaviour.fullBodyIK.leftLegEffectors.foot.positionWeight = Mathf.Ceil(ikPart.weight);
					}
					break;
				case 1://Right Foot
					{
						fullBodyIKBehaviour.fullBodyIK.rightLegEffectors.foot.transform.position = ikPart.weightedPosition;
						fullBodyIKBehaviour.fullBodyIK.rightLegEffectors.foot.positionWeight = Mathf.Ceil(ikPart.weight);
					}
					break;
				case 2://Left Hand
					{
						fullBodyIKBehaviour.fullBodyIK.leftArmEffectors.wrist.transform.position = ikPart.weightedPosition;
						fullBodyIKBehaviour.fullBodyIK.leftArmEffectors.wrist.positionWeight = Mathf.Ceil(ikPart.weight);
					}
					break;
				case 3://Right Hand
					{
						fullBodyIKBehaviour.fullBodyIK.rightArmEffectors.wrist.transform.position = ikPart.weightedPosition;
						fullBodyIKBehaviour.fullBodyIK.rightArmEffectors.wrist.positionWeight = Mathf.Ceil(ikPart.weight);
					}
					break;
				case 4://Body
					{
						Animator.bodyPosition = ikPart.weightedPosition;
					}
					break;
				default: Debug.Log("Error"); break;
			}
		}
		private void SetAnimatorIKPos(IKParts ikPart)
		{
			//Hands & Feet
			if ((int)ikPart.part < 4)
			{
				Animator.SetIKPosition((AvatarIKGoal)ikPart.part, ikPart.weightedPosition);
				Animator.SetIKPositionWeight((AvatarIKGoal)ikPart.part, Mathf.Ceil(ikPart.weight));
				return;
			}
			//Body
			else if ((int)ikPart.part == 4)
			{
				Animator.bodyPosition = ikPart.weightedPosition;
				return;
			}
		}
		private void SetAnimatorIKRot(IKParts ikPart)
		{
			if ((int)ikPart.part < 4)
			{
				Animator.SetIKRotation((AvatarIKGoal)ikPart.part, ikPart.currentTarget.GetRotation(ikPart.rotationBeforeIK, ikPart.weight, ikPart.HalfDone()));
				Animator.SetIKRotationWeight((AvatarIKGoal)ikPart.part, ikPart.weight);
				return;
			}
		}

		private void SetLook(Transform target, float weight)
		{
			Animator.SetLookAtPosition(target.position);
			Animator.SetLookAtWeight(weight);
		}

		private void SetLookAlternativeHeadBone(Transform target, float weight)
		{
			Quaternion oneBoneTargetRotation = Quaternion.LookRotation(target.position - headBone.position);
			headBone.rotation = Quaternion.Slerp(headBone.rotation, oneBoneTargetRotation, weight);
		}

		private void SetChildBones(IKParts ikpart)
		{
			if (ikpart.currentTarget == null || !ikpart.currentTarget.MatchSource) return;
			if (!ikpart.currentTarget.matchChildBones) return;

			if (ikpart.waitForReset)
			{
				ikpart.currentTarget.RotateChildren(ikpart.childBones, 1f);
				return;
			}

			ikpart.currentTarget.RotateChildren(ikpart.childBones, ikpart.weight);
		}

		[System.Serializable]
		public class IKParts
		{
			[Tooltip("Select the body part. This will match with the Interactor effector type.")]
			public IKPart part;
			[Tooltip("Global control for matching child bone rotations (Fingers for hand for example). Only possible for hands and feet. Also you have this option in every InteractorTarget if you wish to disable for a specific target only.")]
			public bool matchChildBones = true;
			[Tooltip("When disabled, hand rotation will focus on wrist but this can cause deformation on the wrist when target rotation is too much. Enabling this option will fix wrist deformation with minor performance cost by distributing some of the rotation to lower arm (Like in real world).")]
			public bool fixWristDeformation;
			[Tooltip("If you wish to exclude a transform (with its children) from this bone hierarchy, assign here. (A child object on hand for example. So this way bone count won't change and excluded objects won't be included for matching child rotations.) Hands and feet only.")]
			public Transform[] excludeFromBones;
			[Tooltip("Current target for this IK Part. Debug purposes only, will be changed by Interator in runtime.")]
			[ReadOnly] public InteractorTarget currentTarget;
			[Tooltip("Current weight for this IK Part (0 is default animation position, 1 is target position). Debug purposes only, will be changed by InteratorIK in runtime.")]
			[ReadOnly] public float weight;

			//TwoBoneIK properties
			[Space(10)]
			[Conditional(Condition.Show, nameof(IsGeneric))]
			[Tooltip("Assign the root bone here. (Arm/shoulder for example)")]
			public Transform rootBone;
			[Conditional(Condition.Show, nameof(IsGeneric))]
			[Tooltip("Assign the middle bone here. (Forearm/elbow for example)")]
			public Transform midBone;
			[Conditional(Condition.Show, nameof(IsGeneric))]
			[Tooltip("Assign the tip bone here. (Hand for example)")]
			public Transform tipBone;
			[Conditional(Condition.Show, nameof(IsGeneric))]
			[Tooltip("Assign the hint transform here. (Middle bone will bend towards this)")]
			public Transform hint;
			[Conditional(Condition.Show, nameof(IsGeneric))]
			[Tooltip("Since bone structures change a lot, you need to set this yourself until you're satisfied with results. If your IK animation looks weird, change the value until it fixes (between 0 - 359 degrees).")]
			public float rootRotationOffset;
			[Conditional(Condition.Show, nameof(IsGeneric))]
			[Tooltip("Since bone structures change a lot, you need to set this yourself until you're satisfied with results. If your IK animation looks weird, change the value until it fixes (between 0 - 359 degrees).")]
			public float midRotationOffset;

			[HideInInspector] public float targetDuration;
			[HideInInspector] public float backDuration;
			[HideInInspector] public bool pause;
            [HideInInspector] public float pauseTime;
            [HideInInspector] public bool waitForReset;
			[HideInInspector] public Transform boneTransform;
			[HideInInspector] public bool enabled;
			[HideInInspector] public bool interrupt;
			[HideInInspector] public Easer easer;
			[HideInInspector] public Transform[] childBones;
			[HideInInspector] public Vector3 weightedPosition;
			[HideInInspector] public Vector3 positionBeforeIK;
			[HideInInspector] public Quaternion rotationBeforeIK;
			[HideInInspector] public bool resetted;

			private InteractorIK _interactorIK;
			private AvatarIKGoal _avatarIKGoal;
			private float _elapsed;
			private bool _halfDone;
			private bool _interactReset;
			private float _lastWeightBeforeHalf;

			//TwoBoneIK
			private TwoBoneIKSolver _twoBoneIKSolver;

			public bool Init(Animator anim, bool isHumanoid, InteractorIK interactorIK)
			{
				_interactorIK = interactorIK;
				if (isHumanoid) //Unity IK
				{
					_avatarIKGoal = (AvatarIKGoal)part;
					boneTransform = anim.GetBoneTransform((HumanBodyBones)AvatarGoaltoHBB(_avatarIKGoal));
				}
				else //TwoBoneIK
				{
					if (!rootBone)
					{
						Debug.LogWarning(part + "'s root bone is missing on InteractorIK!");
						return false;
					}
					if (tipBone)
						boneTransform = tipBone;
					else if (midBone)
						boneTransform = midBone;
					else
						boneTransform = rootBone;

					if (!(_twoBoneIKSolver = rootBone.GetComponent<TwoBoneIKSolver>()))
					{
						_twoBoneIKSolver = rootBone.gameObject.AddComponent<TwoBoneIKSolver>();
					}
					_twoBoneIKSolver.Init(rootBone, midBone, tipBone, hint, rootRotationOffset, midRotationOffset);
				}

				//Setting default values for those shouldn't be zero
				if (backDuration <= 0) backDuration = 1f;
				if (targetDuration <= 0) targetDuration = 1f;
				return true;
			}

			//TwoBoneIK
			public void Validate()
			{
				if (_twoBoneIKSolver)
				{
					_twoBoneIKSolver.Validate(rootRotationOffset, midRotationOffset);
				}
			}
			public void SolveIKPart()
			{
				if (_twoBoneIKSolver)
				{
					_twoBoneIKSolver.SolveIK(weightedPosition, weight);
				}
			}

			//Unity IK
			//Converts an int from Unity AvatarGoal value to Unity HumanBodyBones
			private int AvatarGoaltoHBB(AvatarIKGoal input)
			{
				switch ((int)input)
				{
					case 0:
						return 5;
					case 1:
						return 6;
					case 2:
						return 17;
					case 3:
						return 18;
					case 4:
						return 7;
					case 5:
						return 11;
					case 6:
						return 12;
					case 7:
						return 1;
					case 8:
						return 2;
					default:
						return -1;
				}
			}

			public void SetNewWeight(float newWeight)
			{
				currentTarget.PrepareTarget(positionBeforeIK);
				_elapsed = (targetDuration + backDuration) * newWeight;
			}

			public void StartInteraction(InteractorTarget interactorTarget, InteractorObject interactorObject)
			{
				if (enabled && !interrupt) return;
				if (enabled) ResetIK();
				if (!interactorTarget)
				{
					ResetIK();
					enabled = false;
					return;
				}

				this.targetDuration = interactorObject.targetDuration;
				this.backDuration = interactorObject.backDuration;
				this.pause = interactorObject.pauseOnInteraction;
				this.pauseTime = interactorObject.pauseTime;
				this.easer = Ease.FromType(interactorObject.easeType);
				this.interrupt = interactorObject.interruptible;

				this.currentTarget = interactorTarget;
				currentTarget.PrepareTarget(boneTransform.position);

				enabled = true;
			}

			public void PauseInteraction()
			{
				pause = true;
			}

			public void ResumeInteraction()
			{
				pause = false;
			}

			public void ResumeInteractionWithoutReset()
			{
				pause = false;
				waitForReset = true;
			}

			public void ResetAfterResume()
			{
				waitForReset = false;
			}

			public void ReverseInteraction()
			{
				if (!_halfDone)
				{
					_halfDone = true;
					_elapsed = 0;
					pause = false;
				}
			}

			public void StopInteraction()
			{
				ResetIK();
				enabled = false;
			}

			public float GetProgress()
			{
				if (!_halfDone) return (_elapsed / targetDuration);
				else return 1f + (_elapsed / backDuration);
			}

			public bool IsPaused()
			{
				if (!_halfDone)
				{
					return false;
				}
				else
				{
					return pause;
				}
			}

			public bool HalfDone()
			{
				return _halfDone;
			}

			private void CalcWeight()
			{
				if (!enabled) return;
				if (!currentTarget)
				{
					StopInteraction();
					return;
				}

				if (_elapsed < targetDuration && !_halfDone)
				{
					_elapsed += Time.deltaTime;
					weight = Mathf.Clamp01(easer((_elapsed / targetDuration), currentTarget.IntObj.speedCurve));
					currentTarget.UpdateFirstAndLastPosition(positionBeforeIK);
					weightedPosition = currentTarget.GetTargetPosition(weight);
					_lastWeightBeforeHalf = weight;
					currentTarget.UpdatePivot();
				}
				else if (_elapsed >= targetDuration && !_halfDone)
				{
					_halfDone = true;
					_elapsed = 0;
				}
				if (_halfDone && pause)
				{
                    weightedPosition = currentTarget.GetBackPosition(1f); //Continue to call to update targets values

					if (currentTarget.intObj.pauseTime > 0)
					{
                        if (pauseTime > 0) pauseTime -= Time.deltaTime;
                        else pause = false;
                    }
                }

				if (_elapsed < backDuration && _halfDone && !pause)
				{
					_elapsed += Time.deltaTime;
					_elapsed *= currentTarget.BackPathSpeed();

					if (weight == 0) _elapsed = backDuration;

					if (currentTarget.IntObj.easeType == EaseType.CustomCurve)
					{
						weight = Mathf.Clamp01(easer((1f + (_elapsed / backDuration)), currentTarget.IntObj.speedCurve));
					}
					else
					{
						weight = Mathf.Clamp01(_lastWeightBeforeHalf - easer(_elapsed / backDuration));
					}

					currentTarget.UpdateFirstAndLastPosition(positionBeforeIK);
					weightedPosition = currentTarget.GetBackPosition(weight);
				}
				else if (_elapsed >= backDuration && _halfDone && !pause && !waitForReset)
				{
					_interactReset = true;
				}
				else if (_elapsed >= backDuration && _halfDone && !pause) weight = 0f;

				if (_interactReset)
				{
					ResetIK();
					enabled = false;
					_interactReset = false;
				}

				weight = Round(weight, 2); //2 for 0.xx level

				if (weight == 1f && !_halfDone)
				{
					_halfDone = true;
					_elapsed = 0;
				}
			}

			private float Round(float weight, int digits)
			{
				int multiplier = (int)Mathf.Pow(10, digits);
				float scaled = weight * multiplier;
				float rounded = (scaled - (int)scaled < 0.5f) ?
					Mathf.Floor(scaled) : Mathf.Ceil(scaled);
				return rounded / multiplier;
			}

			private void ResetIK()
			{
				_halfDone = false;
				_elapsed = 0;
				//pause = false;
				pauseTime = -1f;
				weight = 0;
				if (currentTarget)
				{
					currentTarget.EndTarget();
					currentTarget = null;
				}

				if (_interactorIK.interactor.fbikEnabled && _interactorIK.fullBodyIKBehaviour)
				{
					FullBodyIK fbik = _interactorIK.fullBodyIKBehaviour.fullBodyIK;
					if (part == IKPart.LeftHand)
					{
						fbik.leftArmEffectors.elbowPoleHorizontal = _interactorIK._lElbowH;
						fbik.leftArmEffectors.elbowPoleVertical = _interactorIK._lElbowV;
					}
					else if (part == IKPart.RightHand)
					{
						fbik.rightArmEffectors.elbowPoleHorizontal = _interactorIK._rElbowH;
						fbik.rightArmEffectors.elbowPoleVertical = _interactorIK._rElbowV;
					}
				}

				resetted = true;
			}

			public void UpdateWeight()
			{
				if (!enabled) return;

				CalcWeight();
			}
		}

		private bool IsGeneric()
		{
			if (isHumanoid) return false;
			else return true;
		}

		[ContextMenu("Create Full Body IK")]
		private void CreateFullBodyIK()
		{
#if UNITY_EDITOR
			Interactor interactor = transform.root.GetComponentInChildren<Interactor>();

			if (!interactor)
			{
				Debug.LogWarning("Interactor could not found.");
				return;
			}

			if (InteractorIK.defaultFiles != 0 && interactor.fbikEnabled)
			{
				interactor.fbikEnabled = false;
                Debug.LogWarning("Full Body IK can not be used with Final IK because it is also an full body IK solver. Please use Final IK instead.");
            }

			if (interactor.fbikEnabled)
			{
				if (!fullBodyIKBehaviour)
				{
					fullBodyIKBehaviour = FullBodyIKBehaviour.Create(this);
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
				}
			}
			else Debug.LogWarning("Full Body IK is not enabled on Interactor.");
#endif
		}
	}
}
