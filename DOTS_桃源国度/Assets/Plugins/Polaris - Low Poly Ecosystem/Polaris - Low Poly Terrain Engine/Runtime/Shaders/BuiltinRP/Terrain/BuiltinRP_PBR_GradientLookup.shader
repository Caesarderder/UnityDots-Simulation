// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polaris/BuiltinRP/Terrain/PBR_GradientLookup"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_ColorByHeight("Color By Height", 2D) = "white" {}
		_ColorByNormal("Color By Normal", 2D) = "white" {}
		_ColorBlend("ColorBlend", 2D) = "white" {}
		_Dimension("Dimension", Vector) = (1,1,1,0)
		_MainTex("MainTex", 2D) = "black" {}
		_MetallicGlossMap("MetallicGlossMap", 2D) = "black" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "DisableBatching" = "True" }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float3 worldPos;
			half3 worldNormal;
			float2 uv_texcoord;
		};

		uniform sampler2D _ColorByHeight;
		uniform float3 _Dimension;
		uniform sampler2D _ColorBlend;
		uniform sampler2D _ColorByNormal;
		uniform sampler2D _MainTex;
		uniform float4 _Color;
		uniform sampler2D _MetallicGlossMap;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float _Height22 = _Dimension.y;
			float heightFraction59 = (0.0 + (ase_vertex3Pos.y - 0.0) * (1.0 - 0.0) / (_Height22 - 0.0));
			half2 appendResult25 = (half2(heightFraction59 , 0.5));
			float4 colorByHeightResult30 = tex2D( _ColorByHeight, appendResult25 );
			half2 appendResult63 = (half2(heightFraction59 , 0.5));
			half4 tex2DNode65 = tex2D( _ColorBlend, appendResult63 );
			half3 ase_worldNormal = i.worldNormal;
			half3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			half2 appendResult41 = (half2(saturate( ase_vertexNormal.y ) , 0.5));
			float4 colorByNormalResult43 = tex2D( _ColorByNormal, appendResult41 );
			float4 mainTexColor53 = tex2D( _MainTex, i.uv_texcoord );
			float4 _Color9 = _Color;
			float4 albedoResult78 = ( ( ( ( ( colorByHeightResult30 * ( 1.0 - tex2DNode65.r ) ) + ( colorByNormalResult43 * tex2DNode65.r ) ) * ( 1.0 - mainTexColor53.a ) ) + mainTexColor53 ) * _Color9 );
			o.Albedo = albedoResult78.rgb;
			half4 tex2DNode51 = tex2D( _MetallicGlossMap, i.uv_texcoord );
			float metallicValue54 = tex2DNode51.r;
			o.Metallic = metallicValue54;
			float smoothnessValue55 = tex2DNode51.a;
			o.Smoothness = smoothnessValue55;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows noinstancing 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18000
