// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polaris/BuiltinRP/Terrain/Lambert_4Splats4Normals"
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
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "DisableBatching" = "True" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Lambert keepalpha addshadow fullforwardshadows noinstancing 
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

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv0_Splat0 = i.uv_texcoord * _Splat0_ST.xy + _Splat0_ST.zw;
			half4 tex2DNode31_g15 = tex2D( _Control0, i.uv_texcoord );
			half4 break125_g15 = tex2DNode31_g15;
			float2 uv0_Splat1 = i.uv_texcoord * _Splat1_ST.xy + _Splat1_ST.zw;
			float2 uv0_Splat2 = i.uv_texcoord * _Splat2_ST.xy + _Splat2_ST.zw;
			float2 uv0_Splat3 = i.uv_texcoord * _Splat3_ST.xy + _Splat3_ST.zw;
			half3 lerpResult128_g15 = lerp( float3(0,0,0.5) , UnpackNormal( ( ( tex2D( _Normal0, uv0_Splat0 ) * break125_g15.r ) + ( tex2D( _Normal1, uv0_Splat1 ) * break125_g15.g ) + ( tex2D( _Normal2, uv0_Splat2 ) * break125_g15.b ) + ( tex2D( _Normal3, uv0_Splat3 ) * break125_g15.a ) ) ) , ( break125_g15.r + break125_g15.g + break125_g15.b + break125_g15.a ));
			float3 normalVector114_g15 = lerpResult128_g15;
			float3 normalVector80 = normalVector114_g15;
			o.Normal = normalVector80;
			float4 _Color59 = _Color;
			half4 break34_g15 = tex2DNode31_g15;
			float4 splatColor52_g15 = ( ( tex2D( _Splat0, uv0_Splat0 ) * break34_g15.r ) + ( tex2D( _Splat1, uv0_Splat1 ) * break34_g15.g ) + ( tex2D( _Splat2, uv0_Splat2 ) * break34_g15.b ) + ( tex2D( _Splat3, uv0_Splat3 ) * break34_g15.a ) );
			float4 albedo55 = ( _Color59 * splatColor52_g15 );
			o.Albedo = albedo55.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18000
1244;197;1906;885;5893.356;1293.213;4.729704;True;False
Node;AmplifyShaderEditor.CommentaryNode;5;-2582.276,-1258.525;Inherit;False;1849.843;1153.783;;20;73;72;71;70;69;68;67;66;11;9;6;12;8;13;10;7;59;26;58;31;Inputs;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;69;-1303.146,-751.9892;Float;True;Property;_Normal1;Normal1;7;0;Create;True;0;0;False;0;None;None;False;bump;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;66;-1308.146,-983.9891;Float;True;Property;_Normal0;Normal0;6;0;Create;True;0;0;False;0;None;None;False;bump;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;10;-1862.24,-544.5253;Float;True;Property;_Splat2;Splat2;4;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;31;-1872.24,-1208.525;Float;True;Property;_Control0;Control0;1;0;Create;True;0;0;False;0;None;None;False;black;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;13;-1863.784,-328.9691;Float;True;Property;_Splat3;Splat3;5;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;68;-1299.69,-326.4331;Float;True;Property;_Normal3;Normal3;9;0;Create;True;0;0;False;0;None;None;False;bump;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;7;-1872.24,-986.5253;Float;True;Property;_Splat0;Splat0;2;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;8;-1867.24,-754.5253;Float;True;Property;_Splat1;Splat1;3;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;67;-1298.146,-541.9893;Float;True;Property;_Normal2;Normal2;8;0;Create;True;0;0;False;0;None;None;False;bump;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;71;-1010.146,-979.9891;Float;False;_Normal0;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;6;-1574.24,-982.5253;Float;False;_Splat0;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;26;-1574.24,-1204.525;Float;False;_Control;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;72;-1005.146,-747.9892;Float;False;_Normal1;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;12;-1565.784,-324.9691;Float;False;_Splat3;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;73;-1000.146,-537.9894;Float;False;_Normal2;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;70;-1001.69,-322.4331;Float;False;_Normal3;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;11;-1564.24,-540.5253;Float;False;_Splat2;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.ColorNode;58;-2524.98,-1184.629;Float;False;Property;_Color;Color;0;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;65;-2670.415,123.9284;Inherit;False;1408.794;881.6259;;14;77;79;76;38;42;41;78;40;39;84;80;55;60;61;Albedo, Normal, Metallic, Smoothness;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;9;-1569.24,-750.5253;Float;False;_Splat1;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;78;-2499.586,662.4363;Inherit;False;71;_Normal0;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;76;-2495.586,822.4363;Inherit;False;73;_Normal2;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;38;-2502.589,251.9284;Inherit;False;26;_Control;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;41;-2495.589,491.9284;Inherit;False;11;_Splat2;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;42;-2494.589,569.9283;Inherit;False;12;_Splat3;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;40;-2498.589,411.9284;Inherit;False;9;_Splat1;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;77;-2497.286,739.8362;Inherit;False;72;_Normal1;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;59;-2193.028,-1186.264;Float;False;_Color;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;79;-2494.586,900.4363;Inherit;False;70;_Normal3;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;39;-2499.589,331.9284;Inherit;False;6;_Splat0;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.FunctionNode;84;-2116.618,505.0056;Inherit;False;Sample4Splats4Normals;-1;;15;45f7d1aa5e9d58743aec839bd3751783;0;9;54;SAMPLER2D;0;False;55;SAMPLER2D;0;False;56;SAMPLER2D;0;False;57;SAMPLER2D;0;False;58;SAMPLER2D;0;False;100;SAMPLER2D;0;False;101;SAMPLER2D;0;False;105;SAMPLER2D;0;False;102;SAMPLER2D;0;False;2;COLOR;0;FLOAT3;132
Node;AmplifyShaderEditor.GetLocalVarNode;61;-2006.835,362.3217;Inherit;False;59;_Color;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;-1729.835,466.3217;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;80;-1490.6,559.9918;Float;False;normalVector;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;55;-1491.863,461.057;Float;False;albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;62;-329.2018,-75.98779;Inherit;False;55;albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;81;-357.1204,12.14795;Inherit;False;80;normalVector;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;83;0,0;Half;False;True;-1;2;ASEMaterialInspector;0;0;Lambert;Polaris/BuiltinRP/Terrain/Lambert_4Splats4Normals;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;True;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;71;0;66;0
WireConnection;6;0;7;0
WireConnection;26;0;31;0
WireConnection;72;0;69;0
WireConnection;12;0;13;0
WireConnection;73;0;67;0
WireConnection;70;0;68;0
WireConnection;11;0;10;0
WireConnection;9;0;8;0
WireConnection;59;0;58;0
WireConnection;84;54;38;0
WireConnection;84;55;39;0
WireConnection;84;56;40;0
WireConnection;84;57;41;0
WireConnection;84;58;42;0
WireConnection;84;100;78;0
WireConnection;84;101;77;0
WireConnection;84;105;76;0
WireConnection;84;102;79;0
WireConnection;60;0;61;0
WireConnection;60;1;84;0
WireConnection;80;0;84;132
WireConnection;55;0;60;0
WireConnection;83;0;62;0
WireConnection;83;1;81;0
ASEEND*/
//CHKSM=D795A960BA64D4A8B091C859EC8E5EC086ED2A60