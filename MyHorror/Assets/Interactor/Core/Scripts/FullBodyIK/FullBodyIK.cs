using UnityEngine;
using System.Collections.Generic;

namespace razz
{
	[System.Serializable]
	public partial class FullBodyIK
	{
		[System.Serializable]
		public class BodyBones
		{
			public Bone hips;
			public Bone spine;
			public Bone spine2;
			public Bone spine3;
			public Bone spine4;

			public Bone spineU { get { return spine4; } }
		}

		[System.Serializable]
		public class HeadBones
		{
			public Bone neck;
			public Bone head;
		}

		[System.Serializable]
		public class LegBones
		{
			public Bone leg;
			public Bone knee;
			public Bone foot;
		}

		[System.Serializable]
		public class ArmBones
		{
			public Bone shoulder;
			public Bone arm;
			public Bone armRoll;
			public Bone handRoll;
			public Bone elbow;
			public Bone elbowPole;
			public Bone wrist;
		}

		[System.Serializable]
		public class BodyEffectors
		{
			public Effector hips;
		}

		[System.Serializable]
		public class HeadEffectors
		{
			public Effector neck;
			public Effector head;
		}

		[System.Serializable]
		public class LegEffectors
		{
			public Effector knee;
			public Effector foot;
		}

		[System.Serializable]
		public class ArmEffectors
		{
			public Effector arm;
			public Effector elbow;
			public float elbowPoleHorizontal = 0.2f;
			public float elbowPoleVertical = 0.5f;
			public Effector wrist;
		}

		public enum AutomaticBool
		{
			Auto = -1,
			Disable = 0,
			Enable = 1,
		}

		public enum SyncDisplacement
		{
			Disable,
			Firstframe,
			Everyframe,
		}

		[System.Serializable]
		public class Settings
		{
			public AutomaticBool animatorEnabled = AutomaticBool.Auto;
			public AutomaticBool resetTransforms = AutomaticBool.Auto;
			public SyncDisplacement syncDisplacement = SyncDisplacement.Disable;

			public AutomaticBool shoulderDirYAsNeck = AutomaticBool.Auto;

			public bool automaticPrepareHumanoid = true;
			public bool automaticConfigureSpineEnabled = false;

			public bool rollEnabled = true;

			public bool createEffectorTransform = true;

			[System.Serializable]
			public class BodyIK
			{
				public bool forceSolveEnabled = true;

				public bool lowerSolveEnabled = true;
				public bool upperSolveEnabled = true;
				public bool computeWorldTransform = true;

				public bool shoulderSolveEnabled = true;
				public float shoulderSolveBendingRate = 0.25f;
                public bool shoulderLimitEnabled = true;
				public float shoulderLimitAngleYPlus = 30.0f;
				public float shoulderLimitAngleYMinus = 1.0f;
				public float shoulderLimitAngleZ = 30.0f;

				public float spineDirXLegToArmRate = 0.5f;
				public float spineDirXLegToArmToRate = 1.0f;
				public float spineDirYLerpRate = 0.5f;

				public float upperBodyMovingfixRate = 1.0f;
				public float upperHeadMovingfixRate = 0.8f;
				public float upperCenterLegTranslateRate = 0.5f;
				public float upperSpineTranslateRate = 0.65f;
				public float upperCenterLegRotateRate = 0.6f;
				public float upperSpineRotateRate = 0.9f;
				public float upperPostTranslateRate = 1.0f;

				public bool upperSolveHipsEnabled = true;
				public bool upperSolveSpineEnabled = true;
				public bool upperSolveSpine2Enabled = true;
				public bool upperSolveSpine3Enabled = true;
				public bool upperSolveSpine4Enabled = true;

				public float upperCenterLegLerpRate = 1.0f;
				public float upperSpineLerpRate = 1.0f;

				public bool upperDirXLimitEnabled = true;
				public float upperDirXLimitAngleY = 20.0f;

				public bool spineLimitEnabled = true;
				public bool spineAccurateLimitEnabled = false;
				public float spineLimitAngleX = 40.0f;
				public float spineLimitAngleY = 25.0f;

				public float upperContinuousPreTranslateRate = 0.2f;
				public float upperContinuousPreTranslateStableRate = 0.65f;
				public float upperContinuousCenterLegRotationStableRate = 0.0f;
				public float upperContinuousPostTranslateStableRate = 0.01f;
				public float upperContinuousSpineDirYLerpRate = 0.5f;

				public float upperNeckToCenterLegRate = 0.6f;
				public float upperNeckToSpineRate = 0.9f;
			}

			[System.Serializable]
			public class LimbIK
			{
				public bool legAlwaysSolveEnabled = true;
				public bool armAlwaysSolveEnabled = false;

				public float automaticKneeBaseAngle = 0.0f;

				public bool presolveKneeEnabled = false;
				public bool presolveElbowEnabled = false;
				public float presolveKneeRate = 1.0f;
				public float presolveKneeLerpAngle = 10.0f;
				public float presolveKneeLerpLengthRate = 0.1f;
				public float presolveElbowRate = 1.0f;
				public float presolveElbowLerpAngle = 10.0f;
				public float presolveElbowLerpLengthRate = 0.1f;

				public bool prefixLegEffectorEnabled = true;

				public float prefixLegUpperLimitAngle = 60.0f;
				public float prefixKneeUpperLimitAngle = 45.0f;

				public float legEffectorMinLengthRate = 0.1f;
				public float legEffectorMaxLengthRate = 0.9999f;
				public float armEffectorMaxLengthRate = 0.9999f;

				public bool armBasisForcefixEnabled = true;
				public float armBasisForcefixEffectorLengthRate = 0.6f;
				public float armBasisForcefixEffectorLengthLerpRate = 0.03f;

				public bool armEffectorBackfixEnabled = true;
				public bool armEffectorInnerfixEnabled = true;
				public float armEffectorBackBeginAngle = 5.0f;
				public float armEffectorBackCoreBeginAngle = -10.0f;
				public float armEffectorBackCoreEndAngle = -30.0f;
				public float armEffectorBackEndAngle = -160.0f;
				public float armEffectorBackCoreUpperAngle = 8.0f;
				public float armEffectorBackCoreLowerAngle = -15.0f;
				public float automaticElbowBaseAngle = 30.0f;
				public float automaticElbowLowerAngle = 90.0f;
				public float automaticElbowUpperAngle = 90.0f;
				public float automaticElbowBackUpperAngle = 180.0f;
				public float automaticElbowBackLowerAngle = 330.0f;
				public float elbowFrontInnerLimitAngle = 5.0f;
				public float elbowBackInnerLimitAngle = 0.0f;
				public bool wristLimitEnabled = false;
				public float wristLimitAngle = 90.0f;
				public bool footLimitEnabled = true;
				public float footLimitYaw = 45.0f;
				public float footLimitPitchUp = 45.0f;
				public float footLimitPitchDown = 60.0f;
				public float footLimitRoll = 45.0f;
			}

			[System.Serializable]
			public class HeadIK
			{
				public float neckLimitPitchUp = 15.0f;
				public float neckLimitPitchDown = 30.0f;
				public float neckLimitRoll = 5.0f;

				public float headLimitYaw = 60.0f;
				public float headLimitPitchUp = 15.0f;
				public float headLimitPitchDown = 15.0f;
				public float headLimitRoll = 5.0f;
			}

			public BodyIK bodyIK;
			public LimbIK limbIK;
			public HeadIK headIK;

			public void Prefix()
			{
				SafeNew( ref bodyIK );
				SafeNew( ref limbIK );
				SafeNew( ref headIK );
			}
		}

