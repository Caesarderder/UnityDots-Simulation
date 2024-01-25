using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Hybrid
{
class BoneVisualizationAuthoring: MonoBehaviour
{
	public Color boneColor = new Color(0, 1, 1, 0.3f);
	public Color outlineColor = new Color(0, 1, 1, 1);
}

/////////////////////////////////////////////////////////////////////////////////

class BoneVisualizationBaker: Baker<BoneVisualizationAuthoring>
{
	public override void Bake(BoneVisualizationAuthoring a)
	{
		var bvc = new BoneVisualizationComponent()
		{
			colorTri = new float4(a.boneColor.r, a.boneColor.g, a.boneColor.b, a.boneColor.a),
			colorLines = new float4(a.outlineColor.r, a.outlineColor.g, a.outlineColor.b, a.outlineColor.a)
		};

		var e = GetEntity(TransformUsageFlags.Dynamic);
		AddComponent(e, bvc);
	}
}
}
