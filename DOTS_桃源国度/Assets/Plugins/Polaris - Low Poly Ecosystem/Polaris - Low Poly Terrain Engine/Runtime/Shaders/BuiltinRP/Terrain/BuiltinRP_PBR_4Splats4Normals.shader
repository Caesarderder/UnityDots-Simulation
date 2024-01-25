// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polaris/BuiltinRP/Terrain/PBR_4Splats4Normals"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Control0("Control0", 2D) = "black" {}
		_Splat0("Splat0", 2D) = "white" {}
		_Splat1("Splat1", 2D) = "white" {}
		_Splat2("Splat2", 2D) = "white" {}
		_Splat3("Splat3", 2D) = "white" {}
		_Normal0("Normal0", 2D) = "bump" {}
		_Normal1("Normal1", 2D) = "bump" {}
		_Normal2("Normal2", 2D) = "bump" {}
		_Normal3("Normal3", 2D) = "bump" {}
		_Metallic0("Metallic0", Range( 0 , 1)) = 0
		_Metallic1("Metallic1", Range( 0 , 1)) = 0
		_Metallic2("Metallic2", Range( 0 , 1)) = 0
		_Metallic3("Metallic3", Range( 0 , 1)) = 0
		_Smoothness0("Smoothness0", Range( 0 , 1)) = 0
		_Smoothness1("Smoothness1", Range( 0 , 1)) = 0
		_Smoothness2("Smoothness2", Range( 0 , 1)) = 0
		_Smoothness3("Smoothness3", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "DisableBatching" = "True" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows noinstancing 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Normal0;
		uniform sampler2D _Splat0;
		uniform float4 _Splat0_ST;
		uniform sampler2D _Control0;
		uniform sampler2D _Normal1;
		uniform sampler2D _Splat1;
		uniform float4 _Splat1_ST;
		uniform sampler2D _Normal2;
		uniform sampler2D _Splat2;
		uniform float4 _Splat2_ST;
		uniform sampler2D _Normal3;
		uniform sampler2D _Splat3;
		uniform float4 _Splat3_ST;
		uniform float4 _Color;
		uniform float _Metallic0;
		uniform float _Metallic1;
		uniform float _Metallic2;
		uniform float _Metallic3;
		uniform float _Smoothness0;
		uniform float _Smoothness1;
		uniform float _Smoothness2;
		uniform float _Smoothness3;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv0_Splat0 = i.uv_texcoord * _Splat0_ST.xy + _Splat0_ST.zw;
			half4 tex2DNode31_g14 = tex2D( _Control0, i.uv_texcoord );
			half4 break125_g14 = tex2DNode31_g14;
			float2 uv0_Splat1 = i.uv_texcoord * _Splat1_ST.xy + _Splat1_ST.zw;
			float2 uv0_Splat2 = i.uv_texcoord * _Splat2_ST.xy + _Splat2_ST.zw;
			float2 uv0_Splat3 = i.uv_texcoord * _Splat3_ST.xy + _Splat3_ST.zw;
			half3 lerpResult128_g14 = lerp( float3(0,0,0.5) , UnpackNormal( ( ( tex2D( _Normal0, uv0_Splat0 ) * break125_g14.r ) + ( tex2D( _Normal1, uv0_Splat1 ) * break125_g14.g ) + ( tex2D( _Normal2, uv0_Splat2 ) * break125_g14.b ) + ( tex2D( _Normal3, uv0_Splat3 ) * break125_g14.a ) ) ) , ( break125_g14.r + break125_g14.g + break125_g14.b + break125_g14.a ));
			float3 normalVector114_g14 = lerpResult128_g14;
			float3 normalVector80 = normalVector114_g14;
			o.Normal = normalVector80;
			float4 _Color59 = _Color;
			half4 break34_g14 = tex2DNode31_g14;
			float4 splatColor52_g14 = ( ( tex2D( _Splat0, uv0_Splat0 ) * break34_g14.r ) + ( tex2D( _Splat1, uv0_Splat1 ) * break34_g14.g ) + ( tex2D( _Splat2, uv0_Splat2 ) * break34_g14.b ) + ( tex2D( _Splat3, uv0_Splat3 ) * break34_g14.a ) );
			float4 albedo55 = ( _Color59 * splatColor52_g14 );
			o.Albedo = albedo55.rgb;
			float _Metallic014 = _Metallic0;
			float _Metallic013_g14 = _Metallic014;
			half4 break67_g14 = tex2DNode31_g14;
			float _Metallic119 = _Metallic1;
			float _Metallic116_g14 = _Metallic119;
			float _Metallic222 = _Metallic2;
			float _Metallic221_g14 = _Metallic222;
			float _Metallic316 = _Metallic3;
			float _Metallic326_g14 = _Metallic316;
			float metallicValue77_g14 = ( ( _Metallic013_g14 * break67_g14.r ) + ( _Metallic116_g14 * break67_g14.g ) + ( _Metallic221_g14 * break67_g14.b ) + ( _Metallic326_g14 * break67_g14.a ) );
			float metallicValue56 = metallicValue77_g14;
			o.Metallic = metallicValue56;
			float _Smoothness027 = _Smoothness0;
			float _Smoothness014_g14 = _Smoothness027;
			half4 break80_g14 = tex2DNode31_g14;
			float _Smoothness118 = _Smoothness1;
			float _Smoothness119_g14 = _Smoothness118;
			float _Smoothness223 = _Smoothness2;
			float _Smoothness223_g14 = _Smoothness223;
			float _Smoothness315 = _Smoothness3;
			float _Smoothness327_g14 = _Smoothness315;
			float smoothnessValue82_g14 = ( ( _Smoothness014_g14 * break80_g14.r ) + ( _Smoothness119_g14 * break80_g14.g ) + ( _Smoothness223_g14 * break80_g14.b ) + ( _Smoothness327_g14 * break80_g14.a ) );
			float smoothnessValue57 = smoothnessValue82_g14;
			o.Smoothness = smoothnessValue57;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18000
1244;197;1906;885;1272.526;278.2686;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;5;-2582.276,-1258.525;Inherit;False;1849.843;1153.783;;36;73;72;71;70;69;68;67;66;11;9;6;12;8;13;10;7;59;23;26;58;22;16;15;14;18;19;27;31;24;29;30;17;21;25;28;20;Inputs;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;68;-1299.69,-326.4331;Float;True;Property;_Normal3;Normal3;9;0;Create;True;0;0;False;0;None;None;False;bump;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;69;-1303.146,-751.9892;Float;True;Property;_Normal1;Normal1;7;0;Create;True;0;0;False;0;None;None;False;bump;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;66;-1308.146,-983.9891;Float;True;Property;_Normal0;Normal0;6;0;Create;True;0;0;False;0;None;None;False;bump;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;10;-1862.24,-544.5253;Float;True;Property;_Splat2;Splat2;4;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-2520.276,-444.0007;Float;False;Property;_Smoothness2;Smoothness2;16;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-2527.276,-662.0006;Float;False;Property;_Smoothness1;Smoothness1;15;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;13;-1863.784,-328.9691;Float;True;Property;_Splat3;Splat3;5;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;67;-1298.146,-541.9893;Float;True;Property;_Normal2;Normal2;8;0;Create;True;0;0;False;0;None;None;False;bump;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;31;-1872.24,-1208.525;Float;True;Property;_Control0;Control0;1;0;Create;True;0;0;False;0;None;None;False;black;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-2518.366,-319.5409;Float;False;Property;_Metallic3;Metallic3;13;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;7;-1872.24,-986.5253;Float;True;Property;_Splat0;Splat0;2;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;8;-1867.24,-754.5253;Float;True;Property;_Splat1;Splat1;3;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-2529.276,-751.0005;Float;False;Property;_Metallic1;Metallic1;11;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-2530.276,-894.5005;Float;False;Property;_Smoothness0;Smoothness0;14;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-2516.366,-230.5408;Float;False;Property;_Smoothness3;Smoothness3;17;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-2522.276,-533.0005;Float;False;Property;_Metallic2;Metallic2;12;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-2532.276,-983.5005;Float;False;Property;_Metallic0;Metallic0;10;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;70;-1001.69,-322.4331;Float;False;_Normal3;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;22;-2194.277,-531.0005;Float;False;_Metallic2;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;16;-2190.367,-317.5409;Float;False;_Metallic3;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;19;-2196.277,-748.0005;Float;False;_Metallic1;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;18;-2199.277,-660.0006;Float;False;_Smoothness1;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;58;-2524.98,-1184.629;Float;False;Property;_Color;Color;0;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;11;-1564.24,-540.5253;Float;False;_Splat2;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;12;-1565.784,-324.9691;Float;False;_Splat3;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;6;-1574.24,-982.5253;Float;False;_Splat0;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;14;-2204.277,-981.5005;Float;False;_Metallic0;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;26;-1574.24,-1204.525;Float;False;_Control;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;23;-2192.277,-442.0007;Float;False;_Smoothness2;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;71;-1010.146,-979.9891;Float;False;_Normal0;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;72;-1005.146,-747.9892;Float;False;_Normal1;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.CommentaryNode;65;-2670.415,123.9284;Inherit;False;1423.094;1443.226;;23;50;52;53;51;46;48;47;49;79;78;77;76;42;39;40;41;38;57;56;55;60;61;80;Albedo, Normal, Metallic, Smoothness;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;9;-1569.24,-750.5253;Float;False;_Splat1;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;27;-2202.277,-892.5005;Float;False;_Smoothness0;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;15;-2188.367,-228.5408;Float;False;_Smoothness3;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;73;-1000.146,-537.9894;Float;False;_Normal2;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;42;-2590.789,491.9283;Inherit;False;12;_Splat3;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;77;-2594.786,664.4363;Inherit;False;72;_Normal1;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;51;-2617.415,1408.555;Inherit;False;23;_Smoothness2;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;39;-2595.789,253.9284;Inherit;False;6;_Splat0;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;40;-2594.789,333.9284;Inherit;False;9;_Splat1;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;78;-2595.786,584.4363;Inherit;False;71;_Normal0;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;52;-2621.415,1248.555;Inherit;False;27;_Smoothness0;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;41;-2591.789,413.9284;Inherit;False;11;_Splat2;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;53;-2616.415,1486.555;Inherit;False;15;_Smoothness3;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;48;-2595.415,919.555;Inherit;False;14;_Metallic0;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;59;-2193.028,-1186.264;Float;False;_Color;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;47;-2591.415,1079.555;Inherit;False;22;_Metallic2;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;46;-2594.415,999.5549;Inherit;False;19;_Metallic1;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;49;-2590.415,1157.555;Inherit;False;16;_Metallic3;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;76;-2591.786,744.4363;Inherit;False;73;_Normal2;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;79;-2590.786,822.4363;Inherit;False;70;_Normal3;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;50;-2620.415,1328.555;Inherit;False;18;_Smoothness1;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;38;-2598.789,173.9284;Inherit;False;26;_Control;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.FunctionNode;82;-2130.286,499.9363;Inherit;False;Sample4Splats4NormalsPBR;-1;;14;194e84b9304a1e045b74746905759949;0;17;54;SAMPLER2D;0;False;55;SAMPLER2D;0;False;56;SAMPLER2D;0;False;57;SAMPLER2D;0;False;58;SAMPLER2D;0;False;100;SAMPLER2D;0;False;101;SAMPLER2D;0;False;105;SAMPLER2D;0;False;102;SAMPLER2D;0;False;59;FLOAT;0;False;61;FLOAT;0;False;63;FLOAT;0;False;65;FLOAT;0;False;60;FLOAT;0;False;62;FLOAT;0;False;64;FLOAT;0;False;66;FLOAT;0;False;4;COLOR;0;FLOAT3;132;FLOAT;79;FLOAT;92
Node;AmplifyShaderEditor.GetLocalVarNode;61;-2006.835,362.3217;Inherit;False;59;_Color;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;-1729.835,466.3217;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;55;-1491.863,461.057;Float;False;albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;80;-1490.6,559.9918;Float;False;normalVector;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;56;-1491.193,662.8465;Float;False;metallicValue;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;57;-1514.221,760.9777;Float;False;smoothnessValue;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;63;-346.2018,101.0122;Inherit;False;56;metallicValue;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;64;-365.2018,201.0122;Inherit;False;57;smoothnessValue;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;62;-329.2018,-75.98779;Inherit;False;55;albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;81;-357.1204,12.14795;Inherit;False;80;normalVector;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;83;0,0;Half;False;True;-1;2;;0;0;Standard;Polaris/BuiltinRP/Terrain/PBR_4Splats4Normals;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;True;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;70;0;68;0
WireConnection;22;0;20;0
WireConnection;16;0;24;0
WireConnection;19;0;29;0
WireConnection;18;0;17;0
WireConnection;11;0;10;0
WireConnection;12;0;13;0
WireConnection;6;0;7;0
WireConnection;14;0;30;0
WireConnection;26;0;31;0
WireConnection;23;0;21;0
WireConnection;71;0;66;0
WireConnection;72;0;69;0
WireConnection;9;0;8;0
WireConnection;27;0;25;0
WireConnection;15;0;28;0
WireConnection;73;0;67;0
WireConnection;59;0;58;0
WireConnection;82;54;38;0
WireConnection;82;55;39;0
WireConnection;82;56;40;0
WireConnection;82;57;41;0
WireConnection;82;58;42;0
WireConnection;82;100;78;0
WireConnection;82;101;77;0
WireConnection;82;105;76;0
WireConnection;82;102;79;0
WireConnection;82;59;48;0
WireConnection;82;61;46;0
WireConnection;82;63;47;0
WireConnection;82;65;49;0
WireConnection;82;60;52;0
WireConnection;82;62;50;0
WireConnection;82;64;51;0
WireConnection;82;66;53;0
WireConnection;60;0;61;0
WireConnection;60;1;82;0
WireConnection;55;0;60;0
WireConnection;80;0;82;132
WireConnection;56;0;82;79
WireConnection;57;0;82;92
WireConnection;83;0;62;0
WireConnection;83;1;81;0
WireConnection;83;3;63;0
WireConnection;83;4;64;0
ASEEND*/
//CHKSM=2ABE3EE18BD72489DE7741F481B3709643BD298F