		[System.Serializable]
		public class EditorSettings
		{
			public bool debugMode = false;
			public int toolbarSelected;
			public bool isShowEffectorTransform;
		}
		public class InternalValues
		{
			public bool animatorEnabled;
			public bool resetTransforms;
			public bool continuousSolverEnabled;
			public int shoulderDirYAsNeck = -1;

			public Vector3 defaultRootPosition = Vector3.zero;
			public Matrix3x3 defaultRootBasis = Matrix3x3.identity;
			public Matrix3x3 defaultRootBasisInv = Matrix3x3.identity;
			public Quaternion defaultRootRotation = Quaternion.identity;
			public Vector3 baseHipsPos = Vector3.zero;
			public Matrix3x3 baseHipsBasis = Matrix3x3.identity;

			public class BodyIK
			{
				public CachedDegreesToSin shoulderLimitThetaYPlus = CachedDegreesToSin.zero;
				public CachedDegreesToSin shoulderLimitThetaYMinus = CachedDegreesToSin.zero;
				public CachedDegreesToSin shoulderLimitThetaZ = CachedDegreesToSin.zero;

				public CachedRate01 upperCenterLegTranslateRate = CachedRate01.zero;
				public CachedRate01 upperSpineTranslateRate = CachedRate01.zero;

				public CachedRate01 upperPreTranslateRate = CachedRate01.zero;
				public CachedRate01 upperPostTranslateRate = CachedRate01.zero;

				public CachedRate01 upperCenterLegRotateRate = CachedRate01.zero;
				public CachedRate01 upperSpineRotateRate = CachedRate01.zero;
				public bool isFuzzyUpperCenterLegAndSpineRotationRate = true;

				public CachedDegreesToSin upperDirXLimitThetaY = CachedDegreesToSin.zero;

				public CachedScaledValue spineLimitAngleX = CachedScaledValue.zero;
				public CachedScaledValue spineLimitAngleY = CachedScaledValue.zero;

				public CachedRate01 upperContinuousPreTranslateRate = CachedRate01.zero;
				public CachedRate01 upperContinuousPreTranslateStableRate = CachedRate01.zero;
				public CachedRate01 upperContinuousCenterLegRotationStableRate = CachedRate01.zero;
				public CachedRate01 upperContinuousPostTranslateStableRate = CachedRate01.zero;

				public void Update( Settings.BodyIK settingsBodyIK )
				{
					Assert( settingsBodyIK != null );

					if( shoulderLimitThetaYPlus._degrees != settingsBodyIK.shoulderLimitAngleYPlus ) {
						shoulderLimitThetaYPlus._Reset( settingsBodyIK.shoulderLimitAngleYPlus );
					}
					if( shoulderLimitThetaYMinus._degrees != settingsBodyIK.shoulderLimitAngleYMinus ) {
						shoulderLimitThetaYMinus._Reset( settingsBodyIK.shoulderLimitAngleYMinus );
					}
					if( shoulderLimitThetaZ._degrees != settingsBodyIK.shoulderLimitAngleZ ) {
						shoulderLimitThetaZ._Reset( settingsBodyIK.shoulderLimitAngleZ );
					}

					if( upperCenterLegTranslateRate._value != settingsBodyIK.upperCenterLegTranslateRate ||
						upperSpineTranslateRate._value != settingsBodyIK.upperSpineTranslateRate ) {
						upperCenterLegTranslateRate._Reset( settingsBodyIK.upperCenterLegTranslateRate );
						upperSpineTranslateRate._Reset( Mathf.Max( settingsBodyIK.upperCenterLegTranslateRate, settingsBodyIK.upperSpineTranslateRate ) );
					}

					if( upperPostTranslateRate._value != settingsBodyIK.upperPostTranslateRate ) {
						upperPostTranslateRate._Reset( settingsBodyIK.upperPostTranslateRate );
					}

					if( upperCenterLegRotateRate._value != settingsBodyIK.upperCenterLegRotateRate ||
						upperSpineRotateRate._value != settingsBodyIK.upperSpineRotateRate ) {
						upperCenterLegRotateRate._Reset( settingsBodyIK.upperCenterLegRotateRate );
						upperSpineRotateRate._Reset( Mathf.Max( settingsBodyIK.upperCenterLegRotateRate, settingsBodyIK.upperSpineRotateRate ) );
						isFuzzyUpperCenterLegAndSpineRotationRate = IsFuzzy( upperCenterLegRotateRate.value, upperSpineRotateRate.value );
					}

					if( spineLimitAngleX._a != settingsBodyIK.spineLimitAngleX ) {
						spineLimitAngleX._Reset( settingsBodyIK.spineLimitAngleX, Mathf.Deg2Rad );
					}
					if( spineLimitAngleY._a != settingsBodyIK.spineLimitAngleY ) {
						spineLimitAngleY._Reset( settingsBodyIK.spineLimitAngleY, Mathf.Deg2Rad );
					}
					if( upperDirXLimitThetaY._degrees != settingsBodyIK.upperDirXLimitAngleY ) {
						upperDirXLimitThetaY._Reset( settingsBodyIK.upperDirXLimitAngleY );
                    }

					if( upperContinuousPreTranslateRate._value != settingsBodyIK.upperContinuousPreTranslateRate ) {
						upperContinuousPreTranslateRate._Reset( settingsBodyIK.upperContinuousPreTranslateRate );
					}
					if( upperContinuousPreTranslateStableRate._value != settingsBodyIK.upperContinuousPreTranslateStableRate ) {
						upperContinuousPreTranslateStableRate._Reset( settingsBodyIK.upperContinuousPreTranslateStableRate );
					}
					if( upperContinuousCenterLegRotationStableRate._value != settingsBodyIK.upperContinuousCenterLegRotationStableRate ) {
						upperContinuousCenterLegRotationStableRate._Reset( settingsBodyIK.upperContinuousCenterLegRotationStableRate );
					}
					if( upperContinuousPostTranslateStableRate._value != settingsBodyIK.upperContinuousPostTranslateStableRate ) {
						upperContinuousPostTranslateStableRate._Reset( settingsBodyIK.upperContinuousPostTranslateStableRate );
					}
				}
			}

			public class LimbIK
			{
				public CachedDegreesToSin armEffectorBackBeginTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin armEffectorBackCoreBeginTheta = CachedDegreesToSin.zero;
				public CachedDegreesToCos armEffectorBackCoreEndTheta = CachedDegreesToCos.zero;
				public CachedDegreesToCos armEffectorBackEndTheta = CachedDegreesToCos.zero;

				public CachedDegreesToSin armEffectorBackCoreUpperTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin armEffectorBackCoreLowerTheta = CachedDegreesToSin.zero;

				public CachedDegreesToSin elbowFrontInnerLimitTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin elbowBackInnerLimitTheta = CachedDegreesToSin.zero;

				public CachedDegreesToSin footLimitYawTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin footLimitPitchUpTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin footLimitPitchDownTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin footLimitRollTheta = CachedDegreesToSin.zero;

