#if GRIFFIN
#if GRIFFIN_ASE
using UnityEngine;
using UnityEditor;
using System;

namespace AmplifyShaderEditor
{
    [Serializable]
    [NodeAttributes("Get World To Normalized Matrix", "Griffin", "Get transform matrix for Interactive Grass feature.")]
    public class GGetWorldToNormalizedMatrixNode : ParentNode
    {
        protected override void CommonInit(int uniqueId)
        {
            base.CommonInit(uniqueId);
            AddOutputPort(WirePortDataType.FLOAT4x4, "World To Normalized");
            m_insideSize.Set(50, 25);
        }

        public override string GenerateShaderForOutput(int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar)
        {
            //// If local variable is already created then you need only to re-use it
            //if (m_outputPorts[0].IsLocalValue(MasterNodePortCategory.Vertex))
            //    return m_outputPorts[0].LocalValue(MasterNodePortCategory.Vertex);

            dataCollector.AddToUniforms(UniqueId, "float4x4", "_WorldToNormalized");

            WirePortDataType mainType = m_outputPorts[0].DataType;
            string finalCalculation = string.Format("_WorldToNormalized");

            //Register the final operation on a local variable associated with our output port
            RegisterLocalVariable(0, finalCalculation, ref dataCollector, "myLocalVar" + OutputId);

            // Use the newly created local variable
            return m_outputPorts[0].LocalValue(MasterNodePortCategory.Vertex);
        }
    }
}
#endif
#endif
