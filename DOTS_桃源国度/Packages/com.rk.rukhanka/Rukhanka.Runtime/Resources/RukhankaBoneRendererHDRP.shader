Shader "RukhankaBoneRenderer HDRP"
{
SubShader
{
	Tags
	{
		"RenderPipeline" = "HDRenderPipeline"
		"RenderType" = "HDUnlitShader"
		"Queue" = "Transparent+0"
	}

	Pass
	{
		Name "ForwardOnly"
		Tags
		{
			"LightMode" = "ForwardOnly"
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