				public void Update( Settings.LimbIK settingsLimbIK )
				{
					Assert( settingsLimbIK != null );

					if( armEffectorBackBeginTheta._degrees != settingsLimbIK.armEffectorBackBeginAngle ) {
						armEffectorBackBeginTheta._Reset( settingsLimbIK.armEffectorBackBeginAngle );
					}
					if( armEffectorBackCoreBeginTheta._degrees != settingsLimbIK.armEffectorBackCoreBeginAngle ) {
						armEffectorBackCoreBeginTheta._Reset( settingsLimbIK.armEffectorBackCoreBeginAngle );
					}
					if( armEffectorBackCoreEndTheta._degrees != settingsLimbIK.armEffectorBackCoreEndAngle ) {
						armEffectorBackCoreEndTheta._Reset( settingsLimbIK.armEffectorBackCoreEndAngle );
					}
					if( armEffectorBackEndTheta._degrees != settingsLimbIK.armEffectorBackEndAngle ) {
						armEffectorBackEndTheta._Reset( settingsLimbIK.armEffectorBackEndAngle );
					}

					if( armEffectorBackCoreUpperTheta._degrees != settingsLimbIK.armEffectorBackCoreUpperAngle ) {
						armEffectorBackCoreUpperTheta._Reset( settingsLimbIK.armEffectorBackCoreUpperAngle );
					}
					if( armEffectorBackCoreLowerTheta._degrees != settingsLimbIK.armEffectorBackCoreLowerAngle ) {
						armEffectorBackCoreLowerTheta._Reset( settingsLimbIK.armEffectorBackCoreLowerAngle );
					}

					if( elbowFrontInnerLimitTheta._degrees != settingsLimbIK.elbowFrontInnerLimitAngle ) {
						elbowFrontInnerLimitTheta._Reset( settingsLimbIK.elbowFrontInnerLimitAngle );
					}
					if( elbowBackInnerLimitTheta._degrees != settingsLimbIK.elbowBackInnerLimitAngle ) {
						elbowBackInnerLimitTheta._Reset( settingsLimbIK.elbowBackInnerLimitAngle );
					}

					if( footLimitYawTheta._degrees != settingsLimbIK.footLimitYaw ) {
						footLimitYawTheta._Reset( settingsLimbIK.footLimitYaw );
					}
					if( footLimitPitchUpTheta._degrees != settingsLimbIK.footLimitPitchUp ) {
						footLimitPitchUpTheta._Reset( settingsLimbIK.footLimitPitchUp );
					}
					if( footLimitPitchDownTheta._degrees != settingsLimbIK.footLimitPitchDown ) {
						footLimitPitchDownTheta._Reset( settingsLimbIK.footLimitPitchDown );
					}
					if( footLimitRollTheta._degrees != settingsLimbIK.footLimitRoll ) {
						footLimitRollTheta._Reset( settingsLimbIK.footLimitRoll );
					}
				}
			}

			public class HeadIK
			{
				public CachedDegreesToSin neckLimitPitchUpTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin neckLimitPitchDownTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin neckLimitRollTheta = CachedDegreesToSin.zero;

				public CachedDegreesToSin headLimitYawTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin headLimitPitchUpTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin headLimitPitchDownTheta = CachedDegreesToSin.zero;
				public CachedDegreesToSin headLimitRollTheta = CachedDegreesToSin.zero;

				public void Update( Settings.HeadIK settingsHeadIK )
				{
					Assert( settingsHeadIK != null );

					if( neckLimitPitchUpTheta._degrees != settingsHeadIK.neckLimitPitchUp ) {
						neckLimitPitchUpTheta._Reset( settingsHeadIK.neckLimitPitchUp );
					}
					if( neckLimitPitchDownTheta._degrees != settingsHeadIK.neckLimitPitchDown ) {
						neckLimitPitchDownTheta._Reset( settingsHeadIK.neckLimitPitchDown );
					}
					if( neckLimitRollTheta._degrees != settingsHeadIK.neckLimitRoll ) {
						neckLimitRollTheta._Reset( settingsHeadIK.neckLimitRoll );
					}

					if( headLimitYawTheta._degrees != settingsHeadIK.headLimitYaw ) {
						headLimitYawTheta._Reset( settingsHeadIK.headLimitYaw );
					}
					if( headLimitPitchUpTheta._degrees != settingsHeadIK.headLimitPitchUp ) {
						headLimitPitchUpTheta._Reset( settingsHeadIK.headLimitPitchUp );
					}
					if( headLimitPitchDownTheta._degrees != settingsHeadIK.headLimitPitchDown ) {
						headLimitPitchDownTheta._Reset( settingsHeadIK.headLimitPitchDown );
					}
					if( headLimitRollTheta._degrees != settingsHeadIK.headLimitRoll ) {
						headLimitRollTheta._Reset( settingsHeadIK.headLimitRoll );
					}
				}
			}

			public BodyIK bodyIK = new BodyIK();
			public LimbIK limbIK = new LimbIK();
			public HeadIK headIK = new HeadIK();
		}
		public class BoneCaches
		{
			public struct HipsToFootLength
			{
				public Vector3 hipsToLeg;
				public Vector3 legToKnee;
				public Vector3 kneeToFoot;

				public Vector3 defaultOffset;
			}

			public HipsToFootLength[] hipsToFootLength = new HipsToFootLength[2];

			void _PrepareHipsToFootLength( int index, Bone legBone, Bone kneeBone, Bone footBone, InternalValues internalValues )
			{
				Assert( internalValues != null );
				if( legBone != null && kneeBone != null && footBone != null ) {
					float hipsToLegLen = legBone._defaultLocalLength.length;
                    float legToKneeLen = kneeBone._defaultLocalLength.length;
					float kneeToFootLen = footBone._defaultLocalLength.length;

					Vector3 hipsToLegDir = legBone._defaultLocalDirection;
                    Vector3 legToKneeDir = kneeBone._defaultLocalDirection;
					Vector3 kneeToFootDir = footBone._defaultLocalDirection;

					FBIKMatMultVec( out hipsToFootLength[index].hipsToLeg, ref internalValues.defaultRootBasisInv, ref hipsToLegDir );
					FBIKMatMultVec( out hipsToFootLength[index].legToKnee, ref internalValues.defaultRootBasisInv, ref legToKneeDir );
					FBIKMatMultVec( out hipsToFootLength[index].kneeToFoot, ref internalValues.defaultRootBasisInv, ref kneeToFootDir );

					hipsToFootLength[index].defaultOffset =
						hipsToFootLength[index].hipsToLeg * hipsToLegLen +
						hipsToFootLength[index].legToKnee * legToKneeLen +
						hipsToFootLength[index].kneeToFoot * kneeToFootLen;
				}
			}

			Vector3 _GetHipsOffset( int index, Bone legBone, Bone kneeBone, Bone footBone )
			{
				if( legBone != null && kneeBone != null && footBone != null ) {
					float hipsToLegLen = legBone._defaultLocalLength.length;
					float legToKneeLen = kneeBone._defaultLocalLength.length;
					float kneeToFootLen = footBone._defaultLocalLength.length;

					Vector3 currentOffset =
						hipsToFootLength[index].hipsToLeg * hipsToLegLen +
						hipsToFootLength[index].legToKnee * legToKneeLen +
						hipsToFootLength[index].kneeToFoot * kneeToFootLen;

					return currentOffset - hipsToFootLength[index].defaultOffset;
				}

				return Vector3.zero;
            }

			public Vector3 defaultHipsPosition = Vector3.zero;
			public Vector3 hipsOffset = Vector3.zero;

			public void Prepare( FullBodyIK fullBodyIK )
			{
				_PrepareHipsToFootLength( 0, fullBodyIK.leftLegBones.leg, fullBodyIK.leftLegBones.knee, fullBodyIK.leftLegBones.foot, fullBodyIK.internalValues );
				_PrepareHipsToFootLength( 1, fullBodyIK.rightLegBones.leg, fullBodyIK.rightLegBones.knee, fullBodyIK.rightLegBones.foot, fullBodyIK.internalValues );
				if( fullBodyIK.bodyBones.hips != null ) {
					defaultHipsPosition = fullBodyIK.bodyBones.hips._defaultPosition;
				}
			}

