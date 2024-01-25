#if AMPLIFY_SHADER_EDITOR

using UnityEngine;
using System;
using AmplifyShaderEditor;
using static Rukhanka.Editor.ASE_ShaderCodeTemplates;

/////////////////////////////////////////////////////////////////////////////////

namespace AmplifyShaderEditor
{
[Serializable]
[NodeAttributes("Compute Deformation", "Rukhanka", "Compute deformation node")]
public class Rukhanka_ComputeDeformationNode: ParentNode
{
	protected override void CommonInit(int uniqueID)
	{
		base.CommonInit(uniqueID);

		AddInputPort(WirePortDataType.INT, true, "Vertex ID");
		AddOutputPort(WirePortDataType.FLOAT3, "Deformed Position");
		AddOutputPort(WirePortDataType.FLOAT3, "Deformed Normal");
		AddOutputPort(WirePortDataType.FLOAT3, "Deformed Tangent");
	}

/////////////////////////////////////////////////////////////////////////////////

	public override string GenerateShaderForOutput(int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar)
	{
	#if ENABLE_DOTS_DEFORMATION_MOTION_VECTORS
		var instancingPropType = "float4";
		var instancingPropName = "_DotsDeformationParams";
		var instancingPropDefaultValue = "(0, 0, 0, 0)";
		var instancingPropHeaderType = "Vector";
		dataCollector.AddFunction(computeDeformationFNHeader, computeDeformationFNBody_MotionVecs);
	#else
		var instancingPropType = "float";
		var instancingPropName = "_ComputeMeshIndex";
		var instancingPropDefaultValue = "0";
		var instancingPropHeaderType = "Float";
		dataCollector.AddFunction(computeDeformationFNHeader, computeDeformationFNBody);
	#endif
		var paramString = String.Format(parameterProp, instancingPropName, instancingPropHeaderType, instancingPropDefaultValue);
		var directiveString = String.Format(dotsInstancingDefines, instancingPropType, instancingPropName);
		dataCollector.AddToUniforms(UniqueId, instancingPropType, instancingPropName, true);
		dataCollector.AddToDirectives(directiveString, 100);
		dataCollector.AddToProperties(UniqueId, paramString, 0);
		dataCollector.AddToVertexLocalVariables(UniqueId, PrecisionType.Float, WirePortDataType.FLOAT3, "deformedPos", "0");
		dataCollector.AddToVertexLocalVariables(UniqueId, PrecisionType.Float, WirePortDataType.FLOAT3, "deformedNrm", "0");
		dataCollector.AddToVertexLocalVariables(UniqueId, PrecisionType.Float, WirePortDataType.FLOAT3, "deformedTan", "0");
		dataCollector.AddVertexInstruction(dataCollector.VertexLocalVariables, UniqueId, false);
		dataCollector.ClearVertexLocalVariables();

		var str = m_outputPorts[0].LocalValue(MasterNodePortCategory.Vertex);

		var vertexIdName = m_inputPorts[0].GenerateShaderForOutput(ref dataCollector, WirePortDataType.INT, true);
		dataCollector.AddVertexInstruction($"#if defined(UNITY_DOTS_INSTANCING_ENABLED)");
	#if ENABLE_DOTS_DEFORMATION_MOTION_VECTORS
		dataCollector.AddVertexInstruction($"ApplyDeformedVertexData({vertexIdName}, deformedPos, deformedNrm, deformedTan);");
	#else
		dataCollector.AddVertexInstruction($"ComputeDeformedVertex({vertexIdName}, deformedPos, deformedNrm, deformedTan);");
	#endif
		dataCollector.AddVertexInstruction($"#endif");
		var rv = outputId switch 
		{
			0 => "deformedPos",
			1 => "deformedNrm",
			2 => "deformedTan",
			_ => "error"
		};
		return rv;
	}
}
}

#endif