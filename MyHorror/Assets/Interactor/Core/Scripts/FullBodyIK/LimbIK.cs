using UnityEngine;

namespace razz
{
	public partial class FullBodyIK
	{
		public class LimbIK
		{
			Settings _settings;
			InternalValues _internalValues;
			FullBodyIK _fullbodyik;

			public LimbIKLocation _limbIKLocation;
			LimbIKType _limbIKType;
			Side _limbIKSide;

			Bone _beginBone;
			Bone _bendingBone;
			Bone _endBone;
			Effector _bendingEffector;
			Effector _endEffector;

			Bone _armRollBone;
			Bone _handRollBone;
			Bone _elbowPoleBone;

			public float _beginToBendingLength;
			public float _beginToBendingLengthSq;
			public float _bendingToEndLength;
			public float _bendingToEndLengthSq;

			Matrix3x3 _beginToBendingBoneBasis = Matrix3x3.identity;
			Quaternion _endEffectorToWorldRotation = Quaternion.identity;

			Matrix3x3 _effectorToBeginBoneBasis = Matrix3x3.identity;
			float _defaultSinTheta = 0.0f;
			float _defaultCosTheta = 1.0f;

			float _beginToEndMaxLength = 0.0f;
			CachedScaledValue _effectorMaxLength = CachedScaledValue.zero;
			CachedScaledValue _effectorMinLength = CachedScaledValue.zero;

			float _leg_upperLimitNearCircleZ = 0.0f;
			float _leg_upperLimitNearCircleY = 0.0f;

			CachedScaledValue _arm_elbowBasisForcefixEffectorLengthBegin = CachedScaledValue.zero;
			CachedScaledValue _arm_elbowBasisForcefixEffectorLengthEnd = CachedScaledValue.zero;
			Matrix3x3 _arm_bendingToBeginBoneBasis = Matrix3x3.identity;
			Quaternion _arm_bendingWorldToBeginBoneRotation = Quaternion.identity;
			Quaternion _arm_endWorldToBendingBoneRotation = Quaternion.identity;
			bool _arm_isSolvedLimbIK;
			Matrix3x3 _arm_solvedBeginBoneBasis = Matrix3x3.identity;
			Matrix3x3 _arm_solvedBendingBoneBasis = Matrix3x3.identity; 
			
			bool _isSyncDisplacementAtLeastOnce;

            float _cache_legUpperLimitAngle = 0.0f;
            float _cache_kneeUpperLimitAngle = 0.0f;

            const float _LocalDirMaxTheta = 0.99f;
            const float _LocalDirLerpTheta = 0.01f;

            CachedDegreesToCos _presolvedLerpTheta = CachedDegreesToCos.zero;
            CachedDegreesToCos _automaticKneeBaseTheta = CachedDegreesToCos.zero;
            CachedDegreesToCosSin _automaticArmElbowTheta = CachedDegreesToCosSin.zero;

			Transform _leftHandTransform;
			Transform _rightHandTransform;

            public LimbIK( FullBodyIK fullBodyIK, LimbIKLocation limbIKLocation )
			{
				Assert( fullBodyIK != null );
				if( fullBodyIK == null ) {
					return;
				}

				_settings = fullBodyIK.settings;
				_internalValues = fullBodyIK.internalValues;
				_fullbodyik = fullBodyIK;

				_leftHandTransform = fullBodyIK.leftArmBones.wrist.transform;
				_rightHandTransform = fullBodyIK.rightArmBones.wrist.transform;

				_limbIKLocation = limbIKLocation;
				_limbIKType = FullBodyIK.ToLimbIKType( limbIKLocation );
				_limbIKSide = FullBodyIK.ToLimbIKSide( limbIKLocation );

				if( _limbIKType == LimbIKType.Leg )
				{
					var legBones = (_limbIKSide == Side.Left) ? fullBodyIK.leftLegBones : fullBodyIK.rightLegBones;
					var legEffectors = (_limbIKSide == Side.Left) ? fullBodyIK.leftLegEffectors : fullBodyIK.rightLegEffectors;
					_beginBone = legBones.leg;
					_bendingBone = legBones.knee;
					_endBone = legBones.foot;
					_bendingEffector = legEffectors.knee;
					_endEffector = legEffectors.foot;
				}
				else if( _limbIKType == LimbIKType.Arm )
				{
					var armBones = (_limbIKSide == Side.Left) ? fullBodyIK.leftArmBones : fullBodyIK.rightArmBones;
					var armEffectors = (_limbIKSide == Side.Left) ? fullBodyIK.leftArmEffectors : fullBodyIK.rightArmEffectors;
					_beginBone = armBones.arm;
					_bendingBone = armBones.elbow;
					_endBone = armBones.wrist;
					_bendingEffector = armEffectors.elbow;
					_endEffector = armEffectors.wrist;

					_armRollBone = armBones.armRoll;
					_handRollBone = armBones.handRoll;
					_elbowPoleBone = armBones.elbowPole;
				}

				_Prepare();
			}

			void _Prepare()
			{
				FBIKQuatMultInv0( out _endEffectorToWorldRotation, ref _endEffector._defaultRotation, ref _endBone._defaultRotation );
				_beginToBendingLength = _bendingBone._defaultLocalLength.length;
				_beginToBendingLengthSq = _bendingBone._defaultLocalLength.lengthSq;
				_bendingToEndLength = _endBone._defaultLocalLength.length;
				_bendingToEndLengthSq = _endBone._defaultLocalLength.lengthSq;

				float beginToEndLength, beginToEndLengthSq;
				beginToEndLength = FBIKVecLengthAndLengthSq2( out beginToEndLengthSq,
					ref _endBone._defaultPosition, ref _beginBone._defaultPosition );

				_defaultCosTheta = ComputeCosTheta(
					_bendingToEndLengthSq,
					beginToEndLengthSq,
					_beginToBendingLengthSq,
					beginToEndLength,
					_beginToBendingLength );

				_defaultSinTheta = FBIKSqrtClamp01( 1.0f - _defaultCosTheta * _defaultCosTheta );
			}

