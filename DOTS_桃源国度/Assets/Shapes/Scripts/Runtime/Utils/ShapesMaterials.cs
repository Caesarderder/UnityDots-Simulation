using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	internal class ShapesMaterials {

		const bool USE_INSTANCING = true;
		public const string SHAPES_SHADER_PATH_PREFIX = "Shapes/";


		readonly Material[] materials;
		public Material this[ ShapesBlendMode type ] => materials[(int)type];

		public ShapesMaterials( string shaderName, params string[] keywords ) {
			int count = Enum.GetNames( typeof(ShapesBlendMode) ).Length;
			materials = new Material[count];
			for( int i = 0; i < count; i++ )
				materials[i] = InitMaterial( shaderName, ( (ShapesBlendMode)i ).ToString(), keywords );
		}

		public static string GetMaterialName( string shaderName, string blendModeSuffix, params string[] keywords ) {
			string keywordsSuffix = "";
			if( keywords != null && keywords.Length > 0 ) {
				keywordsSuffix = $" [{string.Join( "][", keywords )}]";
			}

			return $"{shaderName} {blendModeSuffix}{keywordsSuffix}";
		}

		public static void ApplyDefaultGlobalProperties( Material mat ) {
			// set default properties
			// todo: this seeeems unnecessary, not sure why this exists
			mat.SetInt_Shapes( ShapesMaterialUtils.propZTest, (int)ShapeRenderer.DEFAULT_ZTEST );
			mat.SetFloat( ShapesMaterialUtils.propZOffsetFactor, ShapeRenderer.DEFAULT_ZOFS_FACTOR );
			mat.SetInt_Shapes( ShapesMaterialUtils.propZOffsetUnits, ShapeRenderer.DEFAULT_ZOFS_UNITS );
			mat.SetInt_Shapes( ShapesMaterialUtils.propColorMask, (int)ShapeRenderer.DEFAULT_COLOR_MASK );
		}

		static Material CreateShapesMaterial( Shader shader, HideFlags hideFlags, params string[] keywords ) {
			Material mat = new Material( shader ) { hideFlags = hideFlags, enableInstancing = USE_INSTANCING };
			if( keywords != null )
				foreach( string keyword in keywords )
					mat.EnableKeyword( keyword );
			ApplyDefaultGlobalProperties( mat );
			return mat;
		}

		static Material InitMaterial( string shaderName, string blendModeSuffix, params string[] keywords ) {
			#if UNITY_EDITOR
			// in editor, we want to use the material *assets*, not create any materials
			string path = $"{ShapesIO.GeneratedMaterialsFolder}/{GetMaterialName( shaderName, blendModeSuffix, keywords )}.mat";
			Material mat = AssetDatabase.LoadAssetAtPath<Material>( path );
			if( mat == null )
				Debug.LogWarning( "Failed to load material " + path );
			return mat;

			#else
				// in builds, we want to create them
				shaderName = SHAPES_SHADER_PATH_PREFIX + shaderName + " " + blendModeSuffix;
				Shader shaderObj = Shader.Find( shaderName );
				if( shaderObj == null ) {
					Debug.LogError( "Could not find shader " + shaderName );
					return null;
				}

				return CreateShapesMaterial( shaderObj, HideFlags.HideAndDontSave, keywords );
			#endif
		}


	}

}