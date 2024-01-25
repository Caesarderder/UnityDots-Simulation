// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polaris/BuiltinRP/Foliage/GrassBillboard"
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
		#pragma target 3.0
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
			half3 objToView2_g20 = mul( UNITY_MATRIX_MV, float4( ase_vertex3Pos, 1 ) ).xyz;
			half _FadeMaxDistance67_g18 = _FadeMaxDistance;
			half temp_output_1_0_g21 = _FadeMaxDistance67_g18;
			half _FadeMinDistance65_g18 = _FadeMinDistance;
			half clampResult7_g20 = clamp( ( ( -objToView2_g20.z - temp_output_1_0_g21 ) / ( _FadeMinDistance65_g18 - temp_output_1_0_g21 ) ) , 0.0 , 1.0 );
			float4 _VertexPos3_g19 = half4( ( clampResult7_g20 * ase_vertex3Pos ) , 0.0 );
			half3 objToWorld64_g19 = mul( unity_ObjectToWorld, float4( _VertexPos3_g19.xyz, 1 ) ).xyz;
			half2 appendResult22_g19 = (half2(objToWorld64_g19.x , objToWorld64_g19.z));
			float2 worldPosXZ21_g19 = appendResult22_g19;
			float _WindDirX25_g18 = _Wind.x;
			float _WindDirX5_g19 = _WindDirX25_g18;
			float _Occlusion12_g18 = _Occlusion;
			float _WindDirY7_g19 = _Occlusion12_g18;
			half2 appendResult19_g19 = (half2(_WindDirX5_g19 , _WindDirY7_g19));
			float _WindSpeed33_g18 = _Wind.z;
			float _WindSpeed9_g19 = _WindSpeed33_g18;
			float _WindSpread31_g18 = _Wind.w;
			float _WindSpread10_g19 = _WindSpread31_g18;
			float2 noisePos32_g19 = ( ( worldPosXZ21_g19 - ( appendResult19_g19 * _WindSpeed9_g19 * _Time.y ) ) / _WindSpread10_g19 );
			half temp_output_35_0_g19 = ( tex2Dlod( _NoiseTex, float4( noisePos32_g19, 0, 0.0) ).r * v.texcoord.xy.y );
			float _WaveDistance34_g18 = _WaveDistance;
			float _WaveDistance12_g19 = _WaveDistance34_g18;
			float _BendFactor27_g18 = _BendFactor;
			float _BendFactor38_g19 = _BendFactor27_g18;
			half4 appendResult42_g19 = (half4(_WindDirX5_g19 , ( temp_output_35_0_g19 * 0.5 ) , _WindDirY7_g19 , 0.0));
			half4 transform47_g19 = mul(unity_WorldToObject,( temp_output_35_0_g19 * _WaveDistance12_g19 * _BendFactor38_g19 * appendResult42_g19 ));
			half4 _NewVertexPosition63_g19 = ( _VertexPos3_g19 + transform47_g19 );
			float4 vertexPosition48_g18 = _NewVertexPosition63_g19;
			v.vertex.xyz = vertexPosition48_g18.xyz;
			float3 vertexNormal49_g18 = float3(0,1,0);
			v.normal = vertexNormal49_g18;
			//Calculate new billboard vertex position and normal;
			float3 upCamVec = float3( 0, 1, 0 );
			float3 forwardCamVec = -normalize ( UNITY_MATRIX_V._m20_m21_m22 );
			float3 rightCamVec = normalize( UNITY_MATRIX_V._m00_m01_m02 );
			float4x4 rotationCamMatrix = float4x4( rightCamVec, 0, upCamVec, 0, forwardCamVec, 0, 0, 0, 0, 1 );
			v.normal = normalize( mul( float4( v.normal , 0 ), rotationCamMatrix )).xyz;
			v.vertex.x *= length( unity_ObjectToWorld._m00_m10_m20 );
			v.vertex.y *= length( unity_ObjectToWorld._m01_m11_m21 );
			v.vertex.z *= length( unity_ObjectToWorld._m02_m12_m22 );
			v.vertex = mul( v.vertex, rotationCamMatrix );
			v.vertex.xyz += unity_ObjectToWorld._m03_m13_m23;
			//Need to nullify rotation inserted by generated surface shader;
			v.vertex = mul( unity_WorldToObject, v.vertex );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 _Color22_g18 = _Color;
			float2 uv0_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			half4 temp_output_37_0_g18 = ( _Color22_g18 * tex2D( _MainTex, uv0_MainTex ) );
			float _Occlusion12_g18 = _Occlusion;
			half lerpResult29_g18 = lerp( 0.0 , _Occlusion12_g18 , ( ( 1.0 - i.uv_texcoord.y ) * ( 1.0 - i.uv_texcoord.y ) ));
			float4 albedoColor50_g18 = ( temp_output_37_0_g18 - half4( ( 0.5 * float3(1,1,1) * lerpResult29_g18 ) , 0.0 ) );
			o.Albedo = albedoColor50_g18.rgb;
			o.Alpha = 1;
			float alpha47_g18 = temp_output_37_0_g18.a;
			clip( alpha47_g18 - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18000
1534;339;1906;879;1136.054;189.9912;1;True;False
Node;AmplifyShaderEditor.FunctionNode;93;-353.3602,109.1532;Inherit;False;GrassBaseGraph;1;;18;ad52558deb80624468aa023b05a9535b;0;0;4;COLOR;0;FLOAT;54;FLOAT4;56;FLOAT3;58
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;76;0,0;Half;False;True;-1;2;;0;0;Lambert;Polaris/BuiltinRP/Foliage/GrassBillboard;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;False;True;False;False;False;True;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0;True;True;0;True;Opaque;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;True;Cylindrical;True;Absolute;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;76;0;93;0
WireConnection;76;10;93;54
WireConnection;76;11;93;56
WireConnection;76;12;93;58
ASEEND*/
//CHKSM=39D12AD88416EDC0F0B9B06CEB85EC8A44AF8D2B