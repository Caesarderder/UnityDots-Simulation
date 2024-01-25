using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	class ForceIncludeInstancing : IPreprocessShaders {

		readonly ShaderKeyword inst;

		public ForceIncludeInstancing() => inst = new ShaderKeyword( "INSTANCING_ON" );
		public int callbackOrder => 0;

		public void OnProcessShader( Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data ) {
			if( shader.name.StartsWith( "Shapes/" ) == false )
				return; // ignore all non-Shapes shaders

			// Shapes immediate mode has to force instancing on.
			// find variants that don't have an instancing counterpart, copy them, and add instancing
			string GetKeywordsStrWithoutInstancing( ShaderCompilerData set ) {
				return string.Join( ",", set.shaderKeywordSet.GetShaderKeywords()
				#if UNITY_2021_2_OR_NEWER
					.Select( a => a.name ).Where( a => a != inst.name )
				#elif UNITY_2019_3_OR_NEWER
					.Select( ShaderKeyword.GetGlobalKeywordName ).Where( a => a != ShaderKeyword.GetGlobalKeywordName( inst ) )
				#else
					.Select( a => a.GetKeywordName() ).Where( a => a != inst.GetKeywordName() )
				#endif
					.OrderBy( a => a ) );
			}

			HashSet<string> thingsWithInstancing = new HashSet<string>( data.Where( x => x.shaderKeywordSet.IsEnabled( inst ) ).Select( GetKeywordsStrWithoutInstancing ) );
			HashSet<string> thingsWithoutInstancing = new HashSet<string>( data.Where( x => !x.shaderKeywordSet.IsEnabled( inst ) ).Select( GetKeywordsStrWithoutInstancing ) );
			thingsWithoutInstancing.ExceptWith( thingsWithInstancing ); // filter out only the ones missing instancing versions
			List<ShaderCompilerData> thingsToClone = data.Where( x => !x.shaderKeywordSet.IsEnabled( inst ) && thingsWithoutInstancing.Contains( GetKeywordsStrWithoutInstancing( x ) ) ).ToList();
			foreach( ShaderCompilerData thing in thingsToClone ) {
				ShaderCompilerData copy = thing;
				copy.shaderKeywordSet.Enable( inst );
				data.Add( copy );
			}
		}
	}

}