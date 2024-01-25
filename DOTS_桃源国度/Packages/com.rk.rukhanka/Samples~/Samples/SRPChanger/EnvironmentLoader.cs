using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Samples
{
[ExecuteAlways]
public class EnvironmentLoader: MonoBehaviour
{
	public Material urpSkybox;
	
/////////////////////////////////////////////////////////////////////////////////

	void OnEnable()
	{
		var srpName = GraphicsSettings.currentRenderPipeline.name;
		var switchToURP = !srpName.Contains("HDRP");

		if (switchToURP)
		{
			ConfigureURPScene();
			
			if (!Application.isPlaying)
				ChangeMaterialShadersToURP();
		}
	}
	
/////////////////////////////////////////////////////////////////////////////////

	void ConfigureURPScene()
	{
		var dl = FindObjectOfType<Light>();
		if (dl != null)
		{
			dl.intensity = 0.9f;
			RenderSettings.skybox = urpSkybox;
		}
	}
	
/////////////////////////////////////////////////////////////////////////////////

	void ChangeMaterialShadersToURP()
	{
#if UNITY_EDITOR
		var shaderReplacementTable = new (string, string)[]
		{
			("HDRP/Lit", "Shader Graphs/Lit URP"),
			("Shader Graphs/AnimatedLitShader HDRP", "Shader Graphs/AnimatedLitShader URP"),
			("Hidden/InternalErrorShader", "Shader Graphs/Lit URP")
		};
		
		var sampleMaterials = AssetDatabase.FindAssets("t:material", new[] { "Assets/Samples/Rukhanka Animation System" });
		foreach (var sa in sampleMaterials)
		{
			var matPath = AssetDatabase.GUIDToAssetPath(sa);
			Debug.Log(matPath);
			var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
			if (mat != null)
			{
				Debug.Log(mat.shader.name);

				var foundIndex = Array.FindIndex(shaderReplacementTable, x => x.Item1 == mat.shader.name);
				if (foundIndex < 0)
					continue;
				
				var (sourceShaderName, replacementShaderName) = shaderReplacementTable[foundIndex];
				
				Debug.Log($"Replacing {sourceShaderName} with {replacementShaderName}");	
				var newShader = Shader.Find(replacementShaderName);
				if (newShader == null)
					Debug.Log($"Replacement shader not found!");
				mat.shader = newShader;
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
		}
#endif
	}
}
}
