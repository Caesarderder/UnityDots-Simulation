// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

using System;

namespace Shapes {

	internal enum ShaderPassType {
		Render,
		Picking,
		Selection,
		DepthOnly
	}

	internal static class ShaderPassTypeExtensions {

		public static (string name, string lightMode) NameAndLightMode( this ShaderPassType pass, RenderPipeline rp ) {
			switch( pass ) {
				case ShaderPassType.Render:
					switch( rp ) {
						case RenderPipeline.URP:  return ( "Pass", "SRPDefaultUnlit" );
						case RenderPipeline.HDRP: return ( "ForwardOnly", "ForwardOnly" );
						default:                  throw new ArgumentOutOfRangeException( nameof(rp), rp, null );
					}
				case ShaderPassType.Picking:   return ( "Picking", "Picking" );
				case ShaderPassType.Selection: return ( "Selection", "SceneSelectionPass" );
				case ShaderPassType.DepthOnly:
					switch( rp ) {
						case RenderPipeline.URP:  return ( "DepthOnly", "DepthOnly" );
						case RenderPipeline.HDRP: return ( "DepthForwardOnly", "DepthForwardOnly" );
						default:                  throw new ArgumentOutOfRangeException( nameof(rp), rp, null );
					}
				default: throw new ArgumentOutOfRangeException( nameof(pass), pass, null );
			}
		}


	}

}