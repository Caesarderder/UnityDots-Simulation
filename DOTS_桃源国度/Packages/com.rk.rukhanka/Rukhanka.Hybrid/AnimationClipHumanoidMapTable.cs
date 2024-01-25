#if UNITY_EDITOR

using System.Collections.Generic;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Hybrid
{
public partial class AnimationClipBaker
{
	static Dictionary<string, ParsedCurveBinding> humanoidMappingTable;
	static Dictionary<string, string> humanoidMuscleNameFromCurveProperty;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static AnimationClipBaker()
	{
		humanoidMappingTable = new ()
		{
			//	--- Head ---
			//	Neck
			{ "Neck Nod Down-Up",				new ParsedCurveBinding() { boneName = "Neck", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Neck Tilt Left-Right",			new ParsedCurveBinding() { boneName = "Neck", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Neck Turn Left-Right",			new ParsedCurveBinding() { boneName = "Neck", channelIndex = 0, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Head
			{ "Head Nod Down-Up",				new ParsedCurveBinding() { boneName = "Head", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Head Tilt Left-Right",			new ParsedCurveBinding() { boneName = "Head", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Head Turn Left-Right",			new ParsedCurveBinding() { boneName = "Head", channelIndex = 0, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Left Eye
			{ "Left Eye Down-Up",				new ParsedCurveBinding() { boneName = "LeftEye", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Left Eye In-Out",				new ParsedCurveBinding() { boneName = "LeftEye", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Right Eye
			{ "Right Eye Down-Up",				new ParsedCurveBinding() { boneName = "RightEye", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Right Eye In-Out",				new ParsedCurveBinding() { boneName = "RightEye", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Jaw
			{ "Jaw Close",						new ParsedCurveBinding() { boneName = "Jaw", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Jaw Left-Right",					new ParsedCurveBinding() { boneName = "Jaw", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},

			//	--- Body ---
			//	Spine
			{ "Spine Front-Back",				new ParsedCurveBinding() { boneName = "Spine", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Spine Left-Right",				new ParsedCurveBinding() { boneName = "Spine", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Spine Twist Left-Right",			new ParsedCurveBinding() { boneName = "Spine", channelIndex = 0, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Chest
			{ "Chest Front-Back",				new ParsedCurveBinding() { boneName = "Chest", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Chest Left-Right",				new ParsedCurveBinding() { boneName = "Chest", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Chest Twist Left-Right",			new ParsedCurveBinding() { boneName = "Chest", channelIndex = 0, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	UpperChest
			{ "UpperChest Front-Back",			new ParsedCurveBinding() { boneName = "UpperChest", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "UpperChest Left-Right",			new ParsedCurveBinding() { boneName = "UpperChest", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "UpperChest Twist Left-Right",	new ParsedCurveBinding() { boneName = "UpperChest", channelIndex = 0, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},

			//	--- Left Arm ---
			//	LeftShoulder
			{ "Left Shoulder Down-Up",			new ParsedCurveBinding() { boneName = "LeftShoulder", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Left Shoulder Front-Back",		new ParsedCurveBinding() { boneName = "LeftShoulder", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	LeftUpperArm
			{ "Left Arm Down-Up",				new ParsedCurveBinding() { boneName = "LeftUpperArm", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Left Arm Front-Back",			new ParsedCurveBinding() { boneName = "LeftUpperArm", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Left Arm Twist In-Out",			new ParsedCurveBinding() { boneName = "LeftUpperArm", channelIndex = 0, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	LeftLowerArm
			{ "Left Forearm Stretch",			new ParsedCurveBinding() { boneName = "LeftLowerArm", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Left Forearm Twist In-Out",		new ParsedCurveBinding() { boneName = "LeftLowerArm", channelIndex = 0, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	LeftHand
			{ "Left Hand Down-Up",				new ParsedCurveBinding() { boneName = "LeftHand", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Left Hand In-Out",				new ParsedCurveBinding() { boneName = "LeftHand", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},

			//	--- Left Hand ---
			//	Thumb 1
			{ "LeftHand.Thumb.1 Stretched",		new ParsedCurveBinding() { boneName = "Left Thumb Proximal", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "LeftHand.Thumb Spread",			new ParsedCurveBinding() { boneName = "Left Thumb Proximal", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Thumb 2
			{ "LeftHand.Thumb.2 Stretched",		new ParsedCurveBinding() { boneName = "Left Thumb Intermediate", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Thumb 3
			{ "LeftHand.Thumb.3 Stretched",		new ParsedCurveBinding() { boneName = "Left Thumb Distal", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Index 1
			{ "LeftHand.Index.1 Stretched",		new ParsedCurveBinding() { boneName = "Left Index Proximal", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "LeftHand.Index.Spread",			new ParsedCurveBinding() { boneName = "Left Index Proximal", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Index 2
			{ "LeftHand.Index.2 Stretched",		new ParsedCurveBinding() { boneName = "Left Index Intermediate", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Index 3
			{ "LeftHand.Index.3 Stretched",		new ParsedCurveBinding() { boneName = "Left Index Distal", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Middle 1
			{ "LeftHand.Middle.1 Stretched",	new ParsedCurveBinding() { boneName = "Left Middle Proximal", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "LeftHand.Middle.Spread",			new ParsedCurveBinding() { boneName = "Left Middle Proximal", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Middle 2
			{ "LeftHand.Middle.2 Stretched",	new ParsedCurveBinding() { boneName = "Left Middle Intermediate", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Middle 3
			{ "LeftHand.Middle.3 Stretched",	new ParsedCurveBinding() { boneName = "Left Middle Distal", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Ring 1
			{ "LeftHand.Ring.1 Stretched",		new ParsedCurveBinding() { boneName = "Left Ring Proximal", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "LeftHand.Ring.Spread",			new ParsedCurveBinding() { boneName = "Left Ring Proximal", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Ring 2
			{ "LeftHand.Ring.2 Stretched",		new ParsedCurveBinding() { boneName = "Left Ring Intermediate", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Ring 3
			{ "LeftHand.Ring.3 Stretched",		new ParsedCurveBinding() { boneName = "Left Ring Distal", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Little 1
			{ "LeftHand.Little.1 Stretched",	new ParsedCurveBinding() { boneName = "Left Little Proximal", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "LeftHand.Little.Spread",			new ParsedCurveBinding() { boneName = "Left Little Proximal", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Little 2
			{ "LeftHand.Little.2 Stretched",	new ParsedCurveBinding() { boneName = "Left Little Intermediate", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Little 3
			{ "LeftHand.Little.3 Stretched",	new ParsedCurveBinding() { boneName = "Left Little Distal", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},

			//	--- Right Arm ---
			//	RightShoulder
			{ "Right Shoulder Down-Up",			new ParsedCurveBinding() { boneName = "RightShoulder", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Right Shoulder Front-Back",		new ParsedCurveBinding() { boneName = "RightShoulder", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	RightUpperArm
			{ "Right Arm Down-Up",				new ParsedCurveBinding() { boneName = "RightUpperArm", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Right Arm Front-Back",			new ParsedCurveBinding() { boneName = "RightUpperArm", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Right Arm Twist In-Out",			new ParsedCurveBinding() { boneName = "RightUpperArm", channelIndex = 0, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	RightLowerArm
			{ "Right Forearm Stretch",			new ParsedCurveBinding() { boneName = "RightLowerArm", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Right Forearm Twist In-Out",		new ParsedCurveBinding() { boneName = "RightLowerArm", channelIndex = 0, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	RightHand
			{ "Right Hand Down-Up",				new ParsedCurveBinding() { boneName = "RightHand", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Right Hand In-Out",				new ParsedCurveBinding() { boneName = "RightHand", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},

			//	--- Right Hand ---
			//	Thumb 1
			{ "RightHand.Thumb.1 Stretched",	new ParsedCurveBinding() { boneName = "Right Thumb Proximal", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "RightHand.Thumb Spread",			new ParsedCurveBinding() { boneName = "Right Thumb Proximal", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Thumb 2
			{ "RightHand.Thumb.2 Stretched",	new ParsedCurveBinding() { boneName = "Right Thumb Intermediate", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Thumb 3
			{ "RightHand.Thumb.3 Stretched",	new ParsedCurveBinding() { boneName = "Right Thumb Distal", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Index 1
			{ "RightHand.Index.1 Stretched",	new ParsedCurveBinding() { boneName = "Right Index Proximal", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "RightHand.Index.Spread",			new ParsedCurveBinding() { boneName = "Right Index Proximal", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Index 2
			{ "RightHand.Index.2 Stretched",	new ParsedCurveBinding() { boneName = "Right Index Intermediate", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Index 3
			{ "RightHand.Index.3 Stretched",	new ParsedCurveBinding() { boneName = "Right Index Distal", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Middle 1
			{ "RightHand.Middle.1 Stretched",	new ParsedCurveBinding() { boneName = "Right Middle Proximal", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "RightHand.Middle.Spread",		new ParsedCurveBinding() { boneName = "Right Middle Proximal", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Middle 2
			{ "RightHand.Middle.2 Stretched",	new ParsedCurveBinding() { boneName = "Right Middle Intermediate", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Middle 3
			{ "RightHand.Middle.3 Stretched",	new ParsedCurveBinding() { boneName = "Right Middle Distal", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Ring 1
			{ "RightHand.Ring.1 Stretched",		new ParsedCurveBinding() { boneName = "Right Ring Proximal", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "RightHand.Ring.Spread",			new ParsedCurveBinding() { boneName = "Right Ring Proximal", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Ring 2
			{ "RightHand.Ring.2 Stretched",		new ParsedCurveBinding() { boneName = "Right Ring Intermediate", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Ring 3
			{ "RightHand.Ring.3 Stretched",		new ParsedCurveBinding() { boneName = "Right Ring Distal", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Little 1
			{ "RightHand.Little.1 Stretched",	new ParsedCurveBinding() { boneName = "Right Little Proximal", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "RightHand.Little.Spread",			new ParsedCurveBinding() { boneName = "Right Little Proximal", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Little 2
			{ "RightHand.Little.2 Stretched",	new ParsedCurveBinding() { boneName = "Right Little Intermediate", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	Little 3
			{ "RightHand.Little.3 Stretched",	new ParsedCurveBinding() { boneName = "Right Little Distal", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},

			//	--- Left Leg ---
			//	LeftUpperLeg
			{ "Left Upper Leg Front-Back",		new ParsedCurveBinding() { boneName = "LeftUpperLeg", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Left Upper Leg In-Out",			new ParsedCurveBinding() { boneName = "LeftUpperLeg", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Left Upper Leg Twist In-Out",	new ParsedCurveBinding() { boneName = "LeftUpperLeg", channelIndex = 0, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	LeftLowerLeg
			{ "Left Lower Leg Stretch",			new ParsedCurveBinding() { boneName = "LeftLowerLeg", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Left Lower Leg Twist In-Out",	new ParsedCurveBinding() { boneName = "LeftLowerLeg", channelIndex = 0, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	LeftFoot
			{ "Left Foot Up-Down",				new ParsedCurveBinding() { boneName = "LeftFoot", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Left Foot Twist In-Out",			new ParsedCurveBinding() { boneName = "LeftFoot", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	LeftToes
			{ "Left Toes Up-Down",				new ParsedCurveBinding() { boneName = "LeftHand", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},

			//	--- Right Leg ---
			//	RightUpperLeg
			{ "Right Upper Leg Front-Back",		new ParsedCurveBinding() { boneName = "RightUpperLeg", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Right Upper Leg In-Out",			new ParsedCurveBinding() { boneName = "RightUpperLeg", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Right Upper Leg Twist In-Out",	new ParsedCurveBinding() { boneName = "RightUpperLeg", channelIndex = 0, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	RightLowerLeg
			{ "Right Lower Leg Stretch",		new ParsedCurveBinding() { boneName = "RightLowerLeg", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Right Lower Leg Twist In-Out",	new ParsedCurveBinding() { boneName = "RightLowerLeg", channelIndex = 0, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	RightFoot
			{ "Right Foot Up-Down",				new ParsedCurveBinding() { boneName = "RightFoot", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			{ "Right Foot Twist In-Out",		new ParsedCurveBinding() { boneName = "RightFoot", channelIndex = 1, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},
			//	RightToes
			{ "Right Toes Up-Down",				new ParsedCurveBinding() { boneName = "RightHand", channelIndex = 2, bindingType = BindingType.HumanMuscle, boneType = BoneType.Generic }},

			// --- Special Bones ---
			/*
			{ "RootT.x",						new ParsedCurveBinding() { boneName = SpecialBones.unnamedRootBoneName, channelIndex = 0, bindingType = BindingType.Translation, boneType = BoneType.RootCurve }},
			{ "RootT.y",						new ParsedCurveBinding() { boneName = SpecialBones.unnamedRootBoneName, channelIndex = 1, bindingType = BindingType.Translation, boneType = BoneType.RootCurve }},
			{ "RootT.z",						new ParsedCurveBinding() { boneName = SpecialBones.unnamedRootBoneName, channelIndex = 2, bindingType = BindingType.Translation, boneType = BoneType.RootCurve }},
			{ "RootQ.x",						new ParsedCurveBinding() { boneName = SpecialBones.unnamedRootBoneName, channelIndex = 0, bindingType = BindingType.Quaternion, boneType = BoneType.RootCurve }},
			{ "RootQ.y",						new ParsedCurveBinding() { boneName = SpecialBones.unnamedRootBoneName, channelIndex = 1, bindingType = BindingType.Quaternion, boneType = BoneType.RootCurve }},
			{ "RootQ.z",						new ParsedCurveBinding() { boneName = SpecialBones.unnamedRootBoneName, channelIndex = 2, bindingType = BindingType.Quaternion, boneType = BoneType.RootCurve }},
			{ "RootQ.w",						new ParsedCurveBinding() { boneName = SpecialBones.unnamedRootBoneName, channelIndex = 3, bindingType = BindingType.Quaternion, boneType = BoneType.RootCurve }},
			*/
		};
	}
}
}

#endif
