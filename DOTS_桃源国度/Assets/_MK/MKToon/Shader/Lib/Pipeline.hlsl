//////////////////////////////////////////////////////
// MK Toon Pipeline					       			//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_PIPELINE
	#define MK_TOON_PIPELINE

	#include "Config.hlsl"

	#if defined(MK_URP)
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
	#elif defined(MK_LWRP)
		#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"
	#else
		#include "AutoLight.cginc"
		#include "UnityGlobalIllumination.cginc"
	#endif

	/////////////////////////////////////////////////////////////////////////////////////////////
	// Helpers
	/////////////////////////////////////////////////////////////////////////////////////////////
	//On Metal overloaded methods (with different types) cause a redefinition compiler issue
	//therefore force float only on Metal here
	#if !defined(SHADER_API_METAL) && !defined(SHADER_API_VULKAN)
		inline half SafeDivide(half v, half d)
		{
			return (v) / (d);
		}
		inline half2 SafeDivide(half2 v, half d)
		{
			return (v) / (d);
		}
		inline half2 SafeDivide(half2 v, half2 d)
		{
			return (v) / (d);
		}
		inline half3 SafeDivide(half3 v, half d)
		{
			return (v) / (d);
		}
		inline half3 SafeDivide(half3 v, half3 d)
		{
			return (v) / (d);
		}
		inline half4 SafeDivide(half4 v, half d)
		{
			return (v) / (d);
		}
		inline half4 SafeDivide(half4 v, half4 d)
		{
			return (v) / (d);
		}

		inline half MKSafeNormalize(half v)
		{
			half d = max(HALF_MIN, dot(v, v));
			return v * rsqrt(d);
			//return normalize(v);
		}
		inline half2 MKSafeNormalize(half2 v)
		{
			half d = max(HALF_MIN, dot(v, v));
			return v * rsqrt(d);
			//return normalize(v);
		}
		inline half3 MKSafeNormalize(half3 v)
		{
			half d = max(HALF_MIN, dot(v, v));
			return v * rsqrt(d);
			//return normalize(v);
		}
		inline half4 MKSafeNormalize(half4 v)
		{
			half d = max(HALF_MIN, dot(v, v));
			return v * rsqrt(d);
			//return normalize(v);
		}
	#endif
	//No infinite/zero/NaN checks are happening right now
	//always make sure divisions can't be any of the above
	inline float SafeDivide(float v, float d)
	{
		return (v) / (d);
	}
	inline float2 SafeDivide(float2 v, float d)
	{
		return (v) / (d);
	}
	inline float2 SafeDivide(float2 v, float2 d)
	{
		return (v) / (d);
	}
	inline float3 SafeDivide(float3 v, float d)
	{
		return (v) / (d);
	}
	inline float3 SafeDivide(float3 v, float3 d)
	{
		return (v) / (d);
	}
	inline float4 SafeDivide(float4 v, float d)
	{
		return (v) / (d);
	}
	inline float4 SafeDivide(float4 v, float4 d)
	{
		return (v) / (d);
	}

	//make sure vectors never have a lenth of 0
	inline float MKSafeNormalize(float v)
	{
		float d = max(HALF_MIN, dot(v, v));
		return v * rsqrt(d);
		//return normalize(v);
	}
	inline float2 MKSafeNormalize(float2 v)
	{
		float d = max(HALF_MIN, dot(v, v));
		return v * rsqrt(d);
		//return normalize(v);
	}
	inline float3 MKSafeNormalize(float3 v)
	{
		float d = max(HALF_MIN, dot(v, v));
		return v * rsqrt(d);
		//return normalize(v);
	}
	inline float4 MKSafeNormalize(float4 v)
	{
		float d = max(HALF_MIN, dot(v, v));
		return v * rsqrt(d);
		//return normalize(v);
	}

	inline half FastPow2(half v)
	{
		return v * v;
	}

	inline half FastPow3(half v)
	{
		return v * v * v;
	}

	inline half FastPow4(half v)
	{
		return v * v * v * v;
	}

	inline half FastPow5(half v)
	{
		return v * v * v * v * v;
	}

	//Single Component Reciprocal
	inline half Rcp(half v)
	{
		#if SHADER_TARGET >= 50
			return rcp(v);
		#else
			//avoid division by 0
			return SafeDivide(1.0, v);
		#endif
	}

	//modified clip function for a complete discard when input equal zero oder smaller
	inline void Clip0(half c)
	{
		//if(any(c < 0.0)) discard;
		clip(c + HALF_MIN);
	}

	/////////////////////////////////////////////////////////////////////////////////////////////
	// Sampling
	/////////////////////////////////////////////////////////////////////////////////////////////
	#if SHADER_TARGET >= 35
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
			#define UNIFORM_TEXTURE_2D_AUTO(textureName) uniform Texture2DArray<half4> textureName;
			#define UNIFORM_SAMPLER_AND_TEXTURE_2D_AUTO(textureName) uniform Texture2DArray<half4> textureName; uniform SamplerState sampler##textureName;
			#define DECLARE_TEXTURE_2D_ARGS_AUTO(textureName, samplerName) Texture2DArray<half4> textureName, SamplerState samplerName
			#define UNIFORM_SAMPLER_AND_TEXTURE_2D_AUTO_HP(textureName) uniform Texture2DArray<float4> textureName; uniform SamplerState sampler##textureName;
			#define DECLARE_TEXTURE_2D_ARGS_AUTO_HP(textureName, samplerName) Texture2DArray<float4> textureName, SamplerState samplerName
		#else
			#define UNIFORM_TEXTURE_2D_AUTO(textureName) uniform Texture2D<half4> textureName;
			#define UNIFORM_SAMPLER_AND_TEXTURE_2D_AUTO(textureName) uniform Texture2D<half4> textureName; uniform SamplerState sampler##textureName;
			#define DECLARE_TEXTURE_2D_ARGS_AUTO(textureName, samplerName) Texture2D<half4> textureName, SamplerState samplerName
			#define UNIFORM_SAMPLER_AND_TEXTURE_2D_AUTO_HP(textureName) uniform Texture2D<float4> textureName; uniform SamplerState sampler##textureName;
			#define DECLARE_TEXTURE_2D_ARGS_AUTO_HP(textureName, samplerName) Texture2D<float4> textureName, SamplerState samplerName
		#endif

		#define DECLARE_TEXTURE_2D_ARGS(textureName, samplerName) Texture2D<half4> textureName, SamplerState samplerName

		#define UNIFORM_SAMPLER(samplerName) uniform SamplerState sampler##samplerName;
		#define UNIFORM_TEXTURE_2D(textureName) uniform Texture2D<half4> textureName;

		#define PASS_TEXTURE_2D(textureName, samplerName) textureName, samplerName
	#else
		#define UNIFORM_TEXTURE_2D_AUTO(textureName) uniform sampler2D textureName;
		#define UNIFORM_SAMPLER_AND_TEXTURE_2D_AUTO(textureName) uniform sampler2D textureName;
		#define DECLARE_TEXTURE_2D_ARGS_AUTO(textureName, samplerName) sampler2D textureName
		#define UNIFORM_SAMPLER_AND_TEXTURE_2D_AUTO_HP(textureName) uniform sampler2D textureName;
		#define DECLARE_TEXTURE_2D_ARGS_AUTO_HP(textureName, samplerName) sampler2D textureName

		#define DECLARE_TEXTURE_2D_ARGS(textureName, samplerName) sampler2D textureName

		#define UNIFORM_SAMPLER(samplerName)
		#define UNIFORM_TEXTURE_2D(textureName) uniform sampler2D textureName;

		#define PASS_TEXTURE_2D(textureName, samplerName) textureName
	#endif

	#define UNIFORM_SAMPLER_2D(sampler2DName) uniform sampler2D sampler2DName;
	#define PASS_SAMPLER_2D(sampler2DName) sampler2DName
	#define DECLARE_SAMPLER_2D_ARGS(sampler2DName) sampler2D sampler2DName

	#ifdef MK_POINT_FILTERING
		UNIFORM_SAMPLER(_point_repeat_Main)
	#else
		UNIFORM_SAMPLER(_linear_repeat_Main)
	#endif
	UNIFORM_SAMPLER(_point_clamp_Main)
	#if SHADER_TARGET >= 35
		#ifdef MK_POINT_FILTERING
			#ifndef SAMPLER_REPEAT_MAIN
				#define SAMPLER_REPEAT_MAIN sampler_point_repeat_Main
			#endif
		#else
			#ifndef SAMPLER_REPEAT_MAIN
				#define SAMPLER_REPEAT_MAIN sampler_linear_repeat_Main
			#endif
		#endif
		#ifndef SAMPLER_CLAMPED_MAIN
			#define SAMPLER_CLAMPED_MAIN sampler_point_clamp_Main
		#endif
	#else
		#ifndef SAMPLER_REPEAT_MAIN
			#define SAMPLER_REPEAT_MAIN 0
		#endif
		#ifndef SAMPLER_CLAMPED_MAIN
			#define SAMPLER_CLAMPED_MAIN 0
		#endif
	#endif

	inline half4 SampleTex2DAuto(DECLARE_TEXTURE_2D_ARGS_AUTO(tex, samplerTex), float2 uv)
	{
		#if SHADER_TARGET >= 35
			#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
				return tex.SampleLevel(samplerTex, float3((uv).xy, (float)unity_StereoEyeIndex), 0);
			#else
				return tex.SampleLevel(samplerTex, UnityStereoTransformScreenSpaceTex(uv), 0);
			#endif
		#else
			return tex2D(tex, UnityStereoTransformScreenSpaceTex(uv));
		#endif
	}

	inline float4 SampleTex2DAutoHP(DECLARE_TEXTURE_2D_ARGS_AUTO_HP(tex, samplerTex), float2 uv)
	{
		#if SHADER_TARGET >= 35
			#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
				return tex.SampleLevel(samplerTex, float3((uv).xy, (float)unity_StereoEyeIndex), 0);
			#else
				return tex.SampleLevel(samplerTex, UnityStereoTransformScreenSpaceTex(uv), 0);
			#endif
		#else
			return tex2D(tex, UnityStereoTransformScreenSpaceTex(uv));
		#endif
	}

	inline half4 SampleTex2D(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 uv)
	{
		#if SHADER_TARGET >= 35
			return tex.Sample(samplerTex, uv);
		#else
			return tex2D(tex, uv);
		#endif
	}

	inline half4 SampleSampler2D(DECLARE_SAMPLER_2D_ARGS(samp2D), float2 uv)
	{
		return tex2D(samp2D, uv);
	}

	//Clamped ramp samplings
	/*
	inline half4 SampleRamp1D(DECLARE_TEXTURE_2D_ARGS(ramp, samplerTex), half value)
	{
		#if SHADER_TARGET >= 35
			return ramp.Sample(samplerTex, float2(clamp(value, HALF_MIN, ONE_MINUS_HALF_MIN), 0.5));
		#else
			return SampleTex2D(PASS_TEXTURE_2D(ramp, samplerTex), float2(clamp(value, HALF_MIN, ONE_MINUS_HALF_MIN), 0.5));
		#endif
	}

	inline half4 SampleRamp2D(DECLARE_TEXTURE_2D_ARGS(ramp, samplerTex), half2 value)
	{
		#if SHADER_TARGET >= 35
			return ramp.Sample(samplerTex, clamp(value, HALF_MIN, ONE_MINUS_HALF_MIN));
		#else
			return SampleTex2D(PASS_TEXTURE_2D(ramp, samplerTex), clamp(value, HALF_MIN, ONE_MINUS_HALF_MIN));
		#endif
	}
	*/

	//saturated ramp samplings
	inline half4 SampleRamp1D(DECLARE_TEXTURE_2D_ARGS(ramp, samplerTex), half value)
	{
		#if SHADER_TARGET >= 35
			return ramp.Sample(samplerTex, float2(saturate(value), 0.5));
		#else
			return SampleTex2D(PASS_TEXTURE_2D(ramp, samplerTex), float2(saturate(value), 0.5));
		#endif
	}

	inline half4 SampleRamp2D(DECLARE_TEXTURE_2D_ARGS(ramp, samplerTex), half2 value)
	{
		#if SHADER_TARGET >= 35
			return ramp.Sample(samplerTex, saturate(value));
		#else
			return SampleTex2D(PASS_TEXTURE_2D(ramp, samplerTex), saturate(value));
		#endif
	}

	/*
	//unclamped ramp samplings
	inline half4 SampleRamp1D(DECLARE_TEXTURE_2D_ARGS(ramp, samplerTex), half value)
	{
		#if SHADER_TARGET >= 35
			return ramp.Sample(samplerTex, float2(value, 0.5));
		#else
			return SampleTex2D(PASS_TEXTURE_2D(ramp, samplerTex), float2(value, 0.5));
		#endif
	}

	inline half4 SampleRamp2D(DECLARE_TEXTURE_2D_ARGS(ramp, samplerTex), half2 value)
	{
		#if SHADER_TARGET >= 35
			return ramp.Sample(samplerTex, value);
		#else
			return SampleTex2D(PASS_TEXTURE_2D(ramp, samplerTex), value);
		#endif
	}
	*/
	
	#ifdef MK_FLIPBOOK
		#define SAMPLE_TEX2D_FLIPBOOK(tex, samplerTex, uv, blendUV) SampleTex2DFlipbook(PASS_TEXTURE_2D(tex, samplerTex), uv, blendUV)
		#define SAMPLE_SAMPLER2D_FLIPBOOK(samp2D, uv, blendUV) SampleSampler2DFlipbook(PASS_SAMPLER_2D(samp2D), uv, blendUV)
	#else
		#define SAMPLE_TEX2D_FLIPBOOK(tex, samplerTex, uv, blendUV) SampleTex2D(PASS_TEXTURE_2D(tex, SAMPLER_REPEAT_MAIN), uv)
		#define SAMPLE_SAMPLER2D_FLIPBOOK(samp2D, uv, blendUV) SampleSampler2D(samp2D, uv)
	#endif
	
	/*
	#ifdef MK_FLIPBOOK
		#if SHADER_TARGET >= 35
			#define SAMPLE_TEX2D_FLIPBOOK(tex, samplerTex, uv, blendUV) SampleTex2DFlipbook(PASS_TEXTURE_2D(tex, samplerTex), uv, blendUV)
		#else
			#define SAMPLE_TEX2D_FLIPBOOK(tex, uv, blendUV) SampleTex2DFlipbook(PASS_TEXTURE_2D(tex, SAMPLER_REPEAT_MAIN), uv, blendUV)
		#endif
	#else
		#if SHADER_TARGET >= 35
			#define SAMPLE_TEX2D_FLIPBOOK(tex, samplerTex, uv, blendUV) SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv)
		#else
			#define SAMPLE_TEX2D_FLIPBOOK(tex, uv, blendUV) SampleTex2D(PASS_TEXTURE_2D(tex, SAMPLER_REPEAT_MAIN), uv)
		#endif
	#endif
	*/

	inline half4 SampleTex2DFlipbook(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 uv, float3 blendUV)
	{
		#ifdef MK_FLIPBOOK
			half4 color0 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv);
			half4 color1 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), blendUV.xy);
			return lerp(color0, color1, blendUV.z);
		#else
			return SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv);
		#endif
	}

	inline half4 SampleSampler2DFlipbook(DECLARE_SAMPLER_2D_ARGS(samp2D), float2 uv, float3 blendUV)
	{
		#ifdef MK_FLIPBOOK
			half4 color0 = SampleSampler2D(samp2D, uv);
			half4 color1 = SampleSampler2D(samp2D, blendUV.xy);
			return lerp(color0, color1, blendUV.z);
		#else
			return SampleSampler2D(samp2D, uv);
		#endif
	}

	struct TriplanarUV 
	{
		float2 x, y, z;
		half3 weights;
	};

	inline TriplanarUV UVTriplanar(float3 position, float scale, half blend, half3 normal)
	{
		TriplanarUV uv;
		float3 uvp = position * scale;
		uv.x = uvp.zy;
		uv.y = uvp.xz;
		uv.z = uvp.xy;
		uv.x.y += 0.5;
		uv.z.x += 0.5;

		uv.weights = pow(abs(normal), blend);
		uv.weights = SafeDivide(uv.weights, uv.weights.x + uv.weights.y + uv.weights.z);
		return uv;
	}

	inline half4 SampleTriplanar(DECLARE_TEXTURE_2D_ARGS(texX, samplerX), DECLARE_TEXTURE_2D_ARGS(texY, samplerY), DECLARE_TEXTURE_2D_ARGS(texZ, samplerZ), TriplanarUV uv)
	{
		half4 colorX = SampleTex2D(PASS_TEXTURE_2D(texX, samplerX), uv.x);
		half4 colorY = SampleTex2D(PASS_TEXTURE_2D(texY, samplerY), uv.y);
		half4 colorZ = SampleTex2D(PASS_TEXTURE_2D(texZ, samplerZ), uv.z);

		return colorX * uv.weights.x + colorY * uv.weights.y + colorZ * uv.weights.z;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////
	// Transformations
	/////////////////////////////////////////////////////////////////////////////////////////////
	#define CAMERA_POSITION_WORLD _WorldSpaceCameraPos
	#define MATRIX_V UNITY_MATRIX_V
	#define MATRIX_VP UNITY_MATRIX_VP
	#define MATRIX_MVP UNITY_MATRIX_MVP
	#if defined(MK_URP) || defined(MK_LWRP)
		#define MATRIX_M UNITY_MATRIX_M
		#define MATRIX_I_M UNITY_MATRIX_I_M
		#define VERTEX_INPUT vertexInput
		#define SV_CLIP_POS svPositionClip
	#else
		#define MATRIX_M unity_ObjectToWorld
		#define MATRIX_I_M unity_WorldToObject
		#define VERTEX_INPUT v
		#define SV_CLIP_POS pos
	#endif

	#define Z_BUFFER_PARAMS _ZBufferParams

	inline float4 ComputeObjectToClipSpace(float3 positionObject)
	{
		return mul(MATRIX_VP, mul(MATRIX_M, float4(positionObject, 1.0)));
		//return mul(MATRIX_MVP, float4(positionObject, 1.0));
	}

	inline float3 ComputeObjectToWorldSpace(float3 positionObject)
	{
		return mul(MATRIX_M, float4(positionObject, 1.0)).xyz;
	}

	inline float4 ComputeWorldToClipSpace(float3 positionWorld)
	{
		return mul(MATRIX_VP, float4(positionWorld, 1.0));
	}
	
	inline half3 ComputeNormalWorld(half3 normalDirectionObject)
	{
		#ifdef UNITY_ASSUME_UNIFORM_SCALING
			return MKSafeNormalize(mul((float3x3) MATRIX_M, normalDirectionObject));
		#else
			// Normal need to be multiply by inverse transpose
			return MKSafeNormalize(mul(normalDirectionObject, (float3x3) MATRIX_I_M));
		#endif
	}

	inline half3 ComputeNormalObjectToClipSpace(half3 normalDirectionObject)
	{
		#ifdef UNITY_ASSUME_UNIFORM_SCALING
			return MKSafeNormalize(mul((float3x3) UNITY_MATRIX_VP, mul((float3x3) MATRIX_M, normalDirectionObject)));
		#else
			// Normal need to be multiply by inverse transpose
			return MKSafeNormalize(mul((float3x3) UNITY_MATRIX_VP, mul(normalDirectionObject, (float3x3) MATRIX_I_M)));
		#endif
	}

	inline half3 ComputeTangentWorld(half3 tangentObject)
	{
		return MKSafeNormalize(mul((float3x3) MATRIX_M, tangentObject));
	}

	inline half3 ComputeBitangentWorld(half3 normalWorld, half3 tangentWorld, half scale)
	{
		return MKSafeNormalize(cross(normalWorld, tangentWorld)) * scale;
	}

	inline half3 ComputeViewWorld(float3 positionWorld)
	{
		return MKSafeNormalize(CAMERA_POSITION_WORLD - positionWorld);
	}

	inline half3 ComputeViewObject(float3 positionObject)
	{
    	return MKSafeNormalize(mul(MATRIX_I_M, float4(CAMERA_POSITION_WORLD, 1)).xyz - positionObject);
	}

	inline half3 ComputeViewTangent(half3 view, half3 normal, half3 tangent, half3 bitangent)
	{
		return MKSafeNormalize(mul(half3x3(tangent, bitangent, normal), view));
	}

	inline float ComputeLinearDepth(float depth)
	{
		return Rcp(Z_BUFFER_PARAMS.z * depth + Z_BUFFER_PARAMS.w);
	}

	inline float4 ComputeNDC(float4 positionClip) 
	{
		float4 ndc;

		#if UNITY_UV_STARTS_AT_TOP
			float scale = -1.0;
		#else
			float scale = 1.0;
		#endif

		ndc = positionClip * 0.5;
		ndc.xy = float2(ndc.x, ndc.y * scale) + ndc.w;
    	ndc.zw = positionClip.zw;

		ndc.xyz = SafeDivide(ndc.xyz, ndc.w);

		/*
		#if defined(UNITY_SINGLE_PASS_STEREO)
			ndc.xy = TransformStereoScreenSpaceTex(ndc.xy, ndc.w);
		#endif
		*/

		return ndc;
	}

	inline float2 ComputeNormalizedScreenUV(float4 ndc, float4 nullNdc, float scale)
	{
		#ifndef MK_REGULAR_SCREEN_SPACE
			//Orthographic camera is hard to handle => no ability to get "size"
			//therefore ortho view differs from perspective view
			
			//NDC offset
			ndc.xy -= nullNdc.xy;

			//Scale based on rendertarget size
			#if defined(UNITY_SINGLE_PASS_STEREO)
				half aspect = SafeDivide(_ScreenParams.x, _ScreenParams.y);
			#else
				half aspect = SafeDivide(_ScreenParams.x, _ScreenParams.y);
			#endif
			ndc.x *= aspect;
			ndc.xy *= scale;
			ndc.xy *= nullNdc.w;
		#else
			#if defined(UNITY_SINGLE_PASS_STEREO)
				half aspect = SafeDivide(_ScreenParams.x, _ScreenParams.y);
			#else
				half aspect = SafeDivide(_ScreenParams.x, _ScreenParams.y);
			#endif
			ndc.x *= aspect;
			ndc.xy *= scale;
			ndc.xy *= nullNdc.w;
		#endif

		return ndc.xy;
	};

	//based on URP - "Octahedron Environment Maps" paper
	half2 PackDepthNormals(half3 n)
	{
		#if defined(MK_URP) || defined(MK_LWRP)
			half3 p = n * rcp(dot(abs(n), 1.0));
			half  x = p.x, y = p.y, z = p.z;

			half r = saturate(0.5 - 0.5 * x + 0.5 * y);
			half g = x + y;

			return half2(CopySign(r, z), g);
		#else
			//not needed on builtin just return input as a workaround
			return n;
		#endif
	}

	/////////////////////////////////////////////////////////////////////////////////////////////
	// Fog
	/////////////////////////////////////////////////////////////////////////////////////////////
	inline float FogFactorVertex(float zClipSpace)
	{
		#if defined(MK_URP) || defined(MK_LWRP)
			return ComputeFogFactor(zClipSpace);
		#else
			#if (SHADER_TARGET < 30) || defined(SHADER_API_MOBILE)
				UNITY_CALC_FOG_FACTOR(zClipSpace);
				return unityFogFactor;
			#else
				return zClipSpace;
			#endif
		#endif
	}

	inline void ApplyFog(inout half3 color, float fogFactor)
	{
		#if defined(MK_URP) || defined(MK_LWRP)
			color = MixFog(color, fogFactor);
		#else
			#if defined(MK_FORWARD_BASE_PASS) || defined(MK_OUTLINE_PASS)
				UNITY_APPLY_FOG_COLOR(fogFactor, color, unity_FogColor);
			#elif defined(MK_FORWARD_ADD_PASS)
				UNITY_APPLY_FOG_COLOR(fogFactor, color, half4(0,0,0,0));
			#endif
		#endif
	}

	/////////////////////////////////////////////////////////////////////////////////////////////
	// Refraction
	/////////////////////////////////////////////////////////////////////////////////////////////
	#if defined(MK_URP) || defined(MK_LWRP)
		UNIFORM_TEXTURE_2D_AUTO(_CameraOpaqueTexture)
	#else
		UNIFORM_TEXTURE_2D_AUTO(_MKToonRefraction)
	#endif

	inline half3 SampleRefraction(float2 uv)
	{
		#if defined(MK_URP) || defined(MK_LWRP)
			return SampleTex2DAuto(PASS_TEXTURE_2D(_CameraOpaqueTexture, SAMPLER_CLAMPED_MAIN), uv).rgb;
		#else
			return SampleTex2DAuto(PASS_TEXTURE_2D(_MKToonRefraction, SAMPLER_CLAMPED_MAIN), uv).rgb;
		#endif
	}

	/////////////////////////////////////////////////////////////////////////////////////////////
	// Depth
	/////////////////////////////////////////////////////////////////////////////////////////////
	UNIFORM_SAMPLER_AND_TEXTURE_2D_AUTO_HP(_CameraDepthTexture)

	inline float SampleDepth(float2 uv)
	{
		return SampleTex2DAutoHP(PASS_TEXTURE_2D(_CameraDepthTexture, SAMPLER_CLAMPED_MAIN), uv).r;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////
	// Other
	/////////////////////////////////////////////////////////////////////////////////////////////
	#if defined(MK_URP) && UNITY_VERSION >= 202220
		#define MK_TIME _TimeParameters
	#else
		#define MK_TIME _Time
	#endif

#endif