			public void _SyncDisplacement( FullBodyIK fullBodyIK )
			{
				Assert( fullBodyIK != null );

				Vector3 hipsOffset0 = _GetHipsOffset( 0, fullBodyIK.leftLegBones.leg, fullBodyIK.leftLegBones.knee, fullBodyIK.leftLegBones.foot );
				Vector3 hipsOffset1 = _GetHipsOffset( 1, fullBodyIK.rightLegBones.leg, fullBodyIK.rightLegBones.knee, fullBodyIK.rightLegBones.foot );
				hipsOffset = (hipsOffset0 + hipsOffset1) * 0.5f;
			}
		}

		public Transform rootTransform;

		[System.NonSerialized]
		public InternalValues internalValues = new InternalValues();
		[System.NonSerialized]
		public BoneCaches boneCaches = new BoneCaches();

		public Settings settings;
		public EditorSettings editorSettings;

		public BodyBones bodyBones;
		public HeadBones headBones;
		public LegBones leftLegBones;
		public LegBones rightLegBones;
		public ArmBones leftArmBones;
		public ArmBones rightArmBones;

		public Effector rootEffector;
		public BodyEffectors bodyEffectors;
		public HeadEffectors headEffectors;
		public LegEffectors leftLegEffectors;
		public LegEffectors rightLegEffectors;
		public ArmEffectors leftArmEffectors;
		public ArmEffectors rightArmEffectors;

		public Bone[] bones { get { return _bones; } }
		public Effector[] effectors { get { return _effectors; } }

		Bone[] _bones = new Bone[(int)BoneType.Max];
		Effector[] _effectors = new Effector[(int)EffectorLocation.Max];

		BodyIK _bodyIK;
		LimbIK[] _limbIK = new LimbIK[(int)LimbIKLocation.Max];
		HeadIK _headIK;

		bool _isNeedFixShoulderWorldTransform;

		bool _isPrefixed;
		bool _isPrepared;
		[SerializeField]
		bool _isPrefixedAtLeastOnce;

		public void Awake( Transform rootTransorm_ )
		{
			if( rootTransform != rootTransorm_ ) {
				rootTransform = rootTransorm_;
			}

			_Prefix();
			ConfigureBoneTransforms();
			Prepare();
		}

		public void Destroy()
		{
#if UNITY_EDITOR
			if( _effectors != null ) {
				for( int i = 0; i < _effectors.Length; ++i ) {
					if( _effectors[i] != null && _effectors[i].transform != null ) {
						GameObject.DestroyImmediate( _effectors[i].transform.gameObject );
					}
                }
			}
#endif
		}

		static void _SetBoneTransform( ref Bone bone, Transform transform )
		{
			if( bone == null ) bone = new Bone();
			bone.transform = transform;
		}

		static bool _IsSpine( Transform trn )
		{
			if( trn != null ) {
				string name = trn.name;
				if( name.Contains( "Spine" ) || name.Contains( "spine" ) || name.Contains( "SPINE" ) ) {
					return true;
				}
				if( name.Contains( "Torso" ) || name.Contains( "torso" ) || name.Contains( "TORSO" ) ) {
					return true;
				}
			}

			return false;
		}

		static bool _IsNeck( Transform trn )
		{
			if( trn != null ) {
				string name = trn.name;
				if( name != null ) {
					if( name.Contains( "Neck" ) || name.Contains( "neck" ) || name.Contains( "NECK" ) ) {
						return true;
					}
					if( name.Contains( "Kubi" ) || name.Contains( "kubi" ) || name.Contains( "KUBI" ) ) {
						return true;
					}
					if( name.Contains( "\u304F\u3073" ) ) {
						return true;
					}
					if( name.Contains( "\u30AF\u30D3" ) ) {
						return true;
					}
					if( name.Contains( "\u9996" ) ) {
						return true;
					}
				}
			}

			return false;
		}
		public void Prefix( Transform rootTransform_ )
		{
			if( rootTransform != rootTransform_ ) {
				rootTransform = rootTransform_;
            }

			_Prefix();
		}
		void _Prefix()
		{
			if( _isPrefixed ) {
				return;
			}

			_isPrefixed = true;

			SafeNew( ref bodyBones );
			SafeNew( ref headBones );
			SafeNew( ref leftLegBones );
			SafeNew( ref rightLegBones );

			SafeNew( ref leftArmBones );
			SafeNew( ref rightArmBones );

			SafeNew( ref bodyEffectors );
			SafeNew( ref headEffectors );
			SafeNew( ref leftArmEffectors );
			SafeNew( ref rightArmEffectors );
			SafeNew( ref leftLegEffectors );
			SafeNew( ref rightLegEffectors );

			SafeNew( ref settings );
			SafeNew( ref editorSettings );
			SafeNew( ref internalValues );

			settings.Prefix();

			if( _bones == null || _bones.Length != (int)BoneLocation.Max ) {
				_bones = new Bone[(int)BoneLocation.Max];
			}
			if( _effectors == null || _effectors.Length != (int)EffectorLocation.Max ) {
				_effectors = new Effector[(int)EffectorLocation.Max];
			}

			_Prefix( ref bodyBones.hips, BoneLocation.Hips, null );
			_Prefix( ref bodyBones.spine, BoneLocation.Spine, bodyBones.hips );
			_Prefix( ref bodyBones.spine2, BoneLocation.Spine2, bodyBones.spine );
			_Prefix( ref bodyBones.spine3, BoneLocation.Spine3, bodyBones.spine2 );
			_Prefix( ref bodyBones.spine4, BoneLocation.Spine4, bodyBones.spine3 );
			_Prefix( ref headBones.neck, BoneLocation.Neck, bodyBones.spineU );
			_Prefix( ref headBones.head, BoneLocation.Head, headBones.neck );
			for( int i = 0; i != 2; ++i ) {
				var legBones = (i == 0) ? leftLegBones : rightLegBones;
				_Prefix( ref legBones.leg, (i == 0) ? BoneLocation.LeftLeg : BoneLocation.RightLeg, bodyBones.hips );
				_Prefix( ref legBones.knee, (i == 0) ? BoneLocation.LeftKnee : BoneLocation.RightKnee, legBones.leg );
				_Prefix( ref legBones.foot, (i == 0) ? BoneLocation.LeftFoot : BoneLocation.RightFoot, legBones.knee );

				var armBones = (i == 0) ? leftArmBones : rightArmBones;
				_Prefix(ref armBones.shoulder, (i == 0) ? BoneLocation.LeftShoulder : BoneLocation.RightShoulder, bodyBones.spineU);
				_Prefix(ref armBones.arm, (i == 0) ? BoneLocation.LeftArm : BoneLocation.RightArm, armBones.shoulder);
				_Prefix(ref armBones.elbow, (i == 0) ? BoneLocation.LeftElbow : BoneLocation.RightElbow, armBones.arm);
				_Prefix(ref armBones.wrist, (i == 0) ? BoneLocation.LeftWrist : BoneLocation.RightWrist, armBones.elbow);
			}

			_Prefix( ref rootEffector, EffectorLocation.Root );
			_Prefix( ref bodyEffectors.hips, EffectorLocation.Hips, rootEffector, bodyBones.hips, leftLegBones.leg, rightLegBones.leg );
			_Prefix( ref headEffectors.neck, EffectorLocation.Neck, bodyEffectors.hips, headBones.neck );
			_Prefix( ref headEffectors.head, EffectorLocation.Head, headEffectors.neck, headBones.head );

			_Prefix( ref leftLegEffectors.knee, EffectorLocation.LeftKnee, rootEffector, leftLegBones.knee );
			_Prefix( ref leftLegEffectors.foot, EffectorLocation.LeftFoot, rootEffector, leftLegBones.foot );
			_Prefix( ref rightLegEffectors.knee, EffectorLocation.RightKnee, rootEffector, rightLegBones.knee );
			_Prefix( ref rightLegEffectors.foot, EffectorLocation.RightFoot, rootEffector, rightLegBones.foot );

			_Prefix( ref leftArmEffectors.arm, EffectorLocation.LeftArm, bodyEffectors.hips, leftArmBones.arm );
			_Prefix( ref leftArmEffectors.elbow, EffectorLocation.LeftElbow, bodyEffectors.hips, leftArmBones.elbow );
			_Prefix( ref leftArmEffectors.wrist, EffectorLocation.LeftWrist, bodyEffectors.hips, leftArmBones.wrist );
			_Prefix( ref rightArmEffectors.arm, EffectorLocation.RightArm, bodyEffectors.hips, rightArmBones.arm );
			_Prefix( ref rightArmEffectors.elbow, EffectorLocation.RightElbow, bodyEffectors.hips, rightArmBones.elbow );
			_Prefix( ref rightArmEffectors.wrist, EffectorLocation.RightWrist, bodyEffectors.hips, rightArmBones.wrist );

			_CreateArmRollTransforms();

			if ( !_isPrefixedAtLeastOnce ) {
				_isPrefixedAtLeastOnce = true;
				for( int i = 0; i != _effectors.Length; ++i ) {
					_effectors[i].Prefix();
                }
			}
		}