            void _SyncDisplacement()
			{
				if( _settings.syncDisplacement == SyncDisplacement.Everyframe || !_isSyncDisplacementAtLeastOnce ) {
					_isSyncDisplacementAtLeastOnce = true;

					FBIKMatMult( out _beginToBendingBoneBasis, ref _beginBone._localAxisBasisInv, ref _bendingBone._localAxisBasis );

					if (_beginBone != null && _bendingBone != null)
					{
						FBIKMatMult(out _arm_bendingToBeginBoneBasis, ref _bendingBone._boneToBaseBasis, ref _beginBone._baseToBoneBasis);
						FBIKMatMultGetRot(out _arm_bendingWorldToBeginBoneRotation, ref _bendingBone._worldToBaseBasis, ref _beginBone._baseToBoneBasis);
					}

					if (_endBone != null && _bendingBone != null)
					{
						FBIKMatMultGetRot(out _arm_endWorldToBendingBoneRotation, ref _endBone._worldToBaseBasis, ref _bendingBone._baseToBoneBasis);
					}

					_beginToBendingLength	= _bendingBone._defaultLocalLength.length;
					_beginToBendingLengthSq	= _bendingBone._defaultLocalLength.lengthSq;
					_bendingToEndLength		= _endBone._defaultLocalLength.length;
					_bendingToEndLengthSq	= _endBone._defaultLocalLength.lengthSq;
					_beginToEndMaxLength	= _beginToBendingLength + _bendingToEndLength;

					Vector3 beginToEndDir = _endBone._defaultPosition - _beginBone._defaultPosition;
					if( FBIKVecNormalize( ref beginToEndDir ) ) {
						if( _limbIKType == LimbIKType.Arm ) {
							if( _limbIKSide == Side.Left ) {
								beginToEndDir = -beginToEndDir;
							}
							Vector3 dirY = _internalValues.defaultRootBasis.column1;
							Vector3 dirZ = _internalValues.defaultRootBasis.column2;
							if( FBIKComputeBasisLockX( out _effectorToBeginBoneBasis, ref beginToEndDir, ref dirY, ref dirZ ) ) {
								_effectorToBeginBoneBasis = _effectorToBeginBoneBasis.transpose;
							}
						} else {
							beginToEndDir = -beginToEndDir;
							Vector3 dirX = _internalValues.defaultRootBasis.column0;
							Vector3 dirZ = _internalValues.defaultRootBasis.column2;
							if( FBIKComputeBasisLockY( out _effectorToBeginBoneBasis, ref dirX, ref beginToEndDir, ref dirZ ) ) {
								_effectorToBeginBoneBasis = _effectorToBeginBoneBasis.transpose;
							}
						}
						FBIKMatMultRet0( ref _effectorToBeginBoneBasis, ref _beginBone._localAxisBasis );
					}

					if( _limbIKType == LimbIKType.Leg ) {
						_leg_upperLimitNearCircleZ = 0.0f;
						_leg_upperLimitNearCircleY = _beginToEndMaxLength;
					}
					_SyncDisplacement_UpdateArgs();
                }
			}

			void _UpdateArgs()
			{
				if( _limbIKType == LimbIKType.Leg ) {
					float effectorMinLengthRate = _settings.limbIK.legEffectorMinLengthRate;
                    if( _effectorMinLength._b != effectorMinLengthRate ) {
						_effectorMinLength._Reset( _beginToEndMaxLength, effectorMinLengthRate );
					}

					if( _cache_kneeUpperLimitAngle != _settings.limbIK.prefixKneeUpperLimitAngle ||
						_cache_legUpperLimitAngle != _settings.limbIK.prefixLegUpperLimitAngle ) 
					{
						_cache_kneeUpperLimitAngle = _settings.limbIK.prefixKneeUpperLimitAngle;
						_cache_legUpperLimitAngle = _settings.limbIK.prefixLegUpperLimitAngle;
						CachedDegreesToCosSin kneeUpperLimitTheta = new CachedDegreesToCosSin( _settings.limbIK.prefixKneeUpperLimitAngle );
						CachedDegreesToCosSin legUpperLimitTheta = new CachedDegreesToCosSin( _settings.limbIK.prefixLegUpperLimitAngle );

						_leg_upperLimitNearCircleZ = _beginToBendingLength * legUpperLimitTheta.cos
													+ _bendingToEndLength * kneeUpperLimitTheta.cos;

						_leg_upperLimitNearCircleY = _beginToBendingLength * legUpperLimitTheta.sin
													+ _bendingToEndLength * kneeUpperLimitTheta.sin;
					}
				}

				if( _limbIKType == LimbIKType.Arm ) 
				{
					float beginRate = _settings.limbIK.armBasisForcefixEffectorLengthRate - _settings.limbIK.armBasisForcefixEffectorLengthLerpRate;
					float endRate = _settings.limbIK.armBasisForcefixEffectorLengthRate;
					if( _arm_elbowBasisForcefixEffectorLengthBegin._b != beginRate ) {
						_arm_elbowBasisForcefixEffectorLengthBegin._Reset( _beginToEndMaxLength, beginRate );
                    }
					if( _arm_elbowBasisForcefixEffectorLengthEnd._b != endRate ) {
						_arm_elbowBasisForcefixEffectorLengthEnd._Reset( _beginToEndMaxLength, endRate );
					}
				}

				float effectorMaxLengthRate = (_limbIKType == LimbIKType.Leg) ? _settings.limbIK.legEffectorMaxLengthRate : _settings.limbIK.armEffectorMaxLengthRate;
				if( _effectorMaxLength._b != effectorMaxLengthRate ) {
					_effectorMaxLength._Reset( _beginToEndMaxLength, effectorMaxLengthRate );
				}
			}

