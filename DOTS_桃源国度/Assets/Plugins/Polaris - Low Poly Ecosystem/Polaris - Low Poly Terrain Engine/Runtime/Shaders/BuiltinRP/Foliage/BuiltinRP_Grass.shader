// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polaris/BuiltinRP/Foliage/Grass"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0
		_Color("Color", Color) = (1,1,1,1)
		_NoiseTex("_NoiseTex", 2D) = "white" {}
		_MainTex("MainTex", 2D) = "white" {}
		_Occlusion("Occlusion", Range( 0 , 1)) = 0.2
		[HideInInspector]_BendFactor("Bend Factor", Float) = 1
		_WaveDistance("Wave Distance", Float) = 0.1
		_Wind("Wind", Vector) = (1,1,4,8)
		_FadeMinDistance("Fade Min Distance", Float) = 50
		_FadeMaxDistance("Fade Max Distance", Float) = 100
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True" "DisableBatching" = "True" }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 2.0
		#pragma multi_compile_instancing
		#pragma instancing_options nolodfade nolightmap
		#pragma surface surf Lambert keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform half _FadeMaxDistance;
		uniform half _FadeMinDistance;
		uniform sampler2D _NoiseTex;
		uniform float4 _Wind;
		uniform float _Occlusion;
		uniform float _WaveDistance;
		uniform float _BendFactor;
		uniform float4 _Color;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float _Cutoff = 0;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			half3 objToView2_g24 = mul( UNITY_MATRIX_MV, float4( ase_vertex3Pos, 1 ) ).xyz;
			half _FadeMaxDistance67_g22 = _FadeMaxDistance;
			half temp_output_1_0_g25 = _FadeMaxDistance67_g22;
			half _FadeMinDistance65_g22 = _FadeMinDistance;
			half clampResult7_g24 = clamp( ( ( -objToView2_g24.z - temp_output_1_0_g25 ) / ( _FadeMinDistance65_g22 - temp_output_1_0_g25 ) ) , 0.0 , 1.0 );
			float4 _VertexPos3_g23 = half4( ( clampResult7_g24 * ase_vertex3Pos ) , 0.0 );
			half3 objToWorld64_g23 = mul( unity_ObjectToWorld, float4( _VertexPos3_g23.xyz, 1 ) ).xyz;
			half2 appendResult22_g23 = (half2(objToWorld64_g23.x , objToWorld64_g23.z));
			float2 worldPosXZ21_g23 = appendResult22_g23;
			float _WindDirX25_g22 = _Wind.x;
			float _WindDirX5_g23 = _WindDirX25_g22;
			float _Occlusion12_g22 = _Occlusion;
			float _WindDirY7_g23 = _Occlusion12_g22;
			half2 appendResult19_g23 = (half2(_WindDirX5_g23 , _WindDirY7_g23));
			float _WindSpeed33_g22 = _Wind.z;
			float _WindSpeed9_g23 = _WindSpeed33_g22;
			float _WindSpread31_g22 = _Wind.w;
			float _WindSpread10_g23 = _WindSpread31_g22;
			float2 noisePos32_g23 = ( ( worldPosXZ21_g23 - ( appendResult19_g23 * _WindSpeed9_g23 * _Time.y ) ) / _WindSpread10_g23 );
			half temp_output_35_0_g23 = ( tex2Dlod( _NoiseTex, float4( noisePos32_g23, 0, 0.0) ).r * v.texcoord.xy.y );
			float _WaveDistance34_g22 = _WaveDistance;
			float _WaveDistance12_g23 = _WaveDistance34_g22;
			float _BendFactor27_g22 = _BendFactor;
			float _BendFactor38_g23 = _BendFactor27_g22;
			half4 appendResult42_g23 = (half4(_WindDirX5_g23 , ( temp_output_35_0_g23 * 0.5 ) , _WindDirY7_g23 , 0.0));
			half4 transform47_g23 = mul(unity_WorldToObject,( temp_output_35_0_g23 * _WaveDistance12_g23 * _BendFactor38_g23 * appendResult42_g23 ));
			half4 _NewVertexPosition63_g23 = ( _VertexPos3_g23 + transform47_g23 );
			float4 vertexPosition48_g22 = _NewVertexPosition63_g23;
			v.vertex.xyz = vertexPosition48_g22.xyz;
			float3 vertexNormal49_g22 = float3(0,1,0);
			v.normal = vertexNormal49_g22;
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 _Color22_g22 = _Color;
			float2 uv0_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			half4 temp_output_37_0_g22 = ( _Color22_g22 * tex2D( _MainTex, uv0_MainTex ) );
			float _Occlusion12_g22 = _Occlusion;
			half lerpResult29_g22 = lerp( 0.0 , _Occlusion12_g22 , ( ( 1.0 - i.uv_texcoord.y ) * ( 1.0 - i.uv_texcoord.y ) ));
			float4 albedoColor50_g22 = ( temp_output_37_0_g22 - half4( ( 0.5 * float3(1,1,1) * lerpResult29_g22 ) , 0.0 ) );
			o.Albedo = albedoColor50_g22.rgb;
			o.Alpha = 1;
			float alpha47_g22 = temp_output_37_0_g22.a;
			clip( alpha47_g22 - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18000
835;319;1906;879;1054.042;176.5205;1;True;False
Node;AmplifyShaderEditor.FunctionNode;104;-432,128;Inherit;False;GrassBaseGraph;1;;22;ad52558deb80624468aa023b05a9535b;0;0;4;COLOR;0;FLOAT;54;FLOAT4;56;FLOAT3;58
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;76;0,0;Half;False;True;-1;0;;0;0;Lambert;Polaris/BuiltinRP/Foliage/Grass;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;False;True;False;False;False;True;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0;True;True;0;True;Opaque;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;True;Absolute;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;76;0;104;0
WireConnection;76;10;104;54
WireConnection;76;11;104;56
WireConnection;76;12;104;58
ASEEND*/
//CHKSM=200553EEED5E127828E7B77C2F29D6E2DACF95F2