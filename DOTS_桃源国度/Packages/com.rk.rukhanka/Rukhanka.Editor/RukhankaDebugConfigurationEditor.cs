using Rukhanka.Hybrid;
using UnityEditor;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Editor
{
[CustomEditor(typeof(RukhankaDebugConfiguration))]
public class RukhankaDebugConfigutationEditor: UnityEditor.Editor
{
	public override void OnInspectorGUI()
	{
	#if RUKHANKA_DEBUG_INFO
		DrawDefaultInspector();
	#else
		EditorGUILayout.HelpBox("No RUKHANKA_DEBUG_INFO scripting symbol defined\n\nWithout this definition all debug, info and validation data stripped out from Rukhanka source code", MessageType.Warning);
	#endif
	}
}
}
