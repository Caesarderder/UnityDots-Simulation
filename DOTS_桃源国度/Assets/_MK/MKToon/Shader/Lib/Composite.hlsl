//////////////////////////////////////////////////////
// MK Toon Composite					       		//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_COMPOSITE
	#define MK_TOON_COMPOSITE
	
	#include "Core.hlsl"
	#include "Surface.hlsl"
	#include "Lighting.hlsl"

	/////////////////////////////////////////////////////////////////////////////////////////////
	// Composite Final Output
	/////////////////////////////////////////////////////////////////////////////////////////////
	inline void Composite(inout Surface surface, in MKSurfaceData surfaceData, in MKPBSData pbsData)
	{
		#ifdef MK_LIT
			#ifdef MK_VERTEX_LIGHTING
				surface.direct.rgb += surfaceData.vertexLighting * pbsData.diffuseRadiance;
			#endif

			#if defined(MK_SCREEN_SPACE_OCCLUSION) && defined(MK_URP_2020_2_Or_Newer)
				surface.occlusion.g *= surfaceData.ambientOcclusion.directAmbientOcclusion;
			#endif

			surface.direct.rgb *= surface.occlusion.g;

			#if defined(MK_FORWARD_BASE_PASS) && defined(MK_INDIRECT)
				surface.final.rgb = surface.indirect * lerp(dot(surface.direct.rgb, REL_LUMA), half3(1, 1, 1), surface.indirectFade) + surface.direct.rgb;
			#elif defined(MK_FORWARD_BASE_PASS) && !defined(MK_INDIRECT)
				surface.final.rgb = surface.direct.rgb;
			#elif defined(MK_FORWARD_ADD_PASS)
				surface.final.rgb = surface.direct.rgb;
			#else
				surface.final.rgb = surface.albedo;
			#endif

			#ifdef MK_IRIDESCENCE_DEFAULT
				surface.iridescence = Iridescence(_IridescenceSize, surfaceData.OneMinusVoN, _IridescenceSmoothness * 0.5, surface);
				surface.final.rgb = lerp(surface.final.rgb, surface.iridescence.rgb, surface.iridescence.a * _IridescenceColor.a);
			#endif

			#if defined(MK_RIM)
				half fi;
				#ifdef MK_VFACE
					fi = saturate(surfaceData.vFace);
				#else
					fi = 1.0h;
				#endif
			#endif
			#if defined(MK_RIM_DEFAULT)
				surface.rim = RimRawEverything(_RimSize, surfaceData.OneMinusVoN, _RimSmoothness * 0.5, surface);
				surface.final.rgb = lerp(surface.final.rgb, _RimColor.rgb, surface.rim.rgb * _RimColor.a * fi);
			#elif defined(MK_RIM_SPLIT)
				surface.rim = saturate(surface.rim);
				#if defined(MK_FORWARD_BASE_PASS)
					half4 rimDark = RimRawEverything(_RimSize, surfaceData.OneMinusVoN, _RimSmoothness * 0.5, surface);
					rimDark -= surface.rim;
					surface.final.rgb = lerp(surface.final.rgb, _RimDarkColor.rgb, rimDark.rgb * _RimDarkColor.a * fi);
				#endif

				surface.final.rgb = lerp(surface.final.rgb, _RimBrightColor.rgb, surface.rim.rgb * _RimBrightColor.a);

				//surface.final.rgb = lerp(surface.final.rgb, _RimBrightColor.rgb, surface.rimBright.rgb * _RimBrightColor.a);
				//surface.final.rgb = lerp(surface.final.rgb, _RimDarkColor.rgb, surface.rimDark.rgb * _RimDarkColor.a);
			#endif

			#ifdef MK_LIGHTING_ALPHA
				#if defined(MK_ALPHA_LOOKUP)
					surface.direct.a = saturate(surface.direct.a);
				#endif

				surface.final.a = lerp(surface.goochDark.a, surface.goochBright.a, surface.direct.a);

				#if defined(MK_ALPHA_CLIPPING)
					Clip0(surface.final.a - _AlphaCutoff);
				#endif
			#else
				surface.final.a = surface.alpha;
			#endif
		#else
			//non lit color output
			surface.final.rgb = surface.albedo;
			surface.final.a = surface.alpha;
		#endif

		#if defined(MK_FORWARD_BASE_PASS) || defined(MK_UNIVERSAL2D_PASS)
			#if defined(MK_DISSOLVE_BORDER_RAMP)
				//apply color for dissolving
				surface.final.rgb = DissolveRamp(surfaceData.dissolveClip, PASS_TEXTURE_2D(_DissolveBorderRamp, SAMPLER_REPEAT_MAIN), _DissolveBorderColor, _DissolveBorderSize, _DissolveAmount, surfaceData.baseUV.xy, surface.final.rgb);
			#elif defined(MK_DISSOLVE_BORDER_COLOR)
				surface.final.rgb = DissolveColor(surfaceData.dissolveClip, _DissolveBorderColor, _DissolveBorderSize, _DissolveAmount, surface.final.rgb);
			#endif
		#endif

		#ifdef MK_COLOR_GRADING_FINAL_OUTPUT
			surface.final.rgb = ColorGrading(surface.final.rgb, _Brightness, _Saturation, _Contrast);
		#endif
		
		#ifdef MK_FOG
			ApplyFog(surface.final.rgb, surfaceData.fogFactor);
		#endif
	}
#endif