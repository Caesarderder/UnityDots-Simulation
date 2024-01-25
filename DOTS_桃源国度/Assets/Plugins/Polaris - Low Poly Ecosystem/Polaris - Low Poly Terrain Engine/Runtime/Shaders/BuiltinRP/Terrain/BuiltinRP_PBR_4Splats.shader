// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polaris/BuiltinRP/Terrain/PBR_4Splats"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Control0("Control0", 2D) = "black" {}
		_Splat0("Splat0", 2D) = "white" {}
		_Splat1("Splat1", 2D) = "white" {}
		_Splat2("Splat2", 2D) = "white" {}
		_Splat3("Splat3", 2D) = "white" {}
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
			float4 _Color59 = _Color;
			float2 uv_Splat0 = i.uv_texcoord * _Splat0_ST.xy + _Splat0_ST.zw;
			half4 tex2DNode31_g11 = tex2D( _Control0, i.uv_texcoord );
			half4 break34_g11 = tex2DNode31_g11;
			float2 uv_Splat1 = i.uv_texcoord * _Splat1_ST.xy + _Splat1_ST.zw;
			float2 uv_Splat2 = i.uv_texcoord * _Splat2_ST.xy + _Splat2_ST.zw;
			float2 uv_Splat3 = i.uv_texcoord * _Splat3_ST.xy + _Splat3_ST.zw;
			float4 splatColor52_g11 = ( ( tex2D( _Splat0, uv_Splat0 ) * break34_g11.r ) + ( tex2D( _Splat1, uv_Splat1 ) * break34_g11.g ) + ( tex2D( _Splat2, uv_Splat2 ) * break34_g11.b ) + ( tex2D( _Splat3, uv_Splat3 ) * break34_g11.a ) );
			float4 albedo55 = ( _Color59 * splatColor52_g11 );
			o.Albedo = albedo55.rgb;
			float _Metallic014 = _Metallic0;
			float _Metallic013_g11 = _Metallic014;
			half4 break67_g11 = tex2DNode31_g11;
			float _Metallic119 = _Metallic1;
			float _Metallic116_g11 = _Metallic119;
			float _Metallic222 = _Metallic2;
			float _Metallic221_g11 = _Metallic222;
			float _Metallic316 = _Metallic3;
			float _Metallic326_g11 = _Metallic316;
			float metallicValue77_g11 = ( ( _Metallic013_g11 * break67_g11.r ) + ( _Metallic116_g11 * break67_g11.g ) + ( _Metallic221_g11 * break67_g11.b ) + ( _Metallic326_g11 * break67_g11.a ) );
			float metallicValue56 = metallicValue77_g11;
			o.Metallic = metallicValue56;
			float _Smoothness027 = _Smoothness0;
			float _Smoothness014_g11 = _Smoothness027;
			half4 break80_g11 = tex2DNode31_g11;
			float _Smoothness118 = _Smoothness1;
			float _Smoothness119_g11 = _Smoothness118;
			float _Smoothness223 = _Smoothness2;
			float _Smoothness223_g11 = _Smoothness223;
			float _Smoothness315 = _Smoothness3;
			float _Smoothness327_g11 = _Smoothness315;
			float smoothnessValue82_g11 = ( ( _Smoothness014_g11 * break80_g11.r ) + ( _Smoothness119_g11 * break80_g11.g ) + ( _Smoothness223_g11 * break80_g11.b ) + ( _Smoothness327_g11 * break80_g11.a ) );
			float smoothnessValue57 = smoothnessValue82_g11;
			o.Smoothness = smoothnessValue57;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18400