			void _SyncDisplacement_UpdateArgs()
			{
				if( _limbIKType == LimbIKType.Leg ) {
					float effectorMinLengthRate = _settings.limbIK.legEffectorMinLengthRate;
					_effectorMinLength._Reset( _beginToEndMaxLength, effectorMinLengthRate );
					CachedDegreesToCosSin kneeUpperLimitTheta = new CachedDegreesToCosSin( _settings.limbIK.prefixKneeUpperLimitAngle );
					CachedDegreesToCosSin legUpperLimitTheta = new CachedDegreesToCosSin( _settings.limbIK.prefixLegUpperLimitAngle );

					_leg_upperLimitNearCircleZ = _beginToBendingLength * legUpperLimitTheta.cos
												+ _bendingToEndLength * kneeUpperLimitTheta.cos;

					_leg_upperLimitNearCircleY = _beginToBendingLength * legUpperLimitTheta.sin
												+ _bendingToEndLength * kneeUpperLimitTheta.sin;
				}

				float effectorMaxLengthRate = (_limbIKType == LimbIKType.Leg) ? _settings.limbIK.legEffectorMaxLengthRate : _settings.limbIK.armEffectorMaxLengthRate;
				_effectorMaxLength._Reset( _beginToEndMaxLength, effectorMaxLengthRate );
			}

			void _SolveBaseBasis( out Matrix3x3 baseBasis, ref Matrix3x3 parentBaseBasis, ref Vector3 effectorDir )
			{
				if( _limbIKType == LimbIKType.Arm ) {
					Vector3 dirX = (_limbIKSide == Side.Left) ? -effectorDir : effectorDir;
					Vector3 basisY = parentBaseBasis.column1;
					Vector3 basisZ = parentBaseBasis.column2;
					if( FBIKComputeBasisLockX( out baseBasis, ref dirX, ref basisY, ref basisZ ) ) {
						FBIKMatMultRet0( ref baseBasis, ref _effectorToBeginBoneBasis );
					} else {
						FBIKMatMult( out baseBasis, ref parentBaseBasis, ref _beginBone._localAxisBasis );
                    }
				} else {
					Vector3 dirY = -effectorDir;
					Vector3 basisX = parentBaseBasis.column0;
					Vector3 basisZ = parentBaseBasis.column2;
					if( FBIKComputeBasisLockY( out baseBasis, ref basisX, ref dirY, ref basisZ ) ) {
						FBIKMatMultRet0( ref baseBasis, ref _effectorToBeginBoneBasis );
                    } else {
						FBIKMatMult( out baseBasis, ref parentBaseBasis, ref _beginBone._localAxisBasis );
                    }
				}
			}

			public void PresolveBending()
			{
				_SyncDisplacement();

				bool presolvedEnabled = (_limbIKType == LimbIKType.Leg) ? _settings.limbIK.presolveKneeEnabled : _settings.limbIK.presolveElbowEnabled;
				if( !presolvedEnabled ) return;

				if( _beginBone == null ||
					!_beginBone.transformIsAlive ||
					_beginBone.parentBone == null ||
					!_beginBone.parentBone.transformIsAlive ||
					_bendingEffector == null ||
					_bendingEffector.bone == null ||
					!_bendingEffector.bone.transformIsAlive ||
					_endEffector == null ||
					_endEffector.bone == null ||
					!_endEffector.bone.transformIsAlive ) return;

				if( !_internalValues.animatorEnabled ) return;

				if( _bendingEffector.positionEnabled ) return;

				if( _limbIKType == LimbIKType.Leg ) 
				{
					if( _settings.limbIK.presolveKneeRate < IKEpsilon ) return;
				}
				else if( _settings.limbIK.presolveElbowRate < IKEpsilon ) return;

				Vector3 beginPos = _beginBone.worldPosition;
				Vector3 bendingPos = _bendingEffector.bone.worldPosition;
				Vector3 effectorPos = _endEffector.bone.worldPosition;
				Vector3 effectorTrans = effectorPos - beginPos;
				Vector3 bendingTrans = bendingPos - beginPos;

				float effectorLen = effectorTrans.magnitude;
				float bendingLen = bendingTrans.magnitude;
				if( effectorLen <= IKEpsilon || bendingLen <= IKEpsilon ) return;
			}

			bool _PrefixLegEffectorPos_UpperNear( ref Vector3 localEffectorTrans )
			{
				float y = localEffectorTrans.y - _leg_upperLimitNearCircleY;
				float z = localEffectorTrans.z;

				float rZ = _leg_upperLimitNearCircleZ;
                float rY = _leg_upperLimitNearCircleY + _effectorMinLength.value;

				if( rZ > IKEpsilon && rY > IKEpsilon ) {
					bool isLimited = false;

					z /= rZ;
					if( y > _leg_upperLimitNearCircleY ) {
						isLimited = true;
					} else {
						y /= rY;
						float len = FBIKSqrt( y * y + z * z );
						if( len < 1.0f ) {
							isLimited = true;
						}
					}

					if( isLimited ) {
						float n = FBIKSqrt( 1.0f - z * z );
						if( n > IKEpsilon ) {
							localEffectorTrans.y = -n * rY + _leg_upperLimitNearCircleY;
						} else {
							localEffectorTrans.z = 0.0f;
							localEffectorTrans.y = -_effectorMinLength.value;
						}
						return true;
					}
				}

				return false;
			}

			static bool _PrefixLegEffectorPos_Circular_Far( ref Vector3 localEffectorTrans, float effectorLength )
			{
				return _PrefixLegEffectorPos_Circular( ref localEffectorTrans, effectorLength, true );
            }

			static bool _PrefixLegEffectorPos_Circular( ref Vector3 localEffectorTrans, float effectorLength, bool isFar )
			{
				float y = localEffectorTrans.y;
				float z = localEffectorTrans.z;
				float len = FBIKSqrt( y * y + z * z );
				if( (isFar && len > effectorLength) || (!isFar && len < effectorLength) ) {
					float n = FBIKSqrt( effectorLength * effectorLength - localEffectorTrans.z * localEffectorTrans.z );
					if( n > IKEpsilon ) {
						localEffectorTrans.y = -n;
					} else {
						localEffectorTrans.z = 0.0f;
						localEffectorTrans.y = -effectorLength;
					}

					return true;
				}

				return false;
			}

