Shader "Hidden/Griffin/BlurRadius"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

	CGINCLUDE
	#define BLUR(radius)	float2 texel = _MainTex_TexelSize.xy;\
							float4 avgColor = float4(0,0,0,0);\
							float sampleCount = 0;\
							for (int x0=-radius; x0<=radius; ++x0)\
							{\
								for (int y0=-radius; y0<=radius; ++y0)\
								{\
									avgColor += tex2D(_MainTex, i.uv + float2(x0*texel.x, y0*texel.y));\
									sampleCount +=1;\
								}\
							}\
							avgColor = avgColor/sampleCount;\
							return avgColor;\

	#define FRAG(radius)	float4 frag##radius (v2f i) : SV_Target\
							{\
								BLUR(radius);\
							}\

	#include "UnityCG.cginc"
	#pragma vertex vert

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

    sampler2D _MainTex;
    float4 _MainTex_ST;
	float4 _MainTex_TexelSize;

    v2f vert (appdata v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        return o;
    }

	fixed4 frag0 (v2f i) : SV_Target
	{
		return tex2D(_MainTex, i.uv);
	}

    FRAG(1)
	FRAG(2)
	FRAG(3)
	FRAG(4)
	FRAG(5)
	FRAG(6)
	FRAG(7)
	FRAG(8)
	FRAG(9)
	FRAG(10)
	FRAG(11)
	FRAG(12)
	FRAG(13)
	FRAG(14)
	FRAG(15)
	FRAG(16)
	FRAG(17)
	FRAG(18)
	FRAG(19)
	FRAG(20)
	/*
	FRAG(21)
	FRAG(22)
	FRAG(23)
	FRAG(24)
	FRAG(25)
	FRAG(26)
	FRAG(27)
	FRAG(28)
	FRAG(29)
	FRAG(30)
	FRAG(31)
	FRAG(32)
	FRAG(33)
	FRAG(34)
	FRAG(35)
	FRAG(36)
	FRAG(37)
	FRAG(38)
	FRAG(39)
	FRAG(40)
	FRAG(41)
	FRAG(42)
	FRAG(43)
	FRAG(44)
	FRAG(45)
	FRAG(46)
	FRAG(47)
	FRAG(48)
	FRAG(49)
	FRAG(50)
	FRAG(51)
	FRAG(52)
	FRAG(53)
	FRAG(54)
	FRAG(55)
	FRAG(56)
	FRAG(57)
	FRAG(58)
	FRAG(59)
	FRAG(60)
	FRAG(61)
	FRAG(62)
	FRAG(63)
	FRAG(64)
	FRAG(65)
	FRAG(66)
	FRAG(67)
	FRAG(68)
	FRAG(69)
	FRAG(70)
	FRAG(71)
	FRAG(72)
	FRAG(73)
	FRAG(74)
	FRAG(75)
	FRAG(76)
	FRAG(77)
	FRAG(78)
	FRAG(79)
	FRAG(80)
	FRAG(81)
	FRAG(82)
	FRAG(83)
	FRAG(84)
	FRAG(85)
	FRAG(86)
	FRAG(87)
	FRAG(88)
	FRAG(89)
	FRAG(90)
	FRAG(91)
	FRAG(92)
	FRAG(93)
	FRAG(94)
	FRAG(95)
	FRAG(96)
	FRAG(97)
	FRAG(98)
	FRAG(99)
	FRAG(100)
	*/

	ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass {CGPROGRAM #pragma fragment frag0 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag1 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag2 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag3 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag4 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag5 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag6 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag7 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag8 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag9 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag10 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag11 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag12 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag13 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag14 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag15 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag16 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag17 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag18 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag19 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag20 ENDCG}
		/*
		Pass {CGPROGRAM #pragma fragment frag21 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag22 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag23 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag24 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag25 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag26 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag27 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag28 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag29 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag30 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag31 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag32 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag33 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag34 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag35 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag36 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag37 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag38 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag39 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag40 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag41 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag42 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag43 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag44 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag45 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag46 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag47 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag48 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag49 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag50 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag51 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag52 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag53 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag54 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag55 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag56 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag57 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag58 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag59 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag60 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag61 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag62 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag63 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag64 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag65 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag66 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag67 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag68 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag69 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag70 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag71 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag72 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag73 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag74 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag75 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag76 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag77 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag78 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag79 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag80 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag81 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag82 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag83 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag84 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag85 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag86 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag87 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag88 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag89 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag90 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag91 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag92 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag93 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag94 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag95 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag96 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag97 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag98 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag99 ENDCG}
		Pass {CGPROGRAM #pragma fragment frag100 ENDCG}
		*/

    }
}