835;319;1906;879;1313.265;197.4003;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;5;-2582.276,-1258.525;Inherit;False;1261.038;1159.556;;28;31;30;29;28;27;26;25;24;23;22;21;20;19;18;17;16;15;14;13;12;11;10;9;8;7;6;58;59;Inputs;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;13;-1863.784,-328.9691;Float;True;Property;_Splat3;Splat3;5;0;Create;True;0;0;False;0;False;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;10;-1862.24,-544.5253;Float;True;Property;_Splat2;Splat2;4;0;Create;True;0;0;False;0;False;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;17;-2527.276,-662.0006;Float;False;Property;_Smoothness1;Smoothness1;11;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-2530.276,-894.5005;Float;False;Property;_Smoothness0;Smoothness0;10;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-2520.276,-444.0007;Float;False;Property;_Smoothness2;Smoothness2;12;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;8;-1867.24,-754.5253;Float;True;Property;_Splat1;Splat1;3;0;Create;True;0;0;False;0;False;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;20;-2522.276,-533.0005;Float;False;Property;_Metallic2;Metallic2;8;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-2529.276,-751.0005;Float;False;Property;_Metallic1;Metallic1;7;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-2518.366,-319.5409;Float;False;Property;_Metallic3;Metallic3;9;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;31;-1872.24,-1208.525;Float;True;Property;_Control0;Control0;1;0;Create;True;0;0;False;0;False;None;None;False;black;LockedToTexture2D;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;30;-2532.276,-983.5005;Float;False;Property;_Metallic0;Metallic0;6;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-2516.366,-230.5408;Float;False;Property;_Smoothness3;Smoothness3;13;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;7;-1872.24,-986.5253;Float;True;Property;_Splat0;Splat0;2;0;Create;True;0;0;False;0;False;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.CommentaryNode;65;-2670.415,123.9284;Inherit;False;1445.194;1141.626;;19;38;39;42;41;52;53;49;46;40;51;50;48;47;54;61;60;55;56;57;Albedo, Metallic, Smoothness;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;23;-2192.277,-442.0007;Float;False;_Smoothness2;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;6;-1574.24,-982.5253;Float;False;_Splat0;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;18;-2199.277,-660.0006;Float;False;_Smoothness1;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;27;-2202.277,-892.5005;Float;False;_Smoothness0;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;19;-2196.277,-748.0005;Float;False;_Metallic1;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;15;-2188.367,-228.5408;Float;False;_Smoothness3;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;26;-1574.24,-1204.525;Float;False;_Control;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;11;-1564.24,-540.5253;Float;False;_Splat2;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.ColorNode;58;-2524.98,-1184.629;Float;False;Property;_Color;Color;0;0;Create;True;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;16;-2190.367,-317.5409;Float;False;_Metallic3;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;9;-1569.24,-750.5253;Float;False;_Splat1;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;12;-1565.784,-324.9691;Float;False;_Splat3;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;22;-2194.277,-531.0005;Float;False;_Metallic2;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;14;-2204.277,-981.5005;Float;False;_Metallic0;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;41;-2591.789,413.9284;Inherit;False;11;_Splat2;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;40;-2594.789,333.9284;Inherit;False;9;_Splat1;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;51;-2616.415,1072.555;Inherit;False;23;_Smoothness2;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;49;-2589.415,821.5549;Inherit;False;16;_Metallic3;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;47;-2590.415,743.555;Inherit;False;22;_Metallic2;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;46;-2593.415,663.555;Inherit;False;19;_Metallic1;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;48;-2594.415,583.555;Inherit;False;14;_Metallic0;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;53;-2615.415,1150.555;Inherit;False;15;_Smoothness3;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;59;-2193.028,-1186.264;Float;False;_Color;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;42;-2590.789,491.9283;Inherit;False;12;_Splat3;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;50;-2619.415,992.555;Inherit;False;18;_Smoothness1;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;39;-2595.789,253.9284;Inherit;False;6;_Splat0;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;52;-2620.415,912.555;Inherit;False;27;_Smoothness0;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;38;-2598.789,173.9284;Inherit;False;26;_Control;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;61;-2006.835,362.3217;Inherit;False;59;_Color;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;54;-2147.638,489.7514;Inherit;False;Sample4SplatsPBR;-1;;11;a6f307978ba7ac84eb0587d072b0df93;0;13;54;SAMPLER2D;0;False;55;SAMPLER2D;0;False;56;SAMPLER2D;0;False;57;SAMPLER2D;0;False;58;SAMPLER2D;0;False;59;FLOAT;0;False;61;FLOAT;0;False;63;FLOAT;0;False;65;FLOAT;0;False;60;FLOAT;0;False;62;FLOAT;0;False;64;FLOAT;0;False;66;FLOAT;0;False;3;COLOR;0;FLOAT;79;FLOAT;92
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;-1729.835,466.3217;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;57;-1493.221,664.9777;Float;False;smoothnessValue;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;55;-1491.863,461.057;Float;False;albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;56;-1489.193,560.8465;Float;False;metallicValue;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;63;-346.2018,101.0122;Inherit;False;56;metallicValue;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;62;-309.2018,0.01220703;Inherit;False;55;albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;64;-365.2018,201.0122;Inherit;False;57;smoothnessValue;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;66;0,0;Half;False;True;-1;2;;0;0;Standard;Polaris/BuiltinRP/Terrain/PBR_4Splats;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;True;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;1;False;-1;1;False;-1;0;1;False;-1;1;False;-1;1;False;-1;1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;23;0;21;0
WireConnection;6;0;7;0
WireConnection;18;0;17;0
WireConnection;27;0;25;0
WireConnection;19;0;29;0
WireConnection;15;0;28;0
WireConnection;26;0;31;0
WireConnection;11;0;10;0
WireConnection;16;0;24;0
WireConnection;9;0;8;0
WireConnection;12;0;13;0
WireConnection;22;0;20;0
WireConnection;14;0;30;0
WireConnection;59;0;58;0
WireConnection;54;54;38;0
WireConnection;54;55;39;0
WireConnection;54;56;40;0
WireConnection;54;57;41;0
WireConnection;54;58;42;0
WireConnection;54;59;48;0
WireConnection;54;61;46;0
WireConnection;54;63;47;0
WireConnection;54;65;49;0
WireConnection;54;60;52;0
WireConnection;54;62;50;0
WireConnection;54;64;51;0
WireConnection;54;66;53;0
WireConnection;60;0;61;0
WireConnection;60;1;54;0
WireConnection;57;0;54;92
WireConnection;55;0;60;0
WireConnection;56;0;54;79
WireConnection;66;0;62;0
WireConnection;66;3;63;0
WireConnection;66;4;64;0
ASEEND*/
//CHKSM=12598FE64267B9D0A77AA67F359DB06E05EEDE3E