using UnityEngine;

namespace razz
{
	public partial class FullBodyIK
	{
		[System.Serializable]
		public class Effector
		{
			[System.Flags]
			enum _EffectorFlags
			{
				None = 0x00,
				RotationContained = 0x01,
				PullContained = 0x02,
			}
			
			public Transform transform = null;

			public bool positionEnabled = false;
			public bool rotationEnabled = false;
			public float positionWeight = 0f;
			public float rotationWeight = 0f;
			public float pull = 0f;

			[System.NonSerialized]
			public Vector3 _hidden_worldPosition = Vector3.zero;

			public bool effectorEnabled {
				get {
					return this.positionEnabled || (this.rotationContained && this.rotationContained);
				}
			}

			[SerializeField]
			bool _isPresetted = false;
			[SerializeField]
			EffectorLocation _effectorLocation = EffectorLocation.Unknown;
			[SerializeField]
			EffectorType _effectorType = EffectorType.Unknown;
			[SerializeField]
			_EffectorFlags _effectorFlags = _EffectorFlags.None;

			Effector _parentEffector = null;
			Bone _bone = null;
			Bone _leftBone = null;
			Bone _rightBone = null;

			[SerializeField]
			Transform _createdTransform = null;

			[SerializeField]
			public Vector3 _defaultPosition = Vector3.zero;
			[SerializeField]
			public Quaternion _defaultRotation = Quaternion.identity;

			public bool rotationContained { get { return (this._effectorFlags & _EffectorFlags.RotationContained) != _EffectorFlags.None; } }
			public bool pullContained { get { return (this._effectorFlags & _EffectorFlags.PullContained) != _EffectorFlags.None; } }

			public EffectorLocation effectorLocation { get { return _effectorLocation; } }
			public EffectorType effectorType { get { return _effectorType; } }
			public Effector parentEffector { get { return _parentEffector; } }
			public Bone bone { get { return _bone; } }
			public Bone leftBone { get { return _leftBone; } }
			public Bone rightBone { get { return _rightBone; } }
			public Vector3 defaultPosition { get { return _defaultPosition; } }
			public Quaternion defaultRotation { get { return _defaultRotation; } }

			[System.NonSerialized]
			public Vector3 _worldPosition = Vector3.zero;
			[System.NonSerialized]
			public Quaternion _worldRotation = Quaternion.identity;

			bool _isReadWorldPosition = false;
			bool _isReadWorldRotation = false;
			bool _isWrittenWorldPosition = false;
			bool _isWrittenWorldRotation = false;

			int _transformIsAlive = -1;

			public string name {
				get {
					return GetEffectorName( _effectorLocation );
				}
			}

			public bool transformIsAlive {
				get {
					if( _transformIsAlive == -1 ) {
						_transformIsAlive = CheckAlive( ref this.transform ) ? 1 : 0;
					}

					return _transformIsAlive != 0;
				}
			}

			bool _defaultLocalBasisIsIdentity {
				get {
					if( (_effectorFlags & _EffectorFlags.RotationContained) != _EffectorFlags.None ) {
						Assert( _bone != null );
						if( _bone != null && _bone.localAxisFrom != _LocalAxisFrom.None && _bone.boneType != BoneType.Hips ) {
							return false;
						}
					}

					return true;
				}
			}
			
			public void Prefix()
			{
				positionEnabled = _GetPresetPositionEnabled( _effectorType );
				positionWeight = _GetPresetPositionWeight( _effectorType );
				pull = _GetPresetPull( _effectorType );
			}

			void _PresetEffectorLocation( EffectorLocation effectorLocation )
			{
				_isPresetted = true;
				_effectorLocation = effectorLocation;
				_effectorType = ToEffectorType( effectorLocation );
				_effectorFlags = _GetEffectorFlags( _effectorType );
			}

			public static void Prefix(
				Effector[] effectors,
				ref Effector effector,
				EffectorLocation effectorLocation,
				bool createEffectorTransform,
				Transform parentTransform,
				Effector parentEffector = null,
				Bone bone = null,
				Bone leftBone = null,
				Bone rightBone = null )
			{
				if( effector == null ) {
					effector = new Effector();
				}

				if( !effector._isPresetted ||
					effector._effectorLocation != effectorLocation ||
					(int)effector._effectorType < 0 ||
					(int)effector._effectorType >= (int)EffectorType.Max ) {
					effector._PresetEffectorLocation( effectorLocation );
				}
				
				effector._parentEffector = parentEffector;
				effector._bone = bone;
				effector._leftBone = leftBone;
				effector._rightBone = rightBone;

				effector._PrefixTransform( createEffectorTransform, parentTransform );

				if( effectors != null ) {
					effectors[(int)effectorLocation] = effector;
				}
			}
			
			static bool _GetPresetPositionEnabled( EffectorType effectorType )
			{
				switch( effectorType ) {
				case EffectorType.Wrist:	return true;
				case EffectorType.Foot:		return true;
				}

				return false;
			}

			static float _GetPresetPositionWeight( EffectorType effectorType )
			{
				switch( effectorType ) {
				case EffectorType.Arm:		return 0.0f;
				}

				return 0f;
			}

			static float _GetPresetPull( EffectorType effectorType )
			{
				switch( effectorType ) {
				case EffectorType.Hips:		return 1.0f;
				case EffectorType.Arm:		return 1.0f;
				case EffectorType.Wrist:	return 1.0f;
				case EffectorType.Foot:		return 1.0f;
				}

				return 0.0f;
			}
			