		public void CleanupBoneTransforms()
		{
			_Prefix();

			if( _bones != null ) {
				for( int i = 0; i < _bones.Length; ++i ) {
					Assert( _bones[i] != null );
					if( _bones[i] != null ) {
						_bones[i].transform = null;
					}
				}
			}
		}

		static readonly string[] _LeftKeywords = new string[]
		{
			"left",
			"_l",
		};

		static readonly string[] _RightKeywords = new string[]
		{
			"right",
			"_r",
		};

		public void ConfigureBoneTransforms()
		{
			_Prefix();

			Assert( settings != null && rootTransform != null );
			if( settings.automaticPrepareHumanoid && rootTransform != null ) 
			{
				Animator animator = rootTransform.GetComponent<Animator>();
				if( animator != null && animator.isHuman ) 
				{
					Transform hips = animator.GetBoneTransform( HumanBodyBones.Hips );
					Transform spine = animator.GetBoneTransform( HumanBodyBones.Spine );
					Transform spine2 = animator.GetBoneTransform( HumanBodyBones.Chest );
					Transform spine3 = null;
					Transform spine4 = null;
					Transform neck = animator.GetBoneTransform( HumanBodyBones.Neck );
					Transform head = animator.GetBoneTransform( HumanBodyBones.Head );
					Transform leftLeg = animator.GetBoneTransform( HumanBodyBones.LeftUpperLeg );
					Transform rightLeg = animator.GetBoneTransform( HumanBodyBones.RightUpperLeg );
					Transform leftKnee = animator.GetBoneTransform( HumanBodyBones.LeftLowerLeg );
					Transform rightKnee = animator.GetBoneTransform( HumanBodyBones.RightLowerLeg );
					Transform leftFoot = animator.GetBoneTransform( HumanBodyBones.LeftFoot );
					Transform rightFoot = animator.GetBoneTransform( HumanBodyBones.RightFoot );
					Transform leftShoulder = animator.GetBoneTransform( HumanBodyBones.LeftShoulder );
					Transform rightShoulder = animator.GetBoneTransform( HumanBodyBones.RightShoulder );
					Transform leftArm = animator.GetBoneTransform( HumanBodyBones.LeftUpperArm );
					Transform rightArm = animator.GetBoneTransform( HumanBodyBones.RightUpperArm );
					Transform leftElbow = animator.GetBoneTransform( HumanBodyBones.LeftLowerArm );
					Transform rightElbow = animator.GetBoneTransform( HumanBodyBones.RightLowerArm );
					Transform leftWrist = animator.GetBoneTransform( HumanBodyBones.LeftHand );
					Transform rightWrist = animator.GetBoneTransform( HumanBodyBones.RightHand );

					if( neck == null ) {
						if( head != null ) {
							Transform t = head.parent;
							if( t != null && _IsNeck( t ) ) {
								neck = t;
							} else {
								neck = head;
							}
						}
					}

					if( settings.automaticConfigureSpineEnabled ) 
					{
						if( spine != null && neck != null ) {
							var spines = new List<Transform>();
							for( Transform trn = neck.parent; trn != null && trn != spine; trn = trn.parent ) {
								if( _IsSpine( trn ) ) {
									spines.Insert( 0, trn );
								}
                            }

							spines.Insert( 0, spine );

							int spineMaxLength = (int)BoneLocation.SpineU - (int)BoneLocation.Spine + 1;
							if( spines.Count > spineMaxLength ) {
								spines.RemoveRange( spineMaxLength, spines.Count - spineMaxLength );
							}

							spine2 = (spines.Count >= 2) ? spines[1] : null;
							spine3 = (spines.Count >= 3) ? spines[2] : null;
							spine4 = (spines.Count >= 4) ? spines[3] : null;
						}
					}

					_SetBoneTransform( ref bodyBones.hips, hips );
					_SetBoneTransform( ref bodyBones.spine, spine );
					_SetBoneTransform( ref bodyBones.spine2, spine2 );
					_SetBoneTransform( ref bodyBones.spine3, spine3 );
					_SetBoneTransform( ref bodyBones.spine4, spine4 );

					_SetBoneTransform( ref headBones.neck, neck );
					_SetBoneTransform( ref headBones.head, head );

					_SetBoneTransform( ref leftLegBones.leg, leftLeg );
					_SetBoneTransform( ref leftLegBones.knee, leftKnee );
					_SetBoneTransform( ref leftLegBones.foot, leftFoot );
					_SetBoneTransform( ref rightLegBones.leg, rightLeg );
					_SetBoneTransform( ref rightLegBones.knee, rightKnee );
					_SetBoneTransform( ref rightLegBones.foot, rightFoot );

					_SetBoneTransform( ref leftArmBones.shoulder, leftShoulder );
					_SetBoneTransform( ref leftArmBones.arm, leftArm );
					_SetBoneTransform( ref leftArmBones.elbow, leftElbow );
					_SetBoneTransform( ref leftArmBones.wrist, leftWrist );
					_SetBoneTransform( ref rightArmBones.shoulder, rightShoulder );
					_SetBoneTransform( ref rightArmBones.arm, rightArm );
					_SetBoneTransform( ref rightArmBones.elbow, rightElbow );
					_SetBoneTransform( ref rightArmBones.wrist, rightWrist );
				}
			}
		}

