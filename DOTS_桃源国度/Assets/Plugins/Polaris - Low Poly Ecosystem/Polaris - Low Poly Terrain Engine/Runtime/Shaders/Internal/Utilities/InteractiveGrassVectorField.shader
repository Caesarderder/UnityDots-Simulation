Shader "Hidden/Griffin/InteractiveGrassVectorField"
{
    Properties
    {
		_Background ("Background", 2D) = "gray" {}
		_Opacity ("Opacity", Float) = 1
    }

	CGINCLUDE
    #include "UnityCG.cginc"

#include "UtilitiesCommon.cginc"

	struct appdata
    {
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
    };

    struct v2f
    {
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
		float4 localPos : TEXCOORD1;
    };

	sampler2D _Background;
	float _Opacity;

	v2f vert (appdata v)
    {
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		o.localPos = v.vertex;
		return o;
    }

	fixed4 fragAdd (v2f i) : SV_Target
    {
		float2 uv = i.uv;
		float2 center = float2(0.5, 0.5);
		float fade = saturate(1 - length(uv-center)/0.5);

		return float4(uv.x, lerp(0.5, 0, fade), uv.y, _Opacity*fade);
	}

	fixed4 fragFade (v2f i) : SV_Target
    {
		float4 bg = tex2D(_Background, i.localPos);
		float4 target = float4(0.5, 0.5, 0.5, 1);
		return MoveTowards(bg, target, _Opacity);
	}
	ENDCG

    SubShader
    {
        Tags { "RenderType"="Transparent" }
		
        Pass
        {
			Name "Add"
			Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragAdd
            ENDCG
        }

		Pass
		{
			Name "Fade"
			Blend One Zero
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragFade
            ENDCG
		}
    }
}
