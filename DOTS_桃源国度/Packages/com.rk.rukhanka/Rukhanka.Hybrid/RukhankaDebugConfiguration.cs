using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Hybrid
{
public class RukhankaDebugConfiguration: MonoBehaviour
{
	[Header("Baking Systems")]
	public bool logRigDefinitionBaking;
	public bool logSkinnedMeshBaking;
	public bool logAnimatorBaking;
	public bool logClipBaking;

	[Header("Animator Controller System")]
	public bool logAnimatorControllerProcesses;

	[Header("Animation Process System")]
	public bool logAnimationCalculationProcesses;

	[Header("Bone Visualization")]
	public bool visualizeAllRigs;
	public Color boneColor = new Color(0, 1, 1, 0.3f);
	public Color outlineColor = new Color(0, 1, 1, 1);
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public class DebugConfigurationBaker: Baker<RukhankaDebugConfiguration>
{
	public override void Bake(RukhankaDebugConfiguration a)
	{
		var dcc = new DebugConfigurationComponent()
		{
			logAnimatorBaking = a.logAnimatorBaking,
			logAnimatorControllerProcesses = a.logAnimatorControllerProcesses,
			logAnimationCalculationProcesses = a.logAnimationCalculationProcesses,
			logClipBaking = a.logClipBaking,
			logRigDefinitionBaking = a.logRigDefinitionBaking,
			logSkinnedMeshBaking = a.logSkinnedMeshBaking,

			visualizeAllRigs = a.visualizeAllRigs,
			colorLines = new float4(a.outlineColor.r, a.outlineColor.g, a.outlineColor.b, a.outlineColor.a),
			colorTri = new float4(a.boneColor.r, a.boneColor.g, a.boneColor.b, a.boneColor.a),
		};

		var e = GetEntity(TransformUsageFlags.None);
		AddComponent(e, dcc);
	}
}
}