		void _CreateArmRollTransforms()
		{
			if (this.rootEffector == null || !this.rootEffector.transformIsAlive) return;

			Transform rootEffector = this.rootEffector.transform;
			Vector3 rootPos = rootTransform.position;
			Vector3 playerRight = rootTransform.right;
			Vector3 playerUp = rootTransform.up;
			Vector3 playerForward = rootTransform.forward;

			for (int side = 0; side < 2; side++)
			{
				var armBones = (side == 0) ? leftArmBones : rightArmBones;
				string prefix = (side == 0) ? "Left" : "Right";
				float sideMultiplier = (side == 0) ? -1f : 1f;

				if (armBones.arm != null && armBones.arm.transformIsAlive)
				{
					Vector3 armPos = armBones.arm.worldPosition;
					Vector3 armLocal = armPos - rootPos;
					float armLocalX = Vector3.Dot(armLocal, playerRight);
					float armLocalY = Vector3.Dot(armLocal, playerUp);

					Vector3 armRollPos = rootPos + playerRight * (sideMultiplier * Mathf.Abs(armLocalX) * 1.5f) + playerUp * armLocalY;
					_CreateRollBoneTransform(ref armBones.armRoll, prefix + "ArmRoll", armRollPos, rootEffector);
					BoneLocation armRollLocation = (side == 0) ? BoneLocation.LeftArmRoll : BoneLocation.RightArmRoll;
					_ConfigureRollBone(ref armBones.armRoll, armBones.armRoll.transform, armRollLocation);
				}

				if (armBones.wrist != null && armBones.wrist.transformIsAlive)
				{
					Vector3 wristPos = armBones.wrist.worldPosition;
					Vector3 wristLocal = wristPos - rootPos;
					float wristLocalX = Vector3.Dot(wristLocal, playerRight);
					float wristLocalY = Vector3.Dot(wristLocal, playerUp);

					Vector3 handRollPos = rootPos + playerRight * (sideMultiplier * Mathf.Abs(wristLocalX) * 1.5f) + playerUp * wristLocalY;
					_CreateRollBoneTransform(ref armBones.handRoll, prefix + "HandRoll", handRollPos, rootEffector);
					BoneLocation handRollLocation = (side == 0) ? BoneLocation.LeftHandRoll : BoneLocation.RightHandRoll;
					_ConfigureRollBone(ref armBones.handRoll, armBones.handRoll.transform, handRollLocation);
				}

				if (armBones.elbow != null && armBones.elbow.transformIsAlive)
				{
					_CreateRollBoneTransform(ref armBones.elbowPole, prefix + "ElbowPole", armBones.elbow.worldPosition, rootEffector);
					BoneLocation elbowPoleLocation = (side == 0) ? BoneLocation.LeftElbowPole : BoneLocation.RightElbowPole;
					_ConfigureRollBone(ref armBones.elbowPole, armBones.elbowPole.transform, elbowPoleLocation);
				}
			}
		}

		void _CreateRollBoneTransform(ref Bone bone, string name, Vector3 position, Transform parent)
		{
			if (bone == null || bone.transform == null)
			{
				GameObject go = new GameObject(name);
				go.transform.SetParent(parent, false);
				go.transform.position = position;
				go.transform.rotation = parent.rotation;

				if (bone == null) bone = new Bone();
				bone.transform = go.transform;
			}
		}
		void _ConfigureRollBone(ref Bone bone, Transform transform, BoneLocation boneLocation)
		{
			Bone.Prefix(_bones, ref bone, boneLocation);
			if (transform != null)
			{
				bone.transform = transform;
				bone.Prepare(this);
			}
		}

		public bool Prepare()
		{
			_Prefix();

			if( _isPrepared ) {
				return false;
			}

			_isPrepared = true;

			Assert( rootTransform != null );
			if( rootTransform != null ) {
				internalValues.defaultRootPosition = rootTransform.position;
				internalValues.defaultRootBasis = Matrix3x3.FromColumn( rootTransform.right, rootTransform.up, rootTransform.forward );
				internalValues.defaultRootBasisInv = internalValues.defaultRootBasis.transpose;
				internalValues.defaultRootRotation = rootTransform.rotation;
			}
			
			if( _bones != null ) {
				int boneLength = _bones.Length;
				for( int i = 0; i != boneLength; ++i ) {
					Assert( _bones[i] != null );
					if( _bones[i] != null ) {
						_bones[i].Prepare( this );
					}
				}
				for( int i = 0; i != boneLength; ++i ) {
					if( _bones[i] != null ) {
						_bones[i].PostPrepare();
					}
				}
			}

			boneCaches.Prepare( this );

			if( _effectors != null ) {
				int effectorLength = _effectors.Length;
                for( int i = 0; i != effectorLength; ++i ) {
					Assert( _effectors[i] != null );
					if( _effectors[i] != null ) {
						_effectors[i].Prepare( this );
					}
				}
			}

			if( _limbIK == null || _limbIK.Length != (int)LimbIKLocation.Max ) {
				_limbIK = new LimbIK[(int)LimbIKLocation.Max];
			}

			for( int i = 0; i != (int)LimbIKLocation.Max; ++i ) {
				_limbIK[i] = new LimbIK( this, (LimbIKLocation)i );
			}

			_bodyIK = new BodyIK( this, _limbIK );
			_headIK = new HeadIK( this );

			{
				Bone neckBone = headBones.neck;
				Bone leftShoulder = leftArmBones.shoulder;
				Bone rightShoulder = rightArmBones.shoulder;
				if( leftShoulder != null && leftShoulder.transformIsAlive &&
					rightShoulder != null && rightShoulder.transformIsAlive &&
					neckBone != null && neckBone.transformIsAlive ) {
					if( leftShoulder.transform.parent == neckBone.transform &&
						rightShoulder.transform.parent == neckBone.transform ) {
						_isNeedFixShoulderWorldTransform = true;
					}
				}
			}

			return true;
        }

		bool _isAnimatorCheckedAtLeastOnce = false;

		void _UpdateInternalValues()
		{
			if( settings.animatorEnabled == AutomaticBool.Auto ) {
				if( !_isAnimatorCheckedAtLeastOnce ) {
					_isAnimatorCheckedAtLeastOnce = true;
					internalValues.animatorEnabled = false;
					if( rootTransform != null ) {
						var animator = rootTransform.GetComponent<Animator>();
						if( animator != null && animator.enabled ) {
							var runtimeAnimatorController = animator.runtimeAnimatorController;
							internalValues.animatorEnabled = (runtimeAnimatorController != null);
						}
						if( animator == null ) {
							var animation = rootTransform.GetComponent<Animation>();
							if( animation != null && animation.enabled && animation.GetClipCount() > 0 ) {
								internalValues.animatorEnabled = true;
							}
						}
					}
				}
			} else {
				internalValues.animatorEnabled = (settings.animatorEnabled != AutomaticBool.Disable);
				_isAnimatorCheckedAtLeastOnce = false;
			}

			if( settings.resetTransforms == AutomaticBool.Auto ) {
				internalValues.resetTransforms = !(internalValues.animatorEnabled);
			} else {
				internalValues.resetTransforms = (settings.resetTransforms != AutomaticBool.Disable);
			}

			internalValues.continuousSolverEnabled = !internalValues.animatorEnabled && !internalValues.resetTransforms;

			internalValues.bodyIK.Update( settings.bodyIK );
			internalValues.limbIK.Update( settings.limbIK );
			internalValues.headIK.Update( settings.headIK );
        }

		bool _isSyncDisplacementAtLeastOnce = false;