1244;197;1906;885;4343.742;1521.234;1.3;True;False
Node;AmplifyShaderEditor.CommentaryNode;19;-3470.578,-1465.357;Inherit;False;705.3615;1527.715;;15;15;13;14;16;18;12;10;22;17;35;33;34;32;9;7;Properties;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector3Node;17;-3364.849,-146.0849;Float;False;Property;_Dimension;Dimension;4;0;Create;True;0;0;False;0;1,1,1;500,100,500;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;29;-4462.73,222.3531;Inherit;False;1705.197;471.512;;9;21;20;23;59;27;26;28;25;30;Sample Color By Height;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;22;-3004.962,-53.73154;Float;False;_Height;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;20;-4431.73,366.4063;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;21;-4420.388,526.8657;Inherit;False;22;_Height;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;46;-4251.19,848.2155;Inherit;False;1479.913;393.3013;;7;44;38;45;40;41;42;43;Sample Color By Normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.TFHCRemapNode;23;-4154.533,369.3532;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;10;-3406.112,-779.2103;Float;True;Property;_ColorByHeight;Color By Height;1;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.NormalVertexDataNode;44;-4201.19,985.4062;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;13;-3408.715,-574.6287;Float;True;Property;_ColorByNormal;Color By Normal;2;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;32;-3414.7,-1220.463;Float;True;Property;_MainTex;MainTex;5;0;Create;True;0;0;False;0;None;None;False;black;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;38;-3893.935,1126.517;Float;False;Constant;_Float1;Float 1;5;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;45;-3887.889,985.4063;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;15;-3409.715,-355.6289;Float;True;Property;_ColorBlend;ColorBlend;3;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-3838.534,551.3537;Float;False;Constant;_Float0;Float 0;5;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;33;-3015.051,-1211.627;Float;False;_MainTex;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;59;-3919.558,376.0131;Float;False;heightFraction;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;12;-3003.319,-778.0468;Float;False;_ColorByHeight;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.CommentaryNode;56;-4035.565,1390.725;Inherit;False;1267.739;563.0514;;8;49;47;50;53;51;52;54;55;Sample Color Map and Metallic Map;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;81;-5200.423,2113.417;Inherit;False;2426.761;752.334;;19;62;64;66;63;65;61;73;60;70;69;71;77;72;76;75;74;80;79;78;Blending;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;14;-3005.922,-573.4653;Float;False;_ColorByNormal;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;64;-5100.239,2458.251;Float;False;Constant;_Float2;Float 2;7;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;47;-3977.185,1555.253;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;25;-3615.534,379.3532;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;50;-3933.888,1440.725;Inherit;False;33;_MainTex;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;40;-3757.434,898.2155;Inherit;False;14;_ColorByNormal;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;16;-3006.922,-354.4655;Float;False;_ColorBlend;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;28;-3708.534,272.3531;Inherit;False;12;_ColorByHeight;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.DynamicAppendNode;41;-3664.434,1005.216;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;62;-5150.423,2367.167;Inherit;False;59;heightFraction;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;42;-3421.434,933.2155;Inherit;True;Property;_TextureSample1;Texture Sample 1;5;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;66;-5135.239,2285.251;Inherit;False;16;_ColorBlend;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SamplerNode;27;-3372.534,307.3531;Inherit;True;Property;_TextureSample0;Texture Sample 0;5;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;49;-3538.628,1451.898;Inherit;True;Property;_TextureSample2;Texture Sample 2;7;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;63;-4921.24,2371.251;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;43;-3060.277,935.2553;Float;False;colorByNormalResult;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;65;-4646.239,2282.251;Inherit;True;Property;_TextureSample4;Texture Sample 4;7;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;53;-3047.001,1457.485;Float;False;mainTexColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;30;-3011.377,309.393;Float;False;colorByHeightResult;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;60;-4608.684,2163.417;Inherit;False;30;colorByHeightResult;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;70;-4300.243,2314.251;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;73;-4565.905,2750.751;Inherit;False;53;mainTexColor;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;61;-4612.435,2516.534;Inherit;False;43;colorByNormalResult;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;77;-4217.19,2604.035;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-4105.244,2469.251;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;-4102.244,2241.251;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;72;-3897.244,2331.251;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TexturePropertyNode;34;-3397.986,-997.0757;Float;True;Property;_MetallicGlossMap;MetallicGlossMap;6;0;Create;True;0;0;False;0;None;None;False;black;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.OneMinusNode;76;-3902.191,2484.035;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;7;-3399.708,-1413.641;Float;False;Property;_Color;Color;0;0;Create;True;0;0;False;0;1,1,1,1;0.8705882,0.8705882,0.8705882,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;-3675.381,2329.59;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;35;-3031.346,-994.3598;Float;False;_MetallicGlossMap;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;9;-3028.91,-1415.357;Float;False;_Color;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;80;-3486.65,2513.787;Inherit;False;9;_Color;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;52;-3985.566,1732.629;Inherit;False;35;_MetallicGlossMap;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SimpleAddOpNode;74;-3443.536,2325.927;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;-3223.65,2328.787;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;51;-3535.835,1706.093;Inherit;True;Property;_TextureSample3;Texture Sample 3;7;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;78;-3016.667,2319.323;Float;False;albedoResult;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;54;-3033.034,1713.076;Float;False;metallicValue;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;55;-3051.191,1841.57;Float;False;smoothnessValue;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;18;-3003.449,-144.7849;Float;False;_Dimension;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;57;-2427.393,217.5707;Inherit;False;54;metallicValue;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;31;-2429.736,122.1925;Inherit;False;78;albedoResult;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;58;-2456.393,317.5708;Inherit;False;55;smoothnessValue;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;82;-2051.866,136.1994;Half;False;True;-1;2;;0;0;Standard;Polaris/BuiltinRP/Terrain/PBR_GradientLookup;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;True;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;22;0;17;2
WireConnection;23;0;20;2
WireConnection;23;2;21;0
WireConnection;45;0;44;2
WireConnection;33;0;32;0
WireConnection;59;0;23;0
WireConnection;12;0;10;0
WireConnection;14;0;13;0
WireConnection;25;0;59;0
WireConnection;25;1;26;0
WireConnection;16;0;15;0
WireConnection;41;0;45;0
WireConnection;41;1;38;0
WireConnection;42;0;40;0
WireConnection;42;1;41;0
WireConnection;27;0;28;0
WireConnection;27;1;25;0
WireConnection;49;0;50;0
WireConnection;49;1;47;0
WireConnection;63;0;62;0
WireConnection;63;1;64;0
WireConnection;43;0;42;0
WireConnection;65;0;66;0
WireConnection;65;1;63;0
WireConnection;53;0;49;0
WireConnection;30;0;27;0
WireConnection;70;0;65;1
WireConnection;77;0;73;0
WireConnection;71;0;61;0
WireConnection;71;1;65;1
WireConnection;69;0;60;0
WireConnection;69;1;70;0
WireConnection;72;0;69;0
WireConnection;72;1;71;0
WireConnection;76;0;77;3
WireConnection;75;0;72;0
WireConnection;75;1;76;0
WireConnection;35;0;34;0
WireConnection;9;0;7;0
WireConnection;74;0;75;0
WireConnection;74;1;73;0
WireConnection;79;0;74;0
WireConnection;79;1;80;0
WireConnection;51;0;52;0
WireConnection;51;1;47;0
WireConnection;78;0;79;0
WireConnection;54;0;51;1
WireConnection;55;0;51;4
WireConnection;18;0;17;0
WireConnection;82;0;31;0
WireConnection;82;3;57;0
WireConnection;82;4;58;0
ASEEND*/
//CHKSM=9779332916E9A851E71EA1C01C6579878B5AE694