using UnityEngine;

namespace razz
{
	public partial class FullBodyIK
	{
		public class HeadIK
		{
			Settings _settings;
			InternalValues _internalValues;

			Bone _neckBone;
			Bone _headBone;

			Effector _headEffector;

			Quaternion _headEffectorToWorldRotation = Quaternion.identity;

			public HeadIK(FullBodyIK fullBodyIK)
			{
				_settings = fullBodyIK.settings;
				_internalValues = fullBodyIK.internalValues;

				_neckBone = _PrepareBone(fullBodyIK.headBones.neck);
				_headBone = _PrepareBone(fullBodyIK.headBones.head);
				_headEffector = fullBodyIK.headEffectors.head;
			}

			bool _isSyncDisplacementAtLeastOnce;

			void _SyncDisplacement( FullBodyIK fullBodyIK )
			{
				if( _settings.syncDisplacement == SyncDisplacement.Everyframe || !_isSyncDisplacementAtLeastOnce ) {
					_isSyncDisplacementAtLeastOnce = true;

					if( _headBone != null && _headBone.transformIsAlive ) 
					{
						if( _headEffector != null ) 
						{
							FBIKQuatMultInv0( out _headEffectorToWorldRotation, ref _headEffector._defaultRotation, ref _headBone._defaultRotation );
						}
					}
                }
			}

			public bool Solve( FullBodyIK fullBodyIK )
			{
				if( _neckBone == null || !_neckBone.transformIsAlive ||
					_headBone == null || !_headBone.transformIsAlive ||
					_headBone.parentBone == null || !_headBone.parentBone.transformIsAlive ) {
					return false;
				}

				_SyncDisplacement( fullBodyIK );

				float headPositionWeight = _headEffector.positionEnabled ? _headEffector.positionWeight : 0.0f;

				if( headPositionWeight <= IKEpsilon ) {
					Quaternion parentWorldRotation = _neckBone.parentBone.worldRotation;
					Quaternion parentBaseRotation;
					FBIKQuatMult( out parentBaseRotation, ref parentWorldRotation, ref _neckBone.parentBone._worldToBaseRotation );

					if( _internalValues.resetTransforms ) {
						Quaternion tempRotation;
						FBIKQuatMult( out tempRotation, ref parentBaseRotation, ref _neckBone._baseToWorldRotation );
						_neckBone.worldRotation = tempRotation;
					}

					float headRotationWeight = _headEffector.rotationEnabled ? _headEffector.rotationWeight : 0.0f;
					if( headRotationWeight > IKEpsilon ) {
						Quaternion headEffectorWorldRotation = _headEffector.worldRotation;
                        Quaternion toRotation;
						FBIKQuatMult( out toRotation, ref headEffectorWorldRotation, ref _headEffectorToWorldRotation );
						if( headRotationWeight < 1.0f - IKEpsilon ) {
							Quaternion fromRotation;
							if( _internalValues.resetTransforms ) {
								FBIKQuatMult( out fromRotation, ref parentBaseRotation, ref _headBone._baseToWorldRotation );
                            } else {
								fromRotation = _headBone.worldRotation;
							}
							_headBone.worldRotation = Quaternion.Lerp( fromRotation, toRotation, headRotationWeight );
						} else {
							_headBone.worldRotation = toRotation;
						}

						_HeadRotationLimit();
					} else {
						if( _internalValues.resetTransforms ) {
							Quaternion tempRotation;
							FBIKQuatMult( out tempRotation, ref parentBaseRotation, ref _headBone._baseToWorldRotation );
							_headBone.worldRotation = tempRotation;
                        }
					}

					return _internalValues.resetTransforms || (headRotationWeight > IKEpsilon);
				}

				_Solve( fullBodyIK );
				return true;
			}

