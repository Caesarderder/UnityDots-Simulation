using System;
using System.Collections.Generic;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	internal static class RenderPipelineExtensions {

		static string PipelineSubshaderTagValue( this RenderPipeline rp ) {
			switch( rp ) {
				case RenderPipeline.Legacy: return "";
				case RenderPipeline.URP:    return "UniversalPipeline";
				case RenderPipeline.HDRP:   return "HDRenderPipeline";
				default:                    throw new ArgumentOutOfRangeException();
			}
		}

		public static string PreprocessorDefineName( this RenderPipeline rp ) {
			switch( rp ) {
				case RenderPipeline.Legacy: return "SHAPES_BIRP";
				case RenderPipeline.URP:    return "SHAPES_URP";
				case RenderPipeline.HDRP:   return "SHAPES_HDRP";
				default:                    throw new ArgumentOutOfRangeException();
			}
		}

		public static string PrettyName( this RenderPipeline rp ) {
			switch( rp ) {
				case RenderPipeline.Legacy: return "the built-in render pipeline";
				case RenderPipeline.URP:    return "URP";
				case RenderPipeline.HDRP:   return "HDRP";
				default:                    throw new ArgumentOutOfRangeException();
			}
		}

		public static string ShortName( this RenderPipeline rp ) {
			switch( rp ) {
				case RenderPipeline.Legacy: return "Built-in RP";
				case RenderPipeline.URP:    return "URP";
				case RenderPipeline.HDRP:   return "HDRP";
				default:                    throw new ArgumentOutOfRangeException();
			}
		}

		public static IEnumerable<string> GetSubshaderTags( this RenderPipeline rp ) {
			yield return (ShaderTag)( "ForceNoShadowCasting", "True" );
			if( rp == RenderPipeline.Legacy )
				yield break; // this is due to a bug where SRP sometimes picks the legacy pipeline. Putting it last and without a tag fixes this 
			yield return (ShaderTag)( "RenderPipeline", rp.PipelineSubshaderTagValue() );
		}

	}

}