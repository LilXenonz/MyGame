namespace razz
{
	public partial class FullBodyIK
	{
		public enum Side
		{
			Left,
			Right,
			Max,
			None = Max,
		}

		public enum LimbIKType
		{
			Leg,
			Arm,
			Max,
			Unknown = Max,
		}

		public enum LimbIKLocation
		{
			LeftLeg,
			RightLeg,
			LeftArm,
			RightArm,
			Max,
			Unknown = Max,
		}

		public static LimbIKType ToLimbIKType( LimbIKLocation limbIKLocation )
		{
			switch( limbIKLocation ) {
			case LimbIKLocation.LeftLeg:	return LimbIKType.Leg;
			case LimbIKLocation.RightLeg:	return LimbIKType.Leg;
			case LimbIKLocation.LeftArm:	return LimbIKType.Arm;
			case LimbIKLocation.RightArm:	return LimbIKType.Arm;
			}

			return LimbIKType.Unknown;
		}

		public static Side ToLimbIKSide( LimbIKLocation limbIKLocation )
		{
			switch( limbIKLocation ) {
			case LimbIKLocation.LeftLeg:	return Side.Left;
			case LimbIKLocation.RightLeg:	return Side.Right;
			case LimbIKLocation.LeftArm:	return Side.Left;
			case LimbIKLocation.RightArm:	return Side.Right;
			}

			return Side.None;
		}

		public enum BoneType
		{
			Hips,
			Spine,
			Neck,
			Head,

			Leg,
			Knee,
			Foot,

			Shoulder,
			Arm,
			ArmRoll,
			Elbow,
			ElbowRoll,
			Wrist,

			Max,
			Unknown = Max,
		}

		public enum BoneLocation
		{
			Hips,
			Spine,
			Spine2,
			Spine3,
			Spine4,
			Neck,
			Head,

			LeftLeg,
			RightLeg,
			LeftKnee,
			RightKnee,
			LeftFoot,
			RightFoot,

			LeftShoulder,
			RightShoulder,
			LeftArm,
			RightArm,
			LeftArmRoll,
			RightArmRoll,
			LeftHandRoll,
			RightHandRoll,
			LeftElbow,
			RightElbow,
			LeftElbowPole,
			RightElbowPole,
			LeftWrist,
			RightWrist,

			Max,
			Unknown = Max,
			SpineU = Spine4,
		}

		public const int MaxArmRollLength = 4;
		public const int MaxElbowRollLength = 4;

		public static BoneType ToBoneType(BoneLocation boneLocation)
		{
			switch (boneLocation)
			{
				case BoneLocation.Hips: return BoneType.Hips;
				case BoneLocation.Neck: return BoneType.Neck;
				case BoneLocation.Head: return BoneType.Head;

				case BoneLocation.LeftLeg: return BoneType.Leg;
				case BoneLocation.RightLeg: return BoneType.Leg;
				case BoneLocation.LeftKnee: return BoneType.Knee;
				case BoneLocation.RightKnee: return BoneType.Knee;
				case BoneLocation.LeftFoot: return BoneType.Foot;
				case BoneLocation.RightFoot: return BoneType.Foot;

				case BoneLocation.LeftShoulder: return BoneType.Shoulder;
				case BoneLocation.RightShoulder: return BoneType.Shoulder;
				case BoneLocation.LeftArm: return BoneType.Arm;
				case BoneLocation.RightArm: return BoneType.Arm;
				case BoneLocation.LeftArmRoll: return BoneType.ArmRoll;
				case BoneLocation.RightArmRoll: return BoneType.ArmRoll;
				case BoneLocation.LeftHandRoll: return BoneType.ArmRoll;
				case BoneLocation.RightHandRoll: return BoneType.ArmRoll;
				case BoneLocation.LeftElbow: return BoneType.Elbow;
				case BoneLocation.RightElbow: return BoneType.Elbow;
				case BoneLocation.LeftElbowPole: return BoneType.ElbowRoll;
				case BoneLocation.RightElbowPole: return BoneType.ElbowRoll;
				case BoneLocation.LeftWrist: return BoneType.Wrist;
				case BoneLocation.RightWrist: return BoneType.Wrist;
			}

			if ((int)boneLocation >= (int)BoneLocation.Spine &&
				(int)boneLocation <= (int)BoneLocation.SpineU)
			{
				return BoneType.Spine;
			}

			return BoneType.Unknown;
		}

		public static Side ToBoneSide(BoneLocation boneLocation)
		{
			switch (boneLocation)
			{
				case BoneLocation.LeftLeg: return Side.Left;
				case BoneLocation.RightLeg: return Side.Right;
				case BoneLocation.LeftKnee: return Side.Left;
				case BoneLocation.RightKnee: return Side.Right;
				case BoneLocation.LeftFoot: return Side.Left;
				case BoneLocation.RightFoot: return Side.Right;

				case BoneLocation.LeftShoulder: return Side.Left;
				case BoneLocation.RightShoulder: return Side.Right;
				case BoneLocation.LeftArm: return Side.Left;
				case BoneLocation.RightArm: return Side.Right;
				case BoneLocation.LeftArmRoll: return Side.Left;
				case BoneLocation.RightArmRoll: return Side.Right;
				case BoneLocation.LeftHandRoll: return Side.Left;
				case BoneLocation.RightHandRoll: return Side.Right;
				case BoneLocation.LeftElbow: return Side.Left;
				case BoneLocation.RightElbow: return Side.Right;
				case BoneLocation.LeftElbowPole: return Side.Left;
				case BoneLocation.RightElbowPole: return Side.Right;
				case BoneLocation.LeftWrist: return Side.Left;
				case BoneLocation.RightWrist: return Side.Right;
			}

			return Side.None;
		}

		public enum EffectorType
		{
			Root,
			Hips,
			Neck,
			Head,
			
			Knee,
			Foot,
			
			Arm,
			Elbow,
			Wrist,
			
			Max,
			Unknown = Max,
		}

		public enum EffectorLocation
		{
			Root,
			Hips,
			Neck,
			Head,
			
			LeftKnee,
			RightKnee,
			LeftFoot,
			RightFoot,
			
			LeftArm,
			RightArm,
			LeftElbow,
			RightElbow,
			LeftWrist,
			RightWrist,

			Max,
			Unknown = Max,
		}
		
		public static EffectorType ToEffectorType( EffectorLocation effectorLocation )
		{
			switch( effectorLocation ) {
			case EffectorLocation.Root:			return EffectorType.Root;
			case EffectorLocation.Hips:			return EffectorType.Hips;
			case EffectorLocation.Neck:			return EffectorType.Neck;
			case EffectorLocation.Head:			return EffectorType.Head;

			case EffectorLocation.LeftKnee:		return EffectorType.Knee;
			case EffectorLocation.RightKnee:	return EffectorType.Knee;
			case EffectorLocation.LeftFoot:		return EffectorType.Foot;
			case EffectorLocation.RightFoot:	return EffectorType.Foot;

			case EffectorLocation.LeftArm:		return EffectorType.Arm;
			case EffectorLocation.RightArm:		return EffectorType.Arm;
			case EffectorLocation.LeftElbow:	return EffectorType.Elbow;
			case EffectorLocation.RightElbow:	return EffectorType.Elbow;
			case EffectorLocation.LeftWrist:	return EffectorType.Wrist;
			case EffectorLocation.RightWrist:	return EffectorType.Wrist;
			}

			return EffectorType.Unknown;
		}

		public static Side ToEffectorSide( EffectorLocation effectorLocation )
		{
			switch( effectorLocation ) {
			case EffectorLocation.LeftKnee:		return Side.Left;
			case EffectorLocation.RightKnee:	return Side.Right;
			case EffectorLocation.LeftFoot:		return Side.Left;
			case EffectorLocation.RightFoot:	return Side.Right;

			case EffectorLocation.LeftArm:		return Side.Left;
			case EffectorLocation.RightArm:		return Side.Right;
			case EffectorLocation.LeftElbow:	return Side.Left;
			case EffectorLocation.RightElbow:	return Side.Right;
			case EffectorLocation.LeftWrist:	return Side.Left;
			case EffectorLocation.RightWrist:	return Side.Right;
			}

			return Side.None;
		}

		public static string GetEffectorName( EffectorLocation effectorLocation )
		{
			if( effectorLocation == EffectorLocation.Root ) {
				return "FullBodyIK";
			} else {
				return effectorLocation.ToString();
			}
		}
		
		public const float SimualteEys_NeckHeadDistanceScale = 1.0f;

		public enum _DirectionAs
		{
			None,
			XPlus,
			XMinus,
			YPlus,
			YMinus,
			Max,
			Uknown = Max,
		}
	}
}