			static bool _PrefixLegEffectorPos_Upper_Circular_Far( ref Vector3 localEffectorTrans,
				float centerPositionZ,
				float effectorLengthZ, float effectorLengthY )
			{
				if( effectorLengthY > IKEpsilon && effectorLengthZ > IKEpsilon ) {
					float y = localEffectorTrans.y;
					float z = localEffectorTrans.z - centerPositionZ;

					y /= effectorLengthY;
					z /= effectorLengthZ;

					float len = FBIKSqrt( y * y + z * z );
					if( len > 1.0f ) {
						float n = FBIKSqrt( 1.0f - z * z );
						if( n > IKEpsilon ) {
							localEffectorTrans.y = n * effectorLengthY;
						} else {
							localEffectorTrans.z = centerPositionZ;
							localEffectorTrans.y = effectorLengthY;
						}

						return true;
					}
				}

				return false;
			}

            static void _ComputeLocalDirXZ( ref Vector3 localDir, out Vector3 localDirXZ )
			{
				if( localDir.y >= _LocalDirMaxTheta - IKEpsilon ) {
					localDirXZ = new Vector3( 1.0f, 0.0f, 0.0f );
				} else if( localDir.y > _LocalDirMaxTheta - _LocalDirLerpTheta - IKEpsilon ) {
					float r = (localDir.y - (_LocalDirMaxTheta - _LocalDirLerpTheta)) * (1.0f / _LocalDirLerpTheta);
					localDirXZ = new Vector3( localDir.x + (1.0f - localDir.x) * r, 0.0f, localDir.z - localDir.z * r );
					if( !FBIKVecNormalizeXZ( ref localDirXZ ) ) {
						localDirXZ = new Vector3( 1.0f, 0.0f, 0.0f );
					}
				} else if( localDir.y <= -_LocalDirMaxTheta + IKEpsilon ) {
					localDirXZ = new Vector3( -1.0f, 0.0f, 0.0f );
				} else if( localDir.y < -(_LocalDirMaxTheta - _LocalDirLerpTheta - IKEpsilon) ) {
					float r = (-(_LocalDirMaxTheta - _LocalDirLerpTheta) - localDir.y) * (1.0f / _LocalDirLerpTheta);
					localDirXZ = new Vector3( localDir.x + (-1.0f - localDir.x) * r, 0.0f, localDir.z - localDir.z * r );
					if( !FBIKVecNormalizeXZ( ref localDirXZ ) ) {
						localDirXZ = new Vector3( -1.0f, 0.0f, 0.0f );
					}
				} else {
					localDirXZ = new Vector3( localDir.x, 0.0f, localDir.z );
					if( !FBIKVecNormalizeXZ( ref localDirXZ ) ) {
						localDirXZ = new Vector3( 1.0f, 0.0f, 0.0f );
					}
				}
			}
			static void _ComputeLocalDirYZ( ref Vector3 localDir, out Vector3 localDirYZ )
			{
				if( localDir.x >= _LocalDirMaxTheta - IKEpsilon ) {
					localDirYZ = new Vector3( 0.0f, 0.0f, -1.0f );
				} else if( localDir.x > _LocalDirMaxTheta - _LocalDirLerpTheta - IKEpsilon ) {
					float r = (localDir.x - (_LocalDirMaxTheta - _LocalDirLerpTheta)) * (1.0f / _LocalDirLerpTheta);
					localDirYZ = new Vector3( 0.0f, localDir.y - localDir.y * r, localDir.z + (-1.0f - localDir.z) * r );
					if( !FBIKVecNormalizeYZ( ref localDirYZ ) ) {
						localDirYZ = new Vector3( 0.0f, 0.0f, -1.0f );
					}
				} else if( localDir.x <= -_LocalDirMaxTheta + IKEpsilon ) {
					localDirYZ = new Vector3( 0.0f, 0.0f, 1.0f );
				} else if( localDir.x < -(_LocalDirMaxTheta - _LocalDirLerpTheta - IKEpsilon) ) {
					float r = (-(_LocalDirMaxTheta - _LocalDirLerpTheta) - localDir.x) * (1.0f / _LocalDirLerpTheta);
					localDirYZ = new Vector3( 0.0f, localDir.y - localDir.y * r, localDir.z + (1.0f - localDir.z) * r );
					if( !FBIKVecNormalizeYZ( ref localDirYZ ) ) {
						localDirYZ = new Vector3( 0.0f, 0.0f, 1.0f );
					}
				} else {
					localDirYZ = new Vector3( 0.0f, localDir.y, localDir.z );
					if( !FBIKVecNormalizeYZ( ref localDirYZ ) ) {
						localDirYZ = new Vector3( 0.0f, 0.0f, (localDir.x >= 0.0f) ? -1.0f : 1.0f );
					}
				}
			}

			public bool IsSolverEnabled()
			{
				if( !_endEffector.positionEnabled && !(_bendingEffector.positionEnabled && _bendingEffector.pull > IKEpsilon) ) {
					if( _limbIKType == LimbIKType.Arm ) {
						if( !_settings.limbIK.armAlwaysSolveEnabled ) {
							return false;
						}
					} else if( _limbIKType == LimbIKType.Leg ) {
						if( !_settings.limbIK.legAlwaysSolveEnabled ) {
							return false;
						}
					}
				}

				return true;
			}

			public bool Presolve(
				ref Matrix3x3 parentBaseBasis,
				ref Vector3 beginPos,
				out Vector3 solvedBeginToBendingDir,
				out Vector3 solvedBendingToEndDir )
			{
				float effectorLen;
				Matrix3x3 baseBasis;
				return PresolveInternal( ref parentBaseBasis, ref beginPos, out effectorLen, out baseBasis, out solvedBeginToBendingDir, out solvedBendingToEndDir );
            }

