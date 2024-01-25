// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polaris/BuiltinRP/Terrain/BlinnPhong_4Splats"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_SpecColor("Specular Color",Color)=(1,1,1,1)
		_Specular("Specular", Range( 0 , 1)) = 0.5
		_Gloss("Gloss", Float) = 1
		_Control0("Control0", 2D) = "black" {}
		_Splat0("Splat0", 2D) = "white" {}
		_Splat1("Splat1", 2D) = "white" {}
		_Splat2("Splat2", 2D) = "white" {}
		_Splat3("Splat3", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "DisableBatching" = "True" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf BlinnPhong keepalpha addshadow fullforwardshadows noinstancing 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _Color;
		uniform sampler2D _Splat0;
		uniform float4 _Splat0_ST;
		uniform sampler2D _Control0;
		uniform sampler2D _Splat1;
		uniform float4 _Splat1_ST;
		uniform sampler2D _Splat2;
		uniform float4 _Splat2_ST;
		uniform sampler2D _Splat3;
		uniform float4 _Splat3_ST;
		uniform float _Specular;
		uniform float _Gloss;

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 _Color59 = _Color;
			float2 uv0_Splat0 = i.uv_texcoord * _Splat0_ST.xy + _Splat0_ST.zw;
			half4 break34_g1 = tex2D( _Control0, i.uv_texcoord );
			float2 uv0_Splat1 = i.uv_texcoord * _Splat1_ST.xy + _Splat1_ST.zw;
			float2 uv0_Splat2 = i.uv_texcoord * _Splat2_ST.xy + _Splat2_ST.zw;
			float2 uv0_Splat3 = i.uv_texcoord * _Splat3_ST.xy + _Splat3_ST.zw;
			float4 splatColor52_g1 = ( ( tex2D( _Splat0, uv0_Splat0 ) * break34_g1.r ) + ( tex2D( _Splat1, uv0_Splat1 ) * break34_g1.g ) + ( tex2D( _Splat2, uv0_Splat2 ) * break34_g1.b ) + ( tex2D( _Splat3, uv0_Splat3 ) * break34_g1.a ) );
			float4 albedo55 = ( _Color59 * splatColor52_g1 );
			o.Albedo = albedo55.rgb;
			float _Specular69 = _Specular;
			o.Specular = _Specular69;
			float _Gloss71 = _Gloss;
			o.Gloss = _Gloss71;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18000
1244;197;1906;885;2934.794;1314.469;1.3;True;False
Node;AmplifyShaderEditor.CommentaryNode;5;-2705.477,-1354.525;Inherit;False;1129.338;1349.356;;16;59;6;58;12;26;11;9;31;8;13;7;10;69;70;71;74;Inputs;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;10;-2636.643,-426.1253;Float;True;Property;_Splat2;Splat2;7;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;7;-2646.643,-868.1251;Float;True;Property;_Splat0;Splat0;5;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;13;-2638.187,-210.5692;Float;True;Property;_Splat3;Splat3;8;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;8;-2641.643,-636.1255;Float;True;Property;_Splat1;Splat1;6;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;31;-2646.643,-1090.125;Float;True;Property;_Control0;Control0;4;0;Create;True;0;0;False;0;None;None;False;black;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;9;-2343.642,-632.1255;Float;False;_Splat1;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;11;-2338.642,-422.1253;Float;False;_Splat2;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;26;-2348.642,-1086.125;Float;False;_Control;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;12;-2340.186,-206.5692;Float;False;_Splat3;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.ColorNode;58;-2648.181,-1280.629;Float;False;Property;_Color;Color;0;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;65;-2707.217,82.32843;Inherit;False;1217.75;531.1187;;9;40;41;39;38;42;67;55;60;61;Albedo, Metallic, Smoothness;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;6;-2348.642,-864.1251;Float;False;_Splat0;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;41;-2641.59,430.8285;Inherit;False;11;_Splat2;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;39;-2645.59,270.8285;Inherit;False;6;_Splat0;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;38;-2648.59,190.8285;Inherit;False;26;_Control;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;42;-2640.59,508.8288;Inherit;False;12;_Splat3;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;40;-2644.59,350.8284;Inherit;False;9;_Splat1;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;59;-2353.028,-1282.264;Float;False;_Color;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;67;-2343.82,308.0544;Inherit;False;Sample4Splats;-1;;1;563b5268ffef6d8479bb5dc663a94e53;0;5;54;SAMPLER2D;0;False;55;SAMPLER2D;0;False;56;SAMPLER2D;0;False;57;SAMPLER2D;0;False;58;SAMPLER2D;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;61;-2245.136,166.0216;Inherit;False;59;_Color;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;-1968.135,270.0217;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;74;-2094.186,-1278.196;Float;False;Property;_Specular;Specular;2;0;Create;True;0;0;False;0;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;70;-2093.016,-1087.158;Float;False;Property;_Gloss;Gloss;3;0;Create;True;0;0;False;0;1;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;55;-1730.163,264.757;Float;False;albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;69;-1796.016,-1281.158;Float;False;_Specular;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;71;-1790.016,-1087.158;Float;False;_Gloss;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;62;-309.2018,0.01220703;Inherit;False;55;albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;72;-317.1857,95.80402;Inherit;False;69;_Specular;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;73;-309.1857,170.804;Inherit;False;71;_Gloss;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;66;0,0;Half;False;True;-1;2;;0;0;BlinnPhong;Polaris/BuiltinRP/Terrain/BlinnPhong_4Splats;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;True;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;9;0;8;0
WireConnection;11;0;10;0
WireConnection;26;0;31;0
WireConnection;12;0;13;0
WireConnection;6;0;7;0
WireConnection;59;0;58;0
WireConnection;67;54;38;0
WireConnection;67;55;39;0
WireConnection;67;56;40;0
WireConnection;67;57;41;0
WireConnection;67;58;42;0
WireConnection;60;0;61;0
WireConnection;60;1;67;0
WireConnection;55;0;60;0
WireConnection;69;0;74;0
WireConnection;71;0;70;0
WireConnection;66;0;62;0
WireConnection;66;3;72;0
WireConnection;66;4;73;0
ASEEND*/
//CHKSM=E36E1117F712479A93E143ACFC095B2B53EB55FD