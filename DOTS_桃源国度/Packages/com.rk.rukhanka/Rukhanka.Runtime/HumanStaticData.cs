
using Unity.Mathematics;
using UnityEngine;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{
public static class HumanStaticData
{
	static readonly float bodyDefaultMass = 82.5f;

	public static readonly float[] bodyPartsMasses = 
	{
		12 / bodyDefaultMass,
		10 / bodyDefaultMass,
		10 / bodyDefaultMass,
		4 / bodyDefaultMass,
		4 / bodyDefaultMass,
		0.8f / bodyDefaultMass,
		0.8f / bodyDefaultMass,
		2.5f / bodyDefaultMass,
		12 / bodyDefaultMass,
		12 / bodyDefaultMass,
		1 / bodyDefaultMass,
		4 / bodyDefaultMass,
		0.5f / bodyDefaultMass,
		0.5f / bodyDefaultMass,
		2 / bodyDefaultMass,
		2 / bodyDefaultMass,
		1.5f / bodyDefaultMass,
		1.5f / bodyDefaultMass,
		0.5f / bodyDefaultMass,
		0.5f / bodyDefaultMass,
		0.2f / bodyDefaultMass,
		0.2f / bodyDefaultMass,
	};

	public static readonly int4[] massIndicesTable = 
	{
		new ((int)HumanBodyBones.LeftUpperLeg, (int)HumanBodyBones.RightUpperLeg, (int)HumanBodyBones.Spine, -1),
		new ((int)HumanBodyBones.LeftUpperLeg, (int)HumanBodyBones.LeftLowerLeg, -1, -1),
		new ((int)HumanBodyBones.RightUpperLeg, (int)HumanBodyBones.RightLowerLeg, -1, -1),
		new ((int)HumanBodyBones.LeftLowerLeg, (int)HumanBodyBones.LeftFoot, -1, -1),
		new ((int)HumanBodyBones.RightLowerLeg, (int)HumanBodyBones.RightFoot, -1, -1),
		new ((int)HumanBodyBones.LeftFoot, -1, -1, -1),
		new ((int)HumanBodyBones.RightFoot, -1, -1, -1),
		new ((int)HumanBodyBones.Spine, (int)HumanBodyBones.Chest, -1, -1),
		new ((int)HumanBodyBones.UpperChest, (int)HumanBodyBones.Chest, -1, -1),
		new ((int)HumanBodyBones.UpperChest, (int)HumanBodyBones.Neck, (int)HumanBodyBones.LeftShoulder, (int)HumanBodyBones.RightShoulder),
		new ((int)HumanBodyBones.Neck, (int)HumanBodyBones.Head, -1, 1),
		new ((int)HumanBodyBones.Head, -1, -1, 1),
		new ((int)HumanBodyBones.LeftShoulder, (int)HumanBodyBones.LeftUpperArm, -1, 1),
		new ((int)HumanBodyBones.RightShoulder, (int)HumanBodyBones.RightUpperArm, -1, 1),
		new ((int)HumanBodyBones.LeftLowerArm, (int)HumanBodyBones.LeftUpperArm, -1, 1),
		new ((int)HumanBodyBones.RightLowerArm, (int)HumanBodyBones.RightUpperArm, -1, 1),
		new ((int)HumanBodyBones.LeftLowerArm, (int)HumanBodyBones.LeftHand, -1, 1),
		new ((int)HumanBodyBones.RightLowerArm, (int)HumanBodyBones.RightHand, -1, 1),
		new ((int)HumanBodyBones.LeftHand, -1, -1, 1),
		new ((int)HumanBodyBones.RightHand, -1, -1, 1),
		new ((int)HumanBodyBones.LeftToes, -1, -1, 1),
		new ((int)HumanBodyBones.RightToes, -1, -1, 1),
	};
}
}