            public bool PresolveInternal(
			ref Matrix3x3 parentBaseBasis,
			ref Vector3 beginPos,
			out float effectorLen,
			out Matrix3x3 baseBasis,
			out Vector3 solvedBeginToBendingDir,
			out Vector3 solvedBendingToEndDir)
            {
                solvedBeginToBendingDir = Vector3.zero;
                solvedBendingToEndDir = Vector3.zero;
                baseBasis = Matrix3x3.identity;

                Vector3 bendingPos = _bendingEffector._hidden_worldPosition;
                Vector3 effectorPos = _endEffector._hidden_worldPosition;

                if (_bendingEffector.positionEnabled && _bendingEffector.pull > IKEpsilon)
                {
                    Vector3 beginToBending = bendingPos - beginPos;
                    float beginToBendingLenSq = beginToBending.sqrMagnitude;
                    if (beginToBendingLenSq > _bendingBone._defaultLocalLength.length)
                    {
                        float beginToBendingLen = FBIKSqrt(beginToBendingLenSq);
                        float tempLen = beginToBendingLen - _bendingBone._defaultLocalLength.length;
                        if (tempLen < -IKEpsilon && beginToBendingLen > IKEpsilon)
                        {
                            bendingPos += beginToBending * (tempLen / beginToBendingLen);
                        }
                    }

                    Vector3 bendingToEffector = effectorPos - bendingPos;
                    float bendingToEffectorLen = bendingToEffector.magnitude;
                    if (bendingToEffectorLen > IKEpsilon)
                    {
                        float tempLen = _endBone._defaultLocalLength.length - bendingToEffectorLen;
                        if (tempLen > IKEpsilon || tempLen < -IKEpsilon)
                        {
                            float pull;
                            if (_endEffector.positionEnabled && _endEffector.pull > IKEpsilon)
                            {
                                pull = _bendingEffector.pull / (_bendingEffector.pull + _endEffector.pull);
                            }
                            else
                            {
                                pull = _bendingEffector.pull;
                            }
                            effectorPos += bendingToEffector * ((tempLen * pull) / bendingToEffectorLen);
                        }
                    }
                }

                Matrix3x3 parentBaseBasisInv = parentBaseBasis.transpose;
                Vector3 effectorTrans = effectorPos - beginPos;
                effectorLen = effectorTrans.magnitude;

                if (effectorLen <= IKEpsilon || _effectorMaxLength.value <= IKEpsilon)
                {
                    baseBasis = Matrix3x3.identity;
                    return false;
                }

                Vector3 effectorDir = effectorTrans * (1.0f / effectorLen);

                if (effectorLen > _effectorMaxLength.value)
                {
                    effectorTrans = effectorDir * _effectorMaxLength.value;
                    effectorPos = beginPos + effectorTrans;
                    effectorLen = _effectorMaxLength.value;
                }

                _SolveBaseBasis(out baseBasis, ref parentBaseBasis, ref effectorDir);

                if (_limbIKType == LimbIKType.Leg)
                {
                    if (_settings.limbIK.prefixLegEffectorEnabled)
                    {
                        Vector3 localEffectorTrans;
                        FBIKMatMultVec(out localEffectorTrans, ref parentBaseBasisInv, ref effectorTrans);

                        bool isProcessed = false;
                        bool isLimited = false;
                        if (localEffectorTrans.z >= 0.0f)
                        {
                            if (localEffectorTrans.z >= _beginToBendingLength + _bendingToEndLength)
                            {
                                isProcessed = true;
                                localEffectorTrans.z = _beginToBendingLength + _bendingToEndLength;
                                localEffectorTrans.y = 0.0f;
                            }

                            if (!isProcessed && localEffectorTrans.y >= -_effectorMinLength.value && localEffectorTrans.z <= _leg_upperLimitNearCircleZ)
                            {
                                isProcessed = true;
                                isLimited = _PrefixLegEffectorPos_UpperNear(ref localEffectorTrans);
                            }

                            if (!isProcessed && localEffectorTrans.y >= 0.0f && localEffectorTrans.z > _leg_upperLimitNearCircleZ)
                            {
                                isProcessed = true;
                                _PrefixLegEffectorPos_Upper_Circular_Far(ref localEffectorTrans,
                                    _leg_upperLimitNearCircleZ,
                                    _beginToBendingLength + _bendingToEndLength - _leg_upperLimitNearCircleZ,
                                    _leg_upperLimitNearCircleY);
                            }

                            if (!isProcessed)
                            {
                                isProcessed = true;
                                isLimited = _PrefixLegEffectorPos_Circular_Far(ref localEffectorTrans, _beginToBendingLength + _bendingToEndLength);
                            }
                        }
                        else
                        {
                            if (localEffectorTrans.y >= -_effectorMinLength.value)
                            {
                                isLimited = true;
                                localEffectorTrans.y = -_effectorMinLength.value;
                            }
                            else
                            {
                                isLimited = _PrefixLegEffectorPos_Circular_Far(ref localEffectorTrans, _beginToBendingLength + _bendingToEndLength);
                            }
                        }

                        if (isLimited)
                        {
                            FBIKMatMultVec(out effectorTrans, ref parentBaseBasis, ref localEffectorTrans);
                            effectorLen = effectorTrans.magnitude;
                            effectorPos = beginPos + effectorTrans;
                            if (effectorLen > IKEpsilon)
                            {
                                effectorDir = effectorTrans * (1.0f / effectorLen);
                            }
                        }
                    }

                    if (!_bendingEffector.positionEnabled)
                    {
                        float cosTheta = ComputeCosTheta(
                            _bendingToEndLengthSq,
                            effectorLen * effectorLen,
                            _beginToBendingLengthSq,
                            effectorLen,
                            _beginToBendingLength);

                        float sinTheta = FBIKSqrtClamp01(1.0f - cosTheta * cosTheta);

                        float moveC = _beginToBendingLength * (1.0f - Mathf.Max(_defaultCosTheta - cosTheta, 0.0f));
                        float moveS = _beginToBendingLength * Mathf.Max(sinTheta - _defaultSinTheta, 0.0f);

                        float automaticKneeBaseAngle = _settings.limbIK.automaticKneeBaseAngle;
                        if (automaticKneeBaseAngle >= -IKEpsilon && automaticKneeBaseAngle <= IKEpsilon)
                        {
                            bendingPos = beginPos + -baseBasis.column1 * moveC + baseBasis.column2 * moveS;
                        }
                        else
                        {
                            if (_automaticKneeBaseTheta._degrees != automaticKneeBaseAngle)
                            {
                                _automaticKneeBaseTheta._Reset(automaticKneeBaseAngle);
                            }

                            float kneeSin = _automaticKneeBaseTheta.cos;
                            float kneeCos = FBIKSqrt(1.0f - kneeSin * kneeSin);
                            if (_limbIKSide == Side.Right)
                            {
                                if (automaticKneeBaseAngle >= 0.0f)
                                {
                                    kneeCos = -kneeCos;
                                }
                            }
                            else
                            {
                                if (automaticKneeBaseAngle < 0.0f)
                                {
                                    kneeCos = -kneeCos;
                                }
                            }

                            bendingPos = beginPos + -baseBasis.column1 * moveC
                                + baseBasis.column0 * moveS * kneeCos
                                + baseBasis.column2 * moveS * kneeSin;
                        }
                    }
                }

                if (_limbIKType == LimbIKType.Arm)
                {
                    if (!_bendingEffector.positionEnabled)
                    {
                        if (_elbowPoleBone != null && _elbowPoleBone.transformIsAlive)
                        {
                            Vector3 polePos = _elbowPoleBone.worldPosition;
                            Vector3 toBend = polePos - beginPos;

                            float dot = Vector3.Dot(toBend, effectorDir);
                            Vector3 poleBendDir = toBend - effectorDir * dot;

                            if (FBIKVecNormalize(ref poleBendDir))
                            {
                                bendingPos = beginPos + poleBendDir * _beginToBendingLength;
                                Vector3 toEnd = effectorPos - bendingPos;

                                if (FBIKVecNormalize(ref toEnd))
                                {
                                    bendingPos = effectorPos - toEnd * _bendingToEndLength;
                                }
                            }
                        }
                    }
				}

                bool isSolved = false;
                {
                    Vector3 beginToBendingTrans = bendingPos - beginPos;
                    Vector3 intersectBendingTrans = beginToBendingTrans - effectorDir * Vector3.Dot(effectorDir, beginToBendingTrans);
                    float intersectBendingLen = intersectBendingTrans.magnitude;

                    if (intersectBendingLen > IKEpsilon)
                    {
                        Vector3 intersectBendingDir = intersectBendingTrans * (1.0f / intersectBendingLen);
                        float bc2 = 2.0f * _beginToBendingLength * effectorLen;

                        if (bc2 > IKEpsilon)
                        {
                            float effectorCosTheta = (_beginToBendingLengthSq + effectorLen * effectorLen - _bendingToEndLengthSq) / bc2;
                            float effectorSinTheta = FBIKSqrtClamp01(1.0f - effectorCosTheta * effectorCosTheta);

                            Vector3 beginToInterTranslate = effectorDir * effectorCosTheta * _beginToBendingLength
                                + intersectBendingDir * effectorSinTheta * _beginToBendingLength;
                            Vector3 interToEndTranslate = effectorPos - (beginPos + beginToInterTranslate);

                            if (FBIKVecNormalize2(ref beginToInterTranslate, ref interToEndTranslate))
                            {
                                isSolved = true;
                                solvedBeginToBendingDir = beginToInterTranslate;
                                solvedBendingToEndDir = interToEndTranslate;
                            }
                        }
                    }
                }

                if (!isSolved)
                {
                    Vector3 bendingDir = bendingPos - beginPos;
                    if (FBIKVecNormalize(ref bendingDir))
                    {
                        Vector3 interPos = beginPos + bendingDir * _beginToBendingLength;
                        Vector3 endDir = effectorPos - interPos;
                        if (FBIKVecNormalize(ref endDir))
                        {
                            isSolved = true;
                            solvedBeginToBendingDir = bendingDir;
                            solvedBendingToEndDir = endDir;
                        }
                    }
                }

                return isSolved;
            }

            public bool Solve()
			{
				_UpdateArgs();

				_arm_isSolvedLimbIK = false;

				Quaternion bendingBonePrevRotation = Quaternion.identity;
				Quaternion endBonePrevRotation = Quaternion.identity;
				if( !_internalValues.resetTransforms ) {
					float endRotationWeight = _endEffector.rotationEnabled ? _endEffector.rotationWeight : 0.0f;
					if( endRotationWeight > IKEpsilon ) {
						if( endRotationWeight < 1.0f - IKEpsilon ) {
							bendingBonePrevRotation = _bendingBone.worldRotation;
							endBonePrevRotation = _endBone.worldRotation;
						}
					}
				}

				if (_limbIKType == LimbIKType.Arm)
				{
					_UpdateElbowPole();
				}

				bool r = _SolveInternal();
				r |= _SolveEndRotation( r, ref bendingBonePrevRotation, ref endBonePrevRotation );
				r |= _RollInternal();

				return r;
			}

			void _UpdateElbowPole()
			{
				if (_fullbodyik == null) return;
				if (!_fullbodyik.settings.rollEnabled) return;
				if (_elbowPoleBone == null || !_elbowPoleBone.transformIsAlive) return;

				var leftArm = _fullbodyik.leftArmBones.arm;
				var rightArm = _fullbodyik.rightArmBones.arm;

				if (leftArm == null || !leftArm.transformIsAlive ||
					rightArm == null || !rightArm.transformIsAlive) return;

				ArmEffectors armEffectors = (_limbIKSide == Side.Left) ? _fullbodyik.leftArmEffectors : _fullbodyik.rightArmEffectors;
				if (!armEffectors.wrist.positionEnabled) return;

				float weight = armEffectors.elbowPoleHorizontal;
				float verticalWeight = armEffectors.elbowPoleVertical;

				Vector3 leftArmPos = leftArm.worldPosition;
				Vector3 rightArmPos = rightArm.worldPosition;

				Vector3 shoulderDir = rightArmPos - leftArmPos;
				float shoulderDistance = shoulderDir.magnitude;
				Vector3 shoulderDirNormalized = shoulderDir.normalized;

				Vector3 currentArmPos = (_limbIKSide == Side.Left) ? leftArmPos : rightArmPos;

				Vector3 rightToLeftDir = -shoulderDirNormalized;
				Vector3 sideOffset = rightToLeftDir * shoulderDistance * 5f;
				Vector3 sidePos = (_limbIKSide == Side.Left) ?
					leftArmPos + sideOffset :
					rightArmPos - sideOffset;

				Vector3 shoulderForward = Vector3.Cross(Vector3.up, shoulderDirNormalized).normalized;
				Vector3 backOffset = shoulderForward * shoulderDistance * 5f;
				Vector3 backPos = currentArmPos + backOffset;

				Vector3 polePos = Vector3.Lerp(backPos, sidePos, weight);

				float verticalRange = shoulderDistance * 5f;
				float verticalOffset = (verticalWeight - 0.5f) * 2f * verticalRange;
				Vector3 oldPole = polePos;
				polePos.y += verticalOffset;

#if UNITY_EDITOR
				if (_fullbodyik.editorSettings.debugMode)
				{
					Debug.DrawLine(leftArm.worldPosition, rightArm.worldPosition, Color.yellow, 1f);
					Debug.DrawLine(sidePos, backPos, Color.blue, 1f);
					Debug.DrawLine(oldPole, polePos + Vector3.up * 0.05f, Color.red, 1f);
				}
#endif

				_elbowPoleBone.transform.position = polePos;
			}