			void _HeadRotationLimit()
			{
				Quaternion tempRotation, headRotation, neckRotation, localRotation;
				tempRotation = _headBone.worldRotation;
                FBIKQuatMult( out headRotation, ref tempRotation, ref _headBone._worldToBaseRotation );
				tempRotation = _neckBone.worldRotation;
				FBIKQuatMult( out neckRotation, ref tempRotation, ref _neckBone._worldToBaseRotation );
				FBIKQuatMultInv0( out localRotation, ref neckRotation, ref headRotation );

				Matrix3x3 localBasis;
				FBIKMatSetRot( out localBasis, ref localRotation );

				Vector3 localDirY = localBasis.column1;
				Vector3 localDirZ = localBasis.column2;

				bool isLimited = false;
				isLimited |= _LimitXZ_Square( ref localDirY,
					_internalValues.headIK.headLimitRollTheta.sin,
					_internalValues.headIK.headLimitRollTheta.sin,
					_internalValues.headIK.headLimitPitchUpTheta.sin,
                    _internalValues.headIK.headLimitPitchDownTheta.sin );
				isLimited |= _LimitXY_Square( ref localDirZ,
					_internalValues.headIK.headLimitYawTheta.sin,
					_internalValues.headIK.headLimitYawTheta.sin,
					_internalValues.headIK.headLimitPitchDownTheta.sin,
					_internalValues.headIK.headLimitPitchUpTheta.sin );

				if( isLimited ) {
					if( FBIKComputeBasisFromYZLockZ( out localBasis, ref localDirY, ref localDirZ ) ) {
						FBIKMatGetRot( out localRotation, ref localBasis );
						FBIKQuatMultNorm3( out headRotation, ref neckRotation, ref localRotation, ref _headBone._baseToWorldRotation );
						_headBone.worldRotation = headRotation;
					}
				}
			}

			void _Solve( FullBodyIK fullBodyIK )
			{
				Quaternion parentWorldRotation = _neckBone.parentBone.worldRotation;
				Matrix3x3 parentBasis;
				FBIKMatSetRotMultInv1( out parentBasis, ref parentWorldRotation, ref _neckBone.parentBone._defaultRotation );
				Matrix3x3 parentBaseBasis;
				FBIKMatMult( out parentBaseBasis, ref parentBasis, ref _internalValues.defaultRootBasis );
				Quaternion parentBaseRotation;
				FBIKQuatMult( out parentBaseRotation, ref parentWorldRotation, ref _neckBone.parentBone._worldToBaseRotation );

				float headPositionWeight = _headEffector.positionEnabled ? _headEffector.positionWeight : 0.0f;

				Quaternion neckBonePrevRotation = Quaternion.identity;
				Quaternion headBonePrevRotation = Quaternion.identity;
				if( !_internalValues.resetTransforms ) {
					neckBonePrevRotation = _neckBone.worldRotation;
					headBonePrevRotation = _headBone.worldRotation;
				}
				if( headPositionWeight > IKEpsilon ) {
					Matrix3x3 neckBoneBasis;
					FBIKMatMult( out neckBoneBasis, ref parentBasis, ref _neckBone._localAxisBasis );

					Vector3 yDir = _headEffector.worldPosition - _neckBone.worldPosition;
					if( FBIKVecNormalize( ref yDir ) ) {
						Vector3 localDir;
						FBIKMatMultVecInv( out localDir, ref neckBoneBasis, ref yDir );

						if( _LimitXZ_Square( ref localDir,
							_internalValues.headIK.neckLimitRollTheta.sin,
							_internalValues.headIK.neckLimitRollTheta.sin,
							_internalValues.headIK.neckLimitPitchDownTheta.sin,
							_internalValues.headIK.neckLimitPitchUpTheta.sin ) ) {
							FBIKMatMultVec( out yDir, ref neckBoneBasis, ref localDir );
						}

						Vector3 xDir = parentBaseBasis.column0;
						Vector3 zDir = parentBaseBasis.column2;
						if( FBIKComputeBasisLockY( out neckBoneBasis, ref xDir, ref yDir, ref zDir ) ) {
							Quaternion worldRotation;
							FBIKMatMultGetRot( out worldRotation, ref neckBoneBasis, ref _neckBone._boneToWorldBasis );
							if( headPositionWeight < 1.0f - IKEpsilon ) {
								Quaternion fromRotation;
								if( _internalValues.resetTransforms ) {
									FBIKQuatMult( out fromRotation, ref parentBaseRotation, ref _neckBone._baseToWorldRotation );
								} else {
									fromRotation = neckBonePrevRotation;
								}

								_neckBone.worldRotation = Quaternion.Lerp( fromRotation, worldRotation, headPositionWeight );
                            } else {
								_neckBone.worldRotation = worldRotation;
							}
						}
					}
				} else if( _internalValues.resetTransforms ) {
					Quaternion tempRotation;
					FBIKQuatMult( out tempRotation, ref parentBaseRotation, ref _neckBone._baseToWorldRotation );
					_neckBone.worldRotation = tempRotation;
				}
			}
		}
	}
}
