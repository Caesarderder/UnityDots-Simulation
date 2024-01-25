Shader "RukhankaBoneRenderer URP"
{
SubShader
{
	Tags
	{
		"RenderPipeline"="UniversalPipeline"
		"RenderType"="Opaque"
	}
	
	Pass
	{
		Tags
		{
			"LightMode" = "UniversalForward"
		}

        Blend SrcAlpha OneMinusSrcAlpha
		ZTest off

		HLSLPROGRAM
		#pragma target 3.0

		#pragma vertex VS
		#pragma fragment PS

		#include "RukhankaBoneRenderer.hlsl"

		ENDHLSL
	}
}
}