			public bool _SolveInternal()
			{
				if( !IsSolverEnabled() ) return false;
				if( _beginBone.parentBone == null || !_beginBone.parentBone.transformIsAlive ) return false;

				Quaternion parentBoneWorldRotation = _beginBone.parentBone.worldRotation;
				Matrix3x3 parentBaseBasis;
				FBIKMatSetRotMult( out parentBaseBasis, ref parentBoneWorldRotation, ref _beginBone.parentBone._worldToBaseRotation );

				Vector3 beginPos = _beginBone.worldPosition;

				float effectorLen;
				Matrix3x3 baseBasis;
				Vector3 solvedBeginToBendingDir;
				Vector3 solvedBendingToEndDir;

				if( !PresolveInternal( ref parentBaseBasis, ref beginPos, out effectorLen, out baseBasis, out solvedBeginToBendingDir, out solvedBendingToEndDir ) ) return false;

				Matrix3x3 beginBasis = Matrix3x3.identity;
				Matrix3x3 bendingBasis = Matrix3x3.identity;

				if( _limbIKType == LimbIKType.Arm ) 
				{
					if( _limbIKSide == Side.Left ) 
					{
						solvedBeginToBendingDir = -solvedBeginToBendingDir;
						solvedBendingToEndDir = -solvedBendingToEndDir;
					}

					Vector3 basisY = parentBaseBasis.column1;
					Vector3 basisZ = parentBaseBasis.column2;
					if( !FBIKComputeBasisLockX( out beginBasis, ref solvedBeginToBendingDir, ref basisY, ref basisZ ) ) return false;

					basisY = Vector3.Cross( -solvedBeginToBendingDir, solvedBendingToEndDir );
					if( _limbIKSide == Side.Left ) basisY = -basisY;
					if( !FBIKComputeBasisFromXYLockX( out bendingBasis, ref solvedBendingToEndDir, ref basisY ) ) return false;
				} 
				else 
				{
					solvedBeginToBendingDir = -solvedBeginToBendingDir;
					solvedBendingToEndDir = -solvedBendingToEndDir;

					Vector3 basisX = baseBasis.column0;
					Vector3 basisZ = baseBasis.column2;
					if( !FBIKComputeBasisLockY( out beginBasis, ref basisX, ref solvedBeginToBendingDir, ref basisZ ) ) {
						return false;
					}

					FBIKMatMultCol0( out basisX, ref beginBasis, ref _beginToBendingBoneBasis );

					if( !FBIKComputeBasisFromXYLockY( out bendingBasis, ref basisX, ref solvedBendingToEndDir ) ) {
						return false;
					}
				}

				if( _limbIKType == LimbIKType.Arm ) {
					_arm_isSolvedLimbIK = true;
					_arm_solvedBeginBoneBasis = beginBasis;
					_arm_solvedBendingBoneBasis = bendingBasis;
				}

				Quaternion worldRotation;
				FBIKMatMultGetRot( out worldRotation, ref beginBasis, ref _beginBone._boneToWorldBasis );
				_beginBone.worldRotation = worldRotation;
				FBIKMatMultGetRot( out worldRotation, ref bendingBasis, ref _bendingBone._boneToWorldBasis );
                _bendingBone.worldRotation = worldRotation;
				return true;
			}

			bool _SolveEndRotation( bool isSolved, ref Quaternion bendingBonePrevRotation, ref Quaternion endBonePrevRotation )
			{
				float endRotationWeight = _endEffector.rotationEnabled ? _endEffector.rotationWeight : 0.0f;

                if ( endRotationWeight > IKEpsilon ) 
				{
					Quaternion endEffectorWorldRotation = _endEffector.worldRotation;
					Quaternion toRotation;
					FBIKQuatMult( out toRotation, ref endEffectorWorldRotation, ref _endEffectorToWorldRotation );

					if( endRotationWeight < 1.0f - IKEpsilon )
					{
						Quaternion fromRotation;
						if( _internalValues.resetTransforms )
						{
							Quaternion bendingBoneWorldRotation = _bendingBone.worldRotation;
							FBIKQuatMult3( out fromRotation, ref bendingBoneWorldRotation, ref _bendingBone._worldToBaseRotation, ref _endBone._baseToWorldRotation );
						}
						else
						{
							if( isSolved )
							{
								Quaternion bendingBoneWorldRotation = _bendingBone.worldRotation;
								FBIKQuatMultNorm3Inv1( out fromRotation, ref bendingBoneWorldRotation, ref bendingBonePrevRotation, ref endBonePrevRotation );
							}
							else fromRotation = endBonePrevRotation;
						}
						_endBone.worldRotation = Quaternion.Lerp( fromRotation, toRotation, endRotationWeight );
					}
					else _endBone.worldRotation = toRotation;

					_EndRotationLimit();
                    return true;
				}
				else
				{
					if( _internalValues.resetTransforms )
					{
						Quaternion fromRotation, bendingBoneWorldRotation = _bendingBone.worldRotation;
						FBIKQuatMult3( out fromRotation, ref bendingBoneWorldRotation, ref _bendingBone._worldToBaseRotation, ref _endBone._baseToWorldRotation );
						_endBone.worldRotation = fromRotation;
						return true;
					}
				}

				return false;
			}

