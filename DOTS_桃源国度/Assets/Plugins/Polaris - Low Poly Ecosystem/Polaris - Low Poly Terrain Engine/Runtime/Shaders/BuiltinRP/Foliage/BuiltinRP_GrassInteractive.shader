// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polaris/BuiltinRP/Foliage/GrassInteractive"
{
	Properties
	{
		_NoiseTex("_NoiseTex", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("MainTex", 2D) = "white" {}
		_Occlusion("Occlusion", Range( 0 , 1)) = 0.2
		_AlphaCutoff("Alpha Cutoff", Range( 0 , 1)) = 0.5
		[HideInInspector]_BendFactor("BendFactor", Float) = 1
		_WaveDistance("Wave Distance", Float) = 0.1
		_Wind("Wind", Vector) = (1,1,4,8)
		_VectorField("VectorField", 2D) = "gray" {}
		_FadeMinDistance("FadeMinDistance", Float) = 50
		_FadeMaxDistance("FadeMaxDistance", Float) = 100
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

		uniform sampler2D _VectorField;
		uniform float4x4 _WorldToNormalized;
		uniform float _FadeMaxDistance;
		uniform float _FadeMinDistance;
		uniform float _BendFactor;
		uniform sampler2D _NoiseTex;
		SamplerState sampler_NoiseTex;
		uniform float4 _Wind;
		uniform float _WaveDistance;
		uniform float4 _Color;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float _Occlusion;
		uniform float _AlphaCutoff;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float4x4 myLocalVar55_g2 = _WorldToNormalized;
			float4x4 _WorldToNormalized8_g2 = myLocalVar55_g2;
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 objToView2_g18 = mul( UNITY_MATRIX_MV, float4( ase_vertex3Pos, 1 ) ).xyz;
			float _FadeMaxDistance118 = _FadeMaxDistance;
			float temp_output_1_0_g19 = _FadeMaxDistance118;
			float _FadeMinDistance117 = _FadeMinDistance;
			float clampResult7_g18 = clamp( ( ( -objToView2_g18.z - temp_output_1_0_g19 ) / ( _FadeMinDistance117 - temp_output_1_0_g19 ) ) , 0.0 , 1.0 );
			float4 _VertexPos3_g2 = float4( ( clampResult7_g18 * ase_vertex3Pos ) , 0.0 );
			float4 break68_g2 = _VertexPos3_g2;
			float4 appendResult69_g2 = (float4(break68_g2.x , break68_g2.y , break68_g2.z , 1.0));
			float4 break28_g2 = mul( _WorldToNormalized8_g2, mul( unity_ObjectToWorld, appendResult69_g2 ) );
			float4 appendResult29_g2 = (float4(break28_g2.x , break28_g2.z , 0.0 , 0.0));
			float4 vectorFieldUV30_g2 = appendResult29_g2;
			float4 bendVector33_g2 = tex2Dlod( _VectorField, float4( vectorFieldUV30_g2.xy, 0, 0.0) );
			float4 break36_g2 = bendVector33_g2;
			float4 appendResult43_g2 = (float4(( ( break36_g2.r * 2.0 ) - 1.0 ) , ( ( break36_g2.g * 2.0 ) - 1.0 ) , ( ( break36_g2.b * 2.0 ) - 1.0 ) , 0.0));
			float4 remappedBendVector44_g2 = appendResult43_g2;
			float _BendFactor71 = _BendFactor;
			float _BendFactor51_g2 = _BendFactor71;
			float4 newVertexPosition52_g2 = ( ( remappedBendVector44_g2 * v.texcoord.xy.y * _BendFactor51_g2 ) + _VertexPos3_g2 );
			float4 _VertexPos3_g32 = newVertexPosition52_g2;
			float3 objToWorld64_g32 = mul( unity_ObjectToWorld, float4( _VertexPos3_g32.xyz, 1 ) ).xyz;
			float2 appendResult22_g32 = (float2(objToWorld64_g32.x , objToWorld64_g32.z));
			float2 worldPosXZ21_g32 = appendResult22_g32;
			float _WindDirX11 = _Wind.x;
			float _WindDirX5_g32 = _WindDirX11;
			float _WindDirY12 = _Wind.y;
			float _WindDirY7_g32 = _WindDirY12;
			float2 appendResult19_g32 = (float2(_WindDirX5_g32 , _WindDirY7_g32));
			float _WindSpeed13 = _Wind.z;
			float _WindSpeed9_g32 = _WindSpeed13;
			float _WindSpread14 = _Wind.w;
			float _WindSpread10_g32 = _WindSpread14;
			float2 noisePos32_g32 = ( ( worldPosXZ21_g32 - ( appendResult19_g32 * _WindSpeed9_g32 * _Time.y ) ) / _WindSpread10_g32 );
			float temp_output_35_0_g32 = ( tex2Dlod( _NoiseTex, float4( noisePos32_g32, 0, 0.0) ).r * v.texcoord.xy.y );
			float _WaveDistance9 = _WaveDistance;
			float _WaveDistance12_g32 = _WaveDistance9;
			float _BendFactor38_g32 = _BendFactor71;
			float4 appendResult42_g32 = (float4(_WindDirX5_g32 , ( temp_output_35_0_g32 * 0.5 ) , _WindDirY7_g32 , 0.0));
			float4 transform47_g32 = mul(unity_WorldToObject,( temp_output_35_0_g32 * _WaveDistance12_g32 * _BendFactor38_g32 * appendResult42_g32 ));
			float4 _NewVertexPosition63_g32 = ( _VertexPos3_g32 + transform47_g32 );
			float4 finalVertexPosition83 = _NewVertexPosition63_g32;
			v.vertex.xyz = finalVertexPosition83.xyz;
			float3 vertexNormal55 = float3(0,1,0);
			v.normal = vertexNormal55;
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 _Color5 = _Color;
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 temp_output_24_0 = ( _Color5 * tex2D( _MainTex, uv_MainTex ) );
			float _Occlusion18 = _Occlusion;
			float lerpResult33 = lerp( 0.0 , _Occlusion18 , ( ( 1.0 - i.uv_texcoord.y ) * ( 1.0 - i.uv_texcoord.y ) ));
			float4 albedoColor40 = ( temp_output_24_0 - float4( ( 0.5 * float3(1,1,1) * lerpResult33 ) , 0.0 ) );
			o.Albedo = albedoColor40.rgb;
			float alpha45 = temp_output_24_0.a;
			float temp_output_46_0 = alpha45;
			o.Alpha = temp_output_46_0;
			clip( temp_output_46_0 - _AlphaCutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18400
1631;73;1531;1286;2861.304;-1253.962;1.168632;True;False
Node;AmplifyShaderEditor.CommentaryNode;41;-3517.229,-256.6267;Inherit;False;1899.109;1469.351;;6;40;39;37;38;44;45;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;19;-2343.704,-2056.074;Inherit;False;681.3742;1685.453;;23;15;16;5;4;18;17;7;6;12;9;13;81;11;71;14;10;80;70;8;115;116;117;118;Properties;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;37;-3415.318,290.2566;Inherit;False;1270.362;876.2831;;7;35;34;33;30;32;31;36;Occlusion;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;6;-2216.348,-1819.075;Float;True;Property;_MainTex;MainTex;3;0;Create;True;0;0;False;0;False;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.CommentaryNode;36;-3395.857,845.9573;Inherit;False;711;293;;4;26;27;28;29;Occlusion factor;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-2289.935,-799.0703;Float;False;Property;_Occlusion;Occlusion;4;0;Create;True;0;0;False;0;False;0.2;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;26;-3345.857,910.957;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;38;-3109.156,-135.5909;Inherit;False;957;392;;5;21;22;23;25;24;Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;7;-1910.346,-1818.075;Float;False;_MainTex;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;116;-2220.509,-489.8274;Inherit;False;Property;_FadeMaxDistance;FadeMaxDistance;11;0;Create;True;0;0;False;0;False;100;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;115;-2220.509,-589.8274;Inherit;False;Property;_FadeMinDistance;FadeMinDistance;10;0;Create;True;0;0;False;0;False;50;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;27;-3060.857,895.9569;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;117;-1928.509,-591.8274;Inherit;False;_FadeMinDistance;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;70;-2179.777,-697.3823;Float;False;Property;_BendFactor;BendFactor;6;1;[HideInInspector];Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;28;-3065.857,1028.956;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;118;-1922.509,-487.8274;Inherit;False;_FadeMaxDistance;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;75;-2991.674,1324.012;Inherit;False;1411.137;1925.975;;13;55;54;83;79;82;120;121;68;66;65;64;63;72;Vertex Animation, Normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;18;-1912.209,-796.5604;Float;False;_Occlusion;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;4;-2200.348,-2005.074;Float;False;Property;_Color;Color;2;0;Create;True;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;80;-2218.335,-1608.123;Float;True;Property;_VectorField;VectorField;9;0;Create;True;0;0;False;0;False;None;None;False;gray;LockedToTexture2D;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.GetLocalVarNode;22;-3011.407,8.565916;Inherit;False;7;_MainTex;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;81;-1906.155,-1606.096;Float;False;_VectorField;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;120;-2884.402,1597.212;Inherit;False;118;_FadeMaxDistance;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-2194.579,-1353.997;Float;False;Property;_WaveDistance;Wave Distance;7;0;Create;True;0;0;False;0;False;0.1;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;10;-2195.579,-1252.998;Float;False;Property;_Wind;Wind;8;0;Create;True;0;0;False;0;False;1,1,4,8;1,1,7,7;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;119;-2888.422,1492.67;Inherit;False;117;_FadeMinDistance;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;5;-1911.346,-2006.074;Float;False;_Color;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;23;-3059.156,100.409;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-2853.856,953.9564;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;35;-2931.117,686.2404;Inherit;False;18;_Occlusion;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-2847.918,578.3406;Float;False;Constant;_Float1;Float 1;6;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;71;-1919.776,-693.3823;Float;False;_BendFactor;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;14;-1910.577,-1008;Float;False;_WindSpread;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;33;-2645.116,634.2404;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;13;-1908.577,-1089;Float;False;_WindSpeed;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;9;-1907.577,-1352.997;Float;False;_WaveDistance;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;11;-1908.577,-1245.998;Float;False;_WindDirX;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;25;-2606.156,-85.59092;Inherit;False;5;_Color;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;21;-2705.05,8.684111;Inherit;True;Property;_TextureSample0;Texture Sample 0;6;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;32;-2659.255,452.257;Float;False;Constant;_Vector1;Vector 1;6;0;Create;True;0;0;False;0;False;1,1,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;82;-2874.532,1828.327;Inherit;False;71;_BendFactor;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;79;-2877.412,1735.809;Inherit;False;81;_VectorField;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;12;-1908.577,-1170.999;Float;False;_WindDirY;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-2634.255,340.2565;Float;False;Constant;_Float0;Float 0;6;0;Create;True;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;121;-2629.597,1526.365;Inherit;False;GrassFade;-1;;18;cbea1fee1a4ae92478317361ce0d3b0b;0;2;4;FLOAT;50;False;5;FLOAT;100;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;66;-2880.345,2203.931;Inherit;False;14;_WindSpread;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;72;-2872.56,2378.925;Inherit;False;71;_BendFactor;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;63;-2869.945,1949.457;Inherit;False;11;_WindDirX;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;64;-2866.345,2029.931;Inherit;False;12;_WindDirY;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;176;-2292.423,1756.451;Inherit;False;GrassTouchBending;12;;2;de9a62c6bacea6b4888ce2cdd0a3d3f8;0;3;2;FLOAT4;0,0,0,0;False;4;SAMPLER2D;;False;50;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-2321.157,-8.590945;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-2289.256,500.2567;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;68;-2889.345,2289.931;Inherit;False;9;_WaveDistance;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;65;-2876.345,2108.931;Inherit;False;13;_WindSpeed;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;44;-2110.054,-11.27011;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.FunctionNode;144;-2306.832,1982.287;Inherit;True;GrassWindAnimation;0;;32;8d39a13fc2a7a164fa1708057ff071d3;0;7;1;FLOAT4;0,0,0,0;False;51;FLOAT;1;False;52;FLOAT;1;False;53;FLOAT;7;False;54;FLOAT;7;False;55;FLOAT;0.2;False;56;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector3Node;54;-2038.875,1502.98;Float;False;Constant;_Up;Up;7;0;Create;True;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;39;-2025.438,246.9899;Inherit;False;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;40;-1847.459,246.99;Float;False;albedoColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;55;-1818.502,1502.283;Float;False;vertexNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;45;-1844.054,53.72989;Float;False;alpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;83;-1873.551,1999.116;Float;False;finalVertexPosition;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;16;-1910.955,-900.7172;Float;False;_AlphaCutoff;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-2289.935,-901.9721;Float;False;Property;_AlphaCutoff;Alpha Cutoff;5;0;Create;True;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;46;-342.2595,180.5466;Inherit;False;45;alpha;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;59;-373.3654,450.1091;Inherit;False;55;vertexNormal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;73;-371.1169,354.3228;Inherit;False;83;finalVertexPosition;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;42;-367.3995,-0.407074;Inherit;False;40;albedoColor;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;110;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Lambert;Polaris/BuiltinRP/Foliage/GrassInteractive;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;False;True;False;False;False;True;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Opaque;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Absolute;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;True;15;1;Pragma;instancing_options nolodfade nolightmap;False;;Custom;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;7;0;6;0
WireConnection;27;0;26;2
WireConnection;117;0;115;0
WireConnection;28;0;26;2
WireConnection;118;0;116;0
WireConnection;18;0;17;0
WireConnection;81;0;80;0
WireConnection;5;0;4;0
WireConnection;23;2;22;0
WireConnection;29;0;27;0
WireConnection;29;1;28;0
WireConnection;71;0;70;0
WireConnection;14;0;10;4
WireConnection;33;0;34;0
WireConnection;33;1;35;0
WireConnection;33;2;29;0
WireConnection;13;0;10;3
WireConnection;9;0;8;0
WireConnection;11;0;10;1
WireConnection;21;0;22;0
WireConnection;21;1;23;0
WireConnection;12;0;10;2
WireConnection;121;4;119;0
WireConnection;121;5;120;0
WireConnection;176;2;121;0
WireConnection;176;4;79;0
WireConnection;176;50;82;0
WireConnection;24;0;25;0
WireConnection;24;1;21;0
WireConnection;30;0;31;0
WireConnection;30;1;32;0
WireConnection;30;2;33;0
WireConnection;44;0;24;0
WireConnection;144;1;176;0
WireConnection;144;51;63;0
WireConnection;144;52;64;0
WireConnection;144;53;65;0
WireConnection;144;54;66;0
WireConnection;144;55;68;0
WireConnection;144;56;72;0
WireConnection;39;0;24;0
WireConnection;39;1;30;0
WireConnection;40;0;39;0
WireConnection;55;0;54;0
WireConnection;45;0;44;3
WireConnection;83;0;144;0
WireConnection;16;0;15;0
WireConnection;110;0;42;0
WireConnection;110;9;46;0
WireConnection;110;10;46;0
WireConnection;110;11;73;0
WireConnection;110;12;59;0
ASEEND*/
//CHKSM=AB66B735D926BDF112589D997FEB34BBA45B0A74