using UnityEngine;

namespace razz
{
	public partial class FullBodyIK
	{
		public static void SafeNew<TYPE_>( ref TYPE_ obj )
			where TYPE_ : class, new()
		{
			if( obj == null ) {
				obj = new TYPE_();
			}
		}

		public static void SafeResize<TYPE_>( ref TYPE_[] objArray, int length )
		{
			if( objArray == null ) {
				objArray = new TYPE_[length];
			} else {
				System.Array.Resize( ref objArray, length );
			}
		}

		public static void PrepareArray< TypeA, TypeB >( ref TypeA[] dstArray, TypeB[] srcArray )
		{
			if( srcArray != null ) {
				if( dstArray == null || dstArray.Length != srcArray.Length ) {
					dstArray = new TypeA[srcArray.Length];
				}
			} else {
				dstArray = null;
			}
		}

		public static void CloneArray< Type >( ref Type[] dstArray, Type[] srcArray )
		{
			if( srcArray != null ) {
				if( dstArray == null || dstArray.Length != srcArray.Length ) {
					dstArray = new Type[srcArray.Length];
				}
				for( int i = 0; i < srcArray.Length; ++i ) {
					dstArray[i] = srcArray[i];
				}
			} else {
				dstArray = null;
			}
		}
		
		public static void DestroyImmediate( ref Transform transform )
		{
			if( transform != null ) {
				Object.DestroyImmediate( transform.gameObject );
				transform = null;
			} else {
				transform = null;
			}
		}
		
		public static void DestroyImmediate( ref Transform transform, bool allowDestroyingAssets )
		{
			if( transform != null ) {
				Object.DestroyImmediate( transform.gameObject, allowDestroyingAssets );
				transform = null;
			} else {
				transform = null;
			}
		}
		
		public static bool CheckAlive< Type >( ref Type obj )
			where Type : UnityEngine.Object
		{
			if( obj != null ) {
				return true;
			} else {
				obj = null;
				return false;
			}
		}

		public static bool IsParentOfRecusively( Transform parent, Transform child )
		{
			while( child != null ) {
				if( child.parent == parent ) {
					return true;
				}

				child = child.parent;
			}

			return false;
		}

		static Bone _PrepareBone( Bone bone )
		{
			return (bone != null && bone.transformIsAlive) ? bone : null;
		}

		static Bone[] _PrepareBones( Bone leftBone, Bone rightBone )
		{
			Assert( leftBone != null && rightBone != null );
			if( leftBone != null && rightBone != null ) {
				if( leftBone.transformIsAlive && rightBone.transformIsAlive ) {
					var bones = new Bone[2];
					bones[0] = leftBone;
					bones[1] = rightBone;
					return bones;
				}
			}

			return null;
		}

		public static string _GetAvatarName( Transform rootTransform )
		{
			if( rootTransform != null ) {
				var animator = rootTransform.GetComponent<Animator>();
				if( animator != null ) {
					var avatar = animator.avatar;
					if( avatar != null ) {
						return avatar.name;
					}
				}
			}

			return null;
		}

		public static void Assert( bool cmp )
		{
			if( !cmp ) 
			{
				Debug.LogWarning( "FullBodyIK issue, please check." );
			}
		}
	}
}