		void _Bones_SyncDisplacement()
		{
			if( settings.syncDisplacement != SyncDisplacement.Disable ) {
				if( settings.syncDisplacement == SyncDisplacement.Everyframe || !_isSyncDisplacementAtLeastOnce ) {
					_isSyncDisplacementAtLeastOnce = true;

					if( _bones != null ) {
						int boneLength = _bones.Length;
						for( int i = 0; i != boneLength; ++i ) {
							if( _bones[i] != null ) {
								_bones[i].SyncDisplacement();
							}
						}
						boneCaches._SyncDisplacement( this );

						for( int i = 0; i != boneLength; ++i ) {
							if( _bones[i] != null ) {
								_bones[i].PostSyncDisplacement( this );
							}
						}

						for( int i = 0; i != boneLength; ++i ) {
							if( _bones[i] != null ) {
								_bones[i].PostPrepare();
							}
						}
					}
					if( _effectors != null ) {
						int effectorLength = _effectors.Length;
						for( int i = 0; i != effectorLength; ++i ) {
							if( _effectors[i] != null ) {
								_effectors[i]._ComputeDefaultTransform( this );
                            }
						}
					}
                }
			}
		}
		void _ComputeBaseHipsTransform()
		{
			Assert( internalValues != null );

			if( bodyEffectors == null ) {
				return;
			}

			Effector hipsEffector = bodyEffectors.hips;
			if( hipsEffector == null || rootEffector == null ) {
				return;
			}

			if( hipsEffector.rotationEnabled && hipsEffector.rotationWeight > IKEpsilon ) {
				Quaternion hipsRotation = hipsEffector.worldRotation * Inverse( hipsEffector._defaultRotation );
				if( hipsEffector.rotationWeight < 1.0f - IKEpsilon ) {
					Quaternion rootRotation = rootEffector.worldRotation * Inverse( rootEffector._defaultRotation );
					Quaternion tempRotation = Quaternion.Lerp( rootRotation, hipsRotation, hipsEffector.rotationWeight );
					FBIKMatSetRot( out internalValues.baseHipsBasis, ref tempRotation );
				} else {
					FBIKMatSetRot( out internalValues.baseHipsBasis, ref hipsRotation );
				}
			} else {
				Quaternion rootEffectorWorldRotation = rootEffector.worldRotation;
				FBIKMatSetRotMultInv1( out internalValues.baseHipsBasis, ref rootEffectorWorldRotation, ref rootEffector._defaultRotation );
			}

			if( hipsEffector.positionEnabled && hipsEffector.positionWeight > IKEpsilon ) {
				Vector3 hipsEffectorWorldPosition = hipsEffector.worldPosition;
				internalValues.baseHipsPos = hipsEffectorWorldPosition;
				if( hipsEffector.positionWeight < 1.0f - IKEpsilon ) {
					Vector3 rootEffectorWorldPosition = rootEffector.worldPosition;
					Vector3 hipsPosition;
					FBIKMatMultVecPreSubAdd(
						out hipsPosition,
						ref internalValues.baseHipsBasis,
						ref hipsEffector._defaultPosition,
						ref rootEffector._defaultPosition,
						ref rootEffectorWorldPosition );
					internalValues.baseHipsPos = Vector3.Lerp( hipsPosition, internalValues.baseHipsPos, hipsEffector.positionWeight );
				}
			} else {
				Vector3 rootEffectorWorldPosition = rootEffector.worldPosition;
				FBIKMatMultVecPreSubAdd(
					out internalValues.baseHipsPos,
					ref internalValues.baseHipsBasis,
					ref hipsEffector._defaultPosition,
					ref rootEffector._defaultPosition,
					ref rootEffectorWorldPosition );
			}
		}

		public void Update()
		{
			_UpdateInternalValues();

			if( _effectors != null ) {
				int effectorLength = _effectors.Length;
                for( int i = 0; i != effectorLength; ++i ) {
					if( _effectors[i] != null ) {
						_effectors[i].PrepareUpdate();
					}
				}
			}

			_Bones_PrepareUpdate();

			_Bones_SyncDisplacement();

			if( internalValues.resetTransforms || internalValues.continuousSolverEnabled ) {
				_ComputeBaseHipsTransform();
            }
			if( _effectors != null ) {
				int effectorLength = _effectors.Length;
				for( int i = 0; i != effectorLength; ++i ) {
					Effector effector = _effectors[i];
					if( effector != null ) 
					{
						float weight = effector.positionEnabled ? effector.positionWeight : 0.0f;
						Vector3 destPosition = (weight > IKEpsilon) ? effector.worldPosition : new Vector3();
						if (weight < 1.0f - IKEpsilon)
						{
							Vector3 sourcePosition = destPosition;
							if (!internalValues.animatorEnabled && (internalValues.resetTransforms || internalValues.continuousSolverEnabled))
							{
								if (effector.effectorLocation == EffectorLocation.Hips)
								{
									sourcePosition = internalValues.baseHipsPos;
								}
								else
								{
									Effector hipsEffector = (bodyEffectors != null) ? bodyEffectors.hips : null;
									if (hipsEffector != null)
									{
										FBIKMatMultVecPreSubAdd(
											out sourcePosition,
											ref internalValues.baseHipsBasis,
											ref effector._defaultPosition,
											ref hipsEffector._defaultPosition,
											ref internalValues.baseHipsPos);
									}
								}
							}
							else
							{
								if (effector.bone != null && effector.bone.transformIsAlive)
								{
									sourcePosition = effector.bone.worldPosition;
								}
							}

							if (weight > IKEpsilon)
							{
								effector._hidden_worldPosition = Vector3.Lerp(sourcePosition, destPosition, weight);
							}
							else
							{
								effector._hidden_worldPosition = sourcePosition;
							}
						}
						else
						{
							effector._hidden_worldPosition = destPosition;
						}
					}
				}
			}
			if( _limbIK != null ) {
				int limbIKLength = _limbIK.Length;
                for( int i = 0; i != limbIKLength; ++i ) {
					if( _limbIK[i] != null ) {
						_limbIK[i].PresolveBending();
					}
				}
			}

			if( _bodyIK != null ) {
				if( _bodyIK.Solve() ) {
					_Bones_WriteToTransform();
				}
			}

			if( _limbIK != null || _headIK != null ) {
				_Bones_PrepareUpdate();

				bool isSolved = false;
				bool isHeadSolved = false;
				if( _limbIK != null ) {
					int limbIKLength = _limbIK.Length;
					for( int i = 0; i != limbIKLength; ++i ) {
						if( _limbIK[i] != null ) {
							isSolved |= _limbIK[i].Solve();
						}
					}
				}
				if( _headIK != null ) {
					isHeadSolved = _headIK.Solve( this );
					isSolved |= isHeadSolved;
                }

				if( isHeadSolved && _isNeedFixShoulderWorldTransform ) {
					if( leftArmBones.shoulder != null ) {
						leftArmBones.shoulder.forcefix_worldRotation();
					}
					if( rightArmBones.shoulder != null ) {
						rightArmBones.shoulder.forcefix_worldRotation();
					}
				}

				if( isSolved ) {
					_Bones_WriteToTransform();
				}
			}
		}

		void _Bones_PrepareUpdate()
		{
			if( _bones != null ) {
				int boneLength = _bones.Length;
                for( int i = 0; i != boneLength; ++i ) {
					if( _bones[i] != null ) {
						_bones[i].PrepareUpdate();
					}
				}
			}
		}

		void _Bones_WriteToTransform()
		{
			if( _bones != null ) {
				int boneLength = _bones.Length;
				for( int i = 0; i != boneLength; ++i ) {
					if( _bones[i] != null ) {
						_bones[i].WriteToTransform();
					}
				}
			}
		}

#if UNITY_EDITOR
		public void DrawGizmos()
		{
			Vector3 cameraForward = Camera.current.transform.forward;

			if( _effectors != null ) {
				int effectorLength = _effectors.Length;
                for( int i = 0; i != effectorLength; ++i ) {
					_DrawEffectorGizmo( _effectors[i] );
				}
			}

			if( _bones != null ) {
				int boneLength = _bones.Length;
                for( int i = 0; i != boneLength; ++i ) {
					_DrawBoneGizmo( _bones[i], ref cameraForward );
				}
			}

		}