			static _EffectorFlags _GetEffectorFlags( EffectorType effectorType )
			{
				switch( effectorType ) {
				case EffectorType.Hips:		return _EffectorFlags.RotationContained | _EffectorFlags.PullContained;
				case EffectorType.Neck:		return _EffectorFlags.PullContained;
				case EffectorType.Head:		return _EffectorFlags.RotationContained | _EffectorFlags.PullContained;
				case EffectorType.Arm:		return _EffectorFlags.PullContained;
				case EffectorType.Wrist:	return _EffectorFlags.RotationContained | _EffectorFlags.PullContained;
				case EffectorType.Foot:		return _EffectorFlags.RotationContained | _EffectorFlags.PullContained;
				case EffectorType.Elbow:	return _EffectorFlags.PullContained;
				case EffectorType.Knee:		return _EffectorFlags.PullContained;
				}
				
				return _EffectorFlags.None;
			}
			
			void _PrefixTransform( bool createEffectorTransform, Transform parentTransform )
			{
				if( createEffectorTransform ) {
					if( this.transform == null || this.transform != _createdTransform ) {
						if( this.transform == null ) {
							var go = new GameObject( GetEffectorName( _effectorLocation ) );
							if( parentTransform != null ) {
								go.transform.SetParent( parentTransform, false );
							} else if( _parentEffector != null && _parentEffector.transformIsAlive ) {
								go.transform.SetParent( _parentEffector.transform, false );
							}
							this.transform = go.transform;
							this._createdTransform = go.transform;
						} else {
							DestroyImmediate( ref _createdTransform, true );
						}
					} else {
						CheckAlive( ref _createdTransform );
					}
				} else {
					if( _createdTransform != null ) {
						if( this.transform == _createdTransform ) {
							this.transform = null;
						}
						Object.DestroyImmediate( _createdTransform.gameObject, true );
					}
					_createdTransform = null;
				}

				_transformIsAlive = CheckAlive( ref this.transform ) ? 1 : 0;
			}

			public void Prepare( FullBodyIK fullBodyIK )
			{
				Assert( fullBodyIK != null );

				_ClearInternal();

				_ComputeDefaultTransform( fullBodyIK );
				
				if( this.transformIsAlive ) {
					this.transform.position = _defaultPosition;

					if ( !_defaultLocalBasisIsIdentity ) {
						this.transform.rotation = _defaultRotation;
					} else {
						this.transform.localRotation = Quaternion.identity;
					}

					this.transform.localScale = Vector3.one;
				}

				_worldPosition = _defaultPosition;
				_worldRotation = _defaultRotation;
			}

			public void _ComputeDefaultTransform( FullBodyIK fullBodyIK )
			{
				if( _parentEffector != null ) {
					_defaultRotation = _parentEffector._defaultRotation;
				}

				if( _effectorType == EffectorType.Root ) {
					_defaultPosition = fullBodyIK.internalValues.defaultRootPosition;
					_defaultRotation = fullBodyIK.internalValues.defaultRootRotation;
				} else if( _effectorType == EffectorType.Hips ) {
					Assert( _bone != null && _leftBone != null && _rightBone != null );
					if( _bone != null && _leftBone != null && _rightBone != null ) {
						_defaultPosition = (_leftBone._defaultPosition + _rightBone._defaultPosition) * 0.5f;
					}
				} else {
					Assert( _bone != null );
					if( _bone != null ) {
						_defaultPosition = bone._defaultPosition;
						if( !_defaultLocalBasisIsIdentity ) {
							_defaultRotation = bone._localAxisRotation;
						}
					}
				}
			}

			void _ClearInternal()
			{
				_transformIsAlive = -1;
				_defaultPosition = Vector3.zero;
				_defaultRotation = Quaternion.identity;
			}

			public void PrepareUpdate()
			{
				_transformIsAlive = -1;
				_isReadWorldPosition = false;
				_isReadWorldRotation = false;
				_isWrittenWorldPosition = false;
				_isWrittenWorldRotation = false;
			}

			public Vector3 worldPosition {
				get {
					if( !_isReadWorldPosition && !_isWrittenWorldPosition ) {
						_isReadWorldPosition = true;
						if( this.transformIsAlive ) {
							_worldPosition = this.transform.position;
						}
					}
					return _worldPosition;
				}
				set {
					_isWrittenWorldPosition = true;
					_worldPosition = value;
				}
			}

			public Quaternion worldRotation {
				get {
					if( !_isReadWorldRotation && !_isWrittenWorldRotation ) {
						_isReadWorldRotation = true;
						if( this.transformIsAlive ) {
							_worldRotation = this.transform.rotation;
						}
					}
					return _worldRotation;
				}
				set {
					_isWrittenWorldRotation = true;
					_worldRotation = value;
				}
			}

			public void WriteToTransform()
			{
				if( _isWrittenWorldPosition ) {
					_isWrittenWorldPosition = false;
					if( this.transformIsAlive ) {
						this.transform.position = _worldPosition;
					}
				}
				if( _isWrittenWorldRotation ) {
					_isWrittenWorldRotation = false;
					if( this.transformIsAlive ) {
						this.transform.rotation = _worldRotation;
					}
				}
			}
		}
	}
}
