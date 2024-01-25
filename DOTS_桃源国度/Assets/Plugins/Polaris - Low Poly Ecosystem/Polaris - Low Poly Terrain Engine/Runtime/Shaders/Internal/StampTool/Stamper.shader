Shader "Hidden/Griffin/Stamper"
{
    Properties
    {
		_HeightMap ("Height Map", 2D) = "black" {}
		_Stamp ("Stamp", 2D) = "black" {}
		_Falloff ("Falloff", 2D) = "white" {}
		_Operation ("Operation", Int) = 0
		_LerpFactor ("Lerp Factor", Float) = 0.5
		_StampHeight ("Stamp Height", Float) = 1
		_StampPositionY ("Stamp Position Y", Float) = 0
		_Inverse ("Inverse", Int) = 0
		_UseFalloffAsBlendFactor ("Use Falloff As Blend Factor", Int) = 1
		_AdditionalMeshResolution ("Additional Mesh Resolution", Float) = 0
		_TerrainMask("Terrain Mask", 2D) = "black" {}
    }

	CGINCLUDE
    #include "UnityCG.cginc"
	#include "StampToolCommon.cginc"

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

	sampler2D _HeightMap;
	sampler2D _Stamp;
	sampler2D _Falloff;
	int _Operation;
	float _LerpFactor;
	float _StampHeight;
	float _StampPositionY;
	int _Inverse;
	int _UseFalloffAsBlendFactor;
	float _AdditionalMeshResolution;
	sampler2D _TerrainMask;

	fixed CalculateValue(int ops, fixed left, fixed right)
	{
		fixed result = 
			(ops==0)*max(left, left + right) +
			(ops==1)*min(left, left - right) +
			(ops==2)*min(right, right - left) +
			(ops==3)*max(left, right) +
			(ops==4)*min(left, right) +
			(ops==5)*lerp(left, right, _LerpFactor) +
			(ops==6)*abs(left-right);

		return result;
	}

	v2f vert (appdata v)
    {
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		o.localPos = v.vertex;
		return o;
    }

	fixed4 fragStampElevation (v2f i) : SV_Target
    {
		float4 heightMapColor = tex2D(_HeightMap, i.localPos);
		float4 stampColor = tex2D(_Stamp, i.uv);

		float f = saturate(2*length(i.uv - float2(0.5,0.5)));
		float falloff = tex2D(_Falloff, float2(f, 0.5)).r;
		stampColor *= falloff;
		stampColor = _Inverse*(1-stampColor) +(1-_Inverse)*stampColor;

		float currentHeight = GriffinDecodeFloatRG(heightMapColor.rg);
		float stampHeight = _StampPositionY + _StampHeight*stampColor.r;

		float terrainMask = 1 - tex2D(_TerrainMask, i.localPos).r;
		float h = CalculateValue(_Operation, currentHeight, stampHeight);
		h = saturate(h);
		h = h*(1-_UseFalloffAsBlendFactor) + lerp(currentHeight, h, falloff)*_UseFalloffAsBlendFactor;
		h = lerp(currentHeight, h, terrainMask);
		h = max(0, min(0.999999, h));

		float2 encodedHeight = GriffinEncodeFloatRG(h);

		float isHeightChanged = ceil(abs(h-currentHeight));
		float additionalResolution = isHeightChanged*_AdditionalMeshResolution + (1-isHeightChanged)*heightMapColor.b;

		return saturate(float4(encodedHeight.rg, additionalResolution, heightMapColor.a));
	}

	fixed4 fragStampVisibility (v2f i) : SV_Target
    {
		float4 heightMapColor = tex2D(_HeightMap, i.localPos);
		float4 stampColor = tex2D(_Stamp, i.uv);

		float f = saturate(2*length(i.uv - float2(0.5,0.5)));
		float falloff = tex2D(_Falloff, float2(f, 0.5)).r;
		stampColor *= falloff;
		stampColor = _Inverse*(1-stampColor) +(1-_Inverse)*stampColor;

		float currentValue = heightMapColor.a;
		float stampValue = _StampPositionY + _StampHeight*stampColor.r;

		float terrainMask = 1 - tex2D(_TerrainMask, i.localPos).r;
		float h = CalculateValue(_Operation, currentValue, stampValue);
		h = lerp(currentValue, h, terrainMask);
		h = saturate(h);

		h = h*(1-_UseFalloffAsBlendFactor) + lerp(currentValue, h, falloff)*_UseFalloffAsBlendFactor;
		
		return saturate(float4(heightMapColor.r, heightMapColor.g, heightMapColor.b, h));
	}

	ENDCG

    SubShader
    {
        Tags { "RenderType"="Transparent" }
		Cull Off

        Pass
        {
			Name "Stamp Elevation"
			Blend One Zero
			BlendOp Add
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragStampElevation
            ENDCG
        }

		Pass
        {
			Name "Stamp Visibility"
			Blend One Zero
			BlendOp Add
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragStampVisibility
            ENDCG
        }
    }
}