		const float _EffectorGizmoSize = 0.04f;

		static void _DrawEffectorGizmo( Effector effector )
		{
			if( effector != null ) {
				float effectorSize = _EffectorGizmoSize;
				Gizmos.color = Color.green;

				Vector3 position = Vector3.zero;
				if( effector.transform != null ) {
					position = effector.transform.position;
				} else {
					position = effector._worldPosition;
				}

				Gizmos.DrawWireSphere( position, effectorSize );
				Gizmos.DrawWireSphere( position, effectorSize );
				Gizmos.DrawWireSphere( position, effectorSize );
				Gizmos.DrawWireSphere( position, effectorSize );
			}
		}

		void _DrawBoneGizmo( Bone bone, ref Vector3 cameraForward )
		{
			if( bone == null || bone.transform == null ) {
				return;
			}

			Transform parentTransform = bone.parentTransform;

			Vector3 position = bone.transform.position;
			Quaternion rotation = bone.transform.rotation * bone._worldToBoneRotation;
			Matrix3x3 basis;
			FBIKMatSetRot( out basis, ref rotation );

			_DrawTransformGizmo( position, ref basis, ref cameraForward, bone.boneType );

			if( parentTransform != null ) {
				Gizmos.color = Color.white;

				rotation = parentTransform.rotation * bone.parentBone._worldToBoneRotation;
				FBIKMatSetRot( out basis, ref rotation );

				_DrawBoneGizmo( parentTransform.position, position, ref basis, ref cameraForward, bone.boneType );
			}
		}

		static void _DrawTransformGizmo( Vector3 position, ref Matrix3x3 basis, ref Vector3 cameraForward, BoneType boneType )
		{
			Vector3 column0 = basis.column0;
			Vector3 column1 = basis.column1;
			Vector3 column2 = basis.column2;
			_DrawArrowGizmo( Color.red, ref position, ref column0, ref cameraForward, boneType );
			_DrawArrowGizmo( Color.green, ref position, ref column1, ref cameraForward, boneType );
			_DrawArrowGizmo( Color.blue, ref position, ref column2, ref cameraForward, boneType );
		}

		const float _ArrowGizmoLowerLength = 0.02f;
		const float _ArrowGizmoLowerWidth = 0.003f;
		const float _ArrowGizmoMiddleLength = 0.0025f;
		const float _ArrowGizmoMiddleWidth = 0.0075f;
		const float _ArrowGizmoUpperLength = 0.05f;
		const float _ArrowGizmoThickness = 0.0002f;
		const int _ArrowGizmoDrawCycles = 8;

		static void _DrawArrowGizmo(
			Color color,
			ref Vector3 position,
			ref Vector3 direction,
			ref Vector3 cameraForward,
			BoneType boneType )
		{
			Vector3 nY = Vector3.Cross( cameraForward, direction );
			if( FBIKVecNormalize( ref nY ) ) {
				Gizmos.color = color;

				float arrowGizmoLowerLength = _ArrowGizmoLowerLength;
				float arrowGizmoLowerWidth = _ArrowGizmoLowerWidth;
				float arrowGizmoMiddleLength = _ArrowGizmoMiddleLength;
				float arrowGizmoMiddleWidth = _ArrowGizmoMiddleWidth;
				float arrowGizmoUpperLength = _ArrowGizmoUpperLength;
				float arrowGizmoThickness = _ArrowGizmoThickness;

				Vector3 posLower = position + direction * arrowGizmoLowerLength;
				Vector3 posMiddle = position + direction * (arrowGizmoLowerLength + arrowGizmoMiddleLength);
				Vector3 posUpper = position + direction * (arrowGizmoLowerLength + arrowGizmoMiddleLength + arrowGizmoUpperLength);

				Vector3 lowerY = nY * arrowGizmoLowerWidth;
				Vector3 middleY = nY * arrowGizmoMiddleWidth;
				Vector3 thicknessY = nY * arrowGizmoThickness;

				for( int i = 0; i < _ArrowGizmoDrawCycles; ++i ) {
					Gizmos.DrawLine( position, posLower + lowerY );
					Gizmos.DrawLine( position, posLower - lowerY );
					Gizmos.DrawLine( posLower + lowerY, posMiddle + middleY );
					Gizmos.DrawLine( posLower - lowerY, posMiddle - middleY );
					Gizmos.DrawLine( posMiddle + middleY, posUpper );
					Gizmos.DrawLine( posMiddle - middleY, posUpper );
					lowerY -= thicknessY;
					middleY -= thicknessY;
				}
			}
		}

		const float _BoneGizmoOuterLen = 0.015f;
		const float _BoneGizmoThickness = 0.0003f;
		const int _BoneGizmoDrawCycles = 8;

		static void _DrawBoneGizmo( Vector3 fromPosition, Vector3 toPosition, ref Matrix3x3 basis, ref Vector3 cameraForward, BoneType boneType )
		{
			Vector3 dir = toPosition - fromPosition;
			if( FBIKVecNormalize( ref dir ) ) {
				Vector3 nY = Vector3.Cross( cameraForward, dir );
				if( FBIKVecNormalize( ref nY ) ) {
					float boneGizmoOuterLen = _BoneGizmoOuterLen;
					float boneGizmoThickness = _BoneGizmoThickness;

					Vector3 outerY = nY * boneGizmoOuterLen;
					Vector3 thicknessY = nY * boneGizmoThickness;
					Vector3 interPosition = fromPosition + dir * boneGizmoOuterLen;

					for( int i = 0; i < _BoneGizmoDrawCycles; ++i ) {
						Gizmos.color = (i < _BoneGizmoDrawCycles / 2) ? Color.black : Color.white;
						Gizmos.DrawLine( fromPosition, interPosition + outerY );
						Gizmos.DrawLine( fromPosition, interPosition - outerY );
						Gizmos.DrawLine( interPosition + outerY, toPosition );
						Gizmos.DrawLine( interPosition - outerY, toPosition );
						outerY -= thicknessY;
					}
				}
			}
		}
#endif

		void _Prefix( ref Bone bone, BoneLocation boneLocation, Bone parentBoneLocationBased )
		{
			Assert( _bones != null );
			Bone.Prefix( _bones, ref bone, boneLocation, parentBoneLocationBased );
		}

		void _Prefix(
			ref Effector effector,
			EffectorLocation effectorLocation )
		{
			Assert( _effectors != null );
			bool createEffectorTransform = this.settings.createEffectorTransform;
			Assert( rootTransform != null );
			Effector.Prefix( _effectors, ref effector, effectorLocation, createEffectorTransform, rootTransform );
		}

		void _Prefix(
			ref Effector effector,
			EffectorLocation effectorLocation,
			Effector parentEffector,
			Bone[] bones )
		{
			_Prefix( ref effector, effectorLocation, parentEffector, (bones != null && bones.Length > 0) ? bones[bones.Length - 1] : null );
		}

		void _Prefix(
			ref Effector effector,
			EffectorLocation effectorLocation,
			Effector parentEffector,
			Bone bone,
			Bone leftBone = null,
			Bone rightBone = null )
		{
			Assert( _effectors != null );
			bool createEffectorTransform = this.settings.createEffectorTransform;
			Effector.Prefix( _effectors, ref effector, effectorLocation, createEffectorTransform, null, parentEffector, bone, leftBone, rightBone );
		}
	}
}