			void _EndRotationLimit()
            {//razz ToAngleAxis and AngleAxis may not match in rot values and wrist can be 180 degree inverted
                if ( _limbIKType == LimbIKType.Arm ) {
					if( !_settings.limbIK.wristLimitEnabled ) {
						return;
					}
				} else if( _limbIKType == LimbIKType.Leg ) {
					if( !_settings.limbIK.footLimitEnabled ) {
						return;
					}
				}
				Quaternion tempRotation, endRotation, bendingRotation, localRotation;
				tempRotation = _endBone.worldRotation;
				FBIKQuatMult( out endRotation, ref tempRotation, ref _endBone._worldToBaseRotation );
				tempRotation = _bendingBone.worldRotation;
				FBIKQuatMult( out bendingRotation, ref tempRotation, ref _bendingBone._worldToBaseRotation );
				FBIKQuatMultInv0( out localRotation, ref bendingRotation, ref endRotation );

				if( _limbIKType == LimbIKType.Arm ) {
					bool isLimited = false;
					float limitAngle = _settings.limbIK.wristLimitAngle;

					float angle;
					Vector3 axis;
					localRotation.ToAngleAxis( out angle, out axis );
					if( angle < -limitAngle ) {
						angle = -limitAngle;
						isLimited = true;
					} else if( angle > limitAngle ) {
						angle = limitAngle;
						isLimited = true;
					}

					if( isLimited ) {
						localRotation = Quaternion.AngleAxis( angle, axis );
						FBIKQuatMultNorm3( out endRotation, ref bendingRotation, ref localRotation, ref _endBone._baseToWorldRotation );
						_endBone.worldRotation = endRotation;
					}
				} else if( _limbIKType == LimbIKType.Leg ) {
					Matrix3x3 localBasis;
					FBIKMatSetRot( out localBasis, ref localRotation );

					Vector3 localDirY = localBasis.column1;
					Vector3 localDirZ = localBasis.column2;

					bool isLimited = false;
					isLimited |= _LimitXZ_Square( ref localDirY,
						_internalValues.limbIK.footLimitRollTheta.sin,
						_internalValues.limbIK.footLimitRollTheta.sin,
						_internalValues.limbIK.footLimitPitchUpTheta.sin,
						_internalValues.limbIK.footLimitPitchDownTheta.sin );
					isLimited |= _LimitXY_Square( ref localDirZ,
						_internalValues.limbIK.footLimitYawTheta.sin,
						_internalValues.limbIK.footLimitYawTheta.sin,
						_internalValues.limbIK.footLimitPitchDownTheta.sin,
						_internalValues.limbIK.footLimitPitchUpTheta.sin );

					if( isLimited ) {
						if( FBIKComputeBasisFromYZLockZ( out localBasis, ref localDirY, ref localDirZ ) ) {
							FBIKMatGetRot( out localRotation, ref localBasis );
							FBIKQuatMultNorm3( out endRotation, ref bendingRotation, ref localRotation, ref _endBone._baseToWorldRotation );
							_endBone.worldRotation = endRotation;
						}
					}
				}
			}

            bool _RollInternal()
            {
                if (_limbIKType != LimbIKType.Arm || !_settings.rollEnabled) return false;
				if (_armRollBone == null || _handRollBone == null || _elbowPoleBone == null) return false;
				if (_handRollBone.transform == null || _endBone == null || _endBone.transform == null) return false;
				if (_armRollBone.transform == null || _bendingBone == null || _beginBone == null) return false;
				if (_elbowPoleBone.transform == null) return false;

				_handRollBone.transform.rotation = _endBone.transform.rotation;
				bool isSolved = false;

                if (_arm_isSolvedLimbIK)
                {
                    Transform handTransform = (_limbIKSide == Side.Left) ? _leftHandTransform : _rightHandTransform;

					// Forearm roll
					float forearmRate = 0.35f;
                    {
                        Quaternion bendingRot = _bendingBone.worldRotation;
                        Quaternion handRot = _handRollBone.transform.rotation;

                        // Get local rotation between elbow and hand
                        Quaternion localRot;
                        FBIKQuatMultInv0(out localRot, ref bendingRot, ref handRot);

                        // Get only the Y euler (roll)
                        float twist = localRot.eulerAngles.y;
                        if (twist > 180f) twist -= 360f;

						// Store partial roll in roll bone
						Quaternion rollRot = Quaternion.Euler(0, twist * forearmRate, 0);
						_elbowPoleBone.worldRotation = bendingRot * rollRot;

						// Apply remaining twist to elbow
						float remainingTwist = twist * (1.0f - forearmRate);
						Quaternion twistRot = Quaternion.Euler(0, remainingTwist, 0);
						_bendingBone.worldRotation = bendingRot * twistRot;
					}

					// Upper arm roll
					float upperarmRate = 0.65f;
					{
                        // Get current rotations
                        Quaternion beginRot = _beginBone.worldRotation;
                        Quaternion bendingRot = _bendingBone.worldRotation;

                        // Get local rotation between shoulder and elbow
                        Quaternion localRot;
                        FBIKQuatMultInv0(out localRot, ref beginRot, ref bendingRot);

                        // Get only the Y euler (roll)
                        float twist = localRot.eulerAngles.y;
                        if (twist > 180f) twist -= 360f;

						// Store partial roll in roll bone
						Quaternion rollRot = Quaternion.Euler(0, twist * upperarmRate, 0);
						_armRollBone.worldRotation = beginRot * rollRot;

						// Apply remaining twist to shoulder
						float remainingTwist = twist * (1.0f - upperarmRate);
						Quaternion twistRot = Quaternion.Euler(0, remainingTwist, 0);
						_beginBone.worldRotation = beginRot * twistRot;

					}

					isSolved = true;
				}

                return isSolved;
            }
        }
	}
}
