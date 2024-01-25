Shader "Hidden/Griffin/BlendMapGenerator"
{
    Properties
    {
        _Background ("Background", 2D) = "black" {}
		_Foreground ("Foreground", 2D) = "black" {}
		_Number ("Number", Float) = 1
		_Vector ("Vector", Vector) = (1,1,1,1)
		_Ops ("Ops", Int) = 0
		_LerpFactor ("Lerp Factor", Float) = 0.5
		_LerpMask ("Lerp Mask", 2D) = "white" {}
		_Saturate ("Saturate", Int) = 1
    }

	CGINCLUDE
    #include "UnityCG.cginc"
	struct appdata
    {
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
    };

    struct v2f
    {
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
    };

    sampler2D _Background;
	sampler2D _Foreground;
	float _Number;
	float4 _Vector;
	int _Ops;
	float _LerpFactor;
	sampler2D _LerpMask;
	int _Saturate;

	v2f vert (appdata v)
    {
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
    }

	float CalculateValue(float left, float right, int ops, float lerpFactor)
	{
		fixed result = 
			(ops==0)*(left + right) +
			(ops==1)*(left - right) +
			(ops==2)*(left * right) +
			(ops==3)*(left / (right+0.0000001)) +
			(ops==4)*min(left, right) +
			(ops==5)*max(left, right) +
			(ops==6)*abs(left - right) +
			(ops==7)*lerp(left, right, lerpFactor);

		return result;
	}

	float4 fragBlendTexture (v2f i) : SV_Target
    {
		float4 bg = tex2D(_Background, i.uv);
		float4 fg = tex2D(_Foreground, i.uv);
		float lerpMask = tex2D(_LerpMask, i.uv).r;
		float4 col = float4(
			CalculateValue(bg.r, fg.r, _Ops, _LerpFactor*lerpMask),
			CalculateValue(bg.g, fg.g, _Ops, _LerpFactor*lerpMask),
			CalculateValue(bg.b, fg.b, _Ops, _LerpFactor*lerpMask),
			CalculateValue(bg.a, fg.a, _Ops, _LerpFactor*lerpMask));

		return _Saturate*saturate(col) + (1-_Saturate)*col;
	}

	float4 fragBlendNumber (v2f i) : SV_Target
    {
		float4 bg = tex2D(_Background, i.uv);
		float lerpMask = tex2D(_LerpMask, i.uv).r;
		float4 col = float4(
			CalculateValue(bg.r, _Number, _Ops, _LerpFactor*lerpMask),
			CalculateValue(bg.g, _Number, _Ops, _LerpFactor*lerpMask),
			CalculateValue(bg.b, _Number, _Ops, _LerpFactor*lerpMask),
			CalculateValue(bg.a, _Number, _Ops, _LerpFactor*lerpMask));

		return _Saturate*saturate(col) + (1-_Saturate)*col;
	}

	float4 fragBlendVector (v2f i) : SV_Target
    {
		float4 bg = tex2D(_Background, i.uv);
		float lerpMask = tex2D(_LerpMask, i.uv).r;
		float4 col = float4(
			CalculateValue(bg.r, _Vector.r, _Ops, _LerpFactor*lerpMask),
			CalculateValue(bg.g, _Vector.g, _Ops, _LerpFactor*lerpMask),
			CalculateValue(bg.b, _Vector.b, _Ops, _LerpFactor*lerpMask),
			CalculateValue(bg.a, _Vector.a, _Ops, _LerpFactor*lerpMask));

		return _Saturate*saturate(col) + (1-_Saturate)*col;
	}

	ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" }
		
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragBlendTexture
            ENDCG
        }

		Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragBlendNumber
            ENDCG
        }

		Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragBlendVector
            ENDCG
        }
    }
}
