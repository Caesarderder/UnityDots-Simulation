using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Editor
{
[InitializeOnLoad]
public class RukhankaEditorAutorun
{
	static RukhankaEditorAutorun()
	{
#if !UNITY_BURST_EXPERIMENTAL_ATOMIC_INTRINSICS
		//	Add obligatory UNITY_BURST_EXPERIMENTAL_ATOMIC_INTRINSICS script symbol
		var bt = GetCurrentBuildTarget();
		var defines = PlayerSettings.GetScriptingDefineSymbols(bt);
		var burstExperimentalDefine = "UNITY_BURST_EXPERIMENTAL_ATOMIC_INTRINSICS";
		defines += ";" + burstExperimentalDefine;
		PlayerSettings.SetScriptingDefineSymbols(bt, defines);
#endif
	}
	
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static NamedBuildTarget GetCurrentBuildTarget()
	{
#if UNITY_SERVER
		return NamedBuildTarget.Server;
#else
		var bt = EditorUserBuildSettings.activeBuildTarget;
		var btg = BuildPipeline.GetBuildTargetGroup(bt);
		var rv = NamedBuildTarget.FromBuildTargetGroup(btg);
		return rv;
#endif
	}
}
}
