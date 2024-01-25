
namespace Rukhanka.Editor
{
public static class ASE_ShaderCodeTemplates
{
	public static readonly string computeDeformationFNHeader = "DeformedVertexData";
	public static readonly string computeDeformationFNBody_MotionVecs =
@"#if defined(UNITY_DOTS_INSTANCING_ENABLED)
#define DOTS_DEFORMED
#include ""Packages/com.unity.entities.graphics/Unity.Entities.Graphics/Deformations/ShaderLibrary/DotsDeformation.hlsl""
#endif";

	public static readonly string computeDeformationFNBody =
@"struct DeformedVertexData
{
	float3 position;
	float3 normal;
	float3 tangent;
};
uniform StructuredBuffer<DeformedVertexData> _DeformedMeshData: register(t1);

void ComputeDeformedVertex(uint vertexId, out float3 p, out float3 n, float3 t)
{
	const DeformedVertexData vertexData = _DeformedMeshData[asuint(UNITY_ACCESS_HYBRID_INSTANCED_PROP(_ComputeMeshIndex, float)) + vertexId];
	p = vertexData.position;
	n = vertexData.normal;
	t = vertexData.tangent;
}";

	public static readonly string parameterProp = @"[HideInInspector]{0}(""Compute Mesh Buffer Index Offset"", {1}) = {2}";
	public static readonly string dotsInstancingDefines =
@"#if defined(DOTS_INSTANCING_ON)
// DOTS instancing definitions
UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
	UNITY_DOTS_INSTANCED_PROP({0}, {1})
UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
// DOTS instancing usage macros
#define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(type, var)
#elif defined(UNITY_INSTANCING_ENABLED)
// Unity instancing definitions
UNITY_INSTANCING_BUFFER_START(SGPerInstanceData)
	UNITY_DEFINE_INSTANCED_PROP({0}, {1})
UNITY_INSTANCING_BUFFER_END(SGPerInstanceData)
// Unity instancing usage macros
#define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_INSTANCED_PROP(SGPerInstanceData, var)
#else
#define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) var
#endif";
}
}
