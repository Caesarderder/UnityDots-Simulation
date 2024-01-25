// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polaris/BuiltinRP/Terrain/PBR_8Splats"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Control0("Control0", 2D) = "black" {}
		_Control1("Control1", 2D) = "black" {}
		_Splat0("Splat0", 2D) = "white" {}
		_Splat1("Splat1", 2D) = "white" {}
		_Splat2("Splat2", 2D) = "white" {}
		_Splat3("Splat3", 2D) = "white" {}
		_Splat4("Splat4", 2D) = "white" {}
		_Splat5("Splat5", 2D) = "white" {}
		_Splat6("Splat6", 2D) = "white" {}
		_Splat7("Splat7", 2D) = "white" {}
		_Metallic0("Metallic0", Range( 0 , 1)) = 0
		_Metallic1("Metallic1", Range( 0 , 1)) = 0
		_Metallic2("Metallic2", Range( 0 , 1)) = 0
		_Metallic3("Metallic3", Range( 0 , 1)) = 0
		_Metallic4("Metallic4", Range( 0 , 1)) = 0
		_Metallic5("Metallic5", Range( 0 , 1)) = 0
		_Metallic6("Metallic6", Range( 0 , 1)) = 0
		_Metallic7("Metallic7", Range( 0 , 1)) = 0
		_Smoothness0("Smoothness0", Range( 0 , 1)) = 0
		_Smoothness1("Smoothness1", Range( 0 , 1)) = 0
		_Smoothness2("Smoothness2", Range( 0 , 1)) = 0
		_Smoothness3("Smoothness3", Range( 0 , 1)) = 0
		_Smoothness4("Smoothness4", Range( 0 , 1)) = 0
		_Smoothness5("Smoothness5", Range( 0 , 1)) = 0
		_Smoothness6("Smoothness6", Range( 0 , 1)) = 0
		_Smoothness7("Smoothness7", Range( 0 , 1)) = 0
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
		uniform sampler2D _Splat4;
		uniform float4 _Splat4_ST;
		uniform sampler2D _Control1;
		uniform sampler2D _Splat5;
		uniform float4 _Splat5_ST;
		uniform sampler2D _Splat6;
		uniform float4 _Splat6_ST;
		uniform sampler2D _Splat7;
		uniform float4 _Splat7_ST;
		uniform float _Metallic0;
		uniform float _Metallic1;
		uniform float _Metallic2;
		uniform float _Metallic3;
		uniform float _Metallic4;
		uniform float _Metallic5;
		uniform float _Metallic6;
		uniform float _Metallic7;
		uniform float _Smoothness0;
		uniform float _Smoothness1;
		uniform float _Smoothness2;
		uniform float _Smoothness3;
		uniform float _Smoothness4;
		uniform float _Smoothness5;
		uniform float _Smoothness6;
		uniform float _Smoothness7;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 _Color59 = _Color;
			float2 uv0_Splat0 = i.uv_texcoord * _Splat0_ST.xy + _Splat0_ST.zw;
			half4 tex2DNode31_g17 = tex2D( _Control0, i.uv_texcoord );
			half4 break34_g17 = tex2DNode31_g17;
			float2 uv0_Splat1 = i.uv_texcoord * _Splat1_ST.xy + _Splat1_ST.zw;
			float2 uv0_Splat2 = i.uv_texcoord * _Splat2_ST.xy + _Splat2_ST.zw;
			float2 uv0_Splat3 = i.uv_texcoord * _Splat3_ST.xy + _Splat3_ST.zw;
			float4 splatColor52_g17 = ( ( tex2D( _Splat0, uv0_Splat0 ) * break34_g17.r ) + ( tex2D( _Splat1, uv0_Splat1 ) * break34_g17.g ) + ( tex2D( _Splat2, uv0_Splat2 ) * break34_g17.b ) + ( tex2D( _Splat3, uv0_Splat3 ) * break34_g17.a ) );
			float4 splatColor055 = splatColor52_g17;
			float2 uv0_Splat4 = i.uv_texcoord * _Splat4_ST.xy + _Splat4_ST.zw;
			half4 tex2DNode31_g16 = tex2D( _Control1, i.uv_texcoord );
			half4 break34_g16 = tex2DNode31_g16;
			float2 uv0_Splat5 = i.uv_texcoord * _Splat5_ST.xy + _Splat5_ST.zw;
			float2 uv0_Splat6 = i.uv_texcoord * _Splat6_ST.xy + _Splat6_ST.zw;
			float2 uv0_Splat7 = i.uv_texcoord * _Splat7_ST.xy + _Splat7_ST.zw;
			float4 splatColor52_g16 = ( ( tex2D( _Splat4, uv0_Splat4 ) * break34_g16.r ) + ( tex2D( _Splat5, uv0_Splat5 ) * break34_g16.g ) + ( tex2D( _Splat6, uv0_Splat6 ) * break34_g16.b ) + ( tex2D( _Splat7, uv0_Splat7 ) * break34_g16.a ) );
			float4 splatColor1117 = splatColor52_g16;
			float4 albedoColor125 = ( _Color59 * ( splatColor055 + splatColor1117 ) );
			o.Albedo = albedoColor125.rgb;
			float _Metallic014 = _Metallic0;
			float _Metallic013_g17 = _Metallic014;
			half4 break67_g17 = tex2DNode31_g17;
			float _Metallic119 = _Metallic1;
			float _Metallic116_g17 = _Metallic119;
			float _Metallic222 = _Metallic2;
			float _Metallic221_g17 = _Metallic222;
			float _Metallic316 = _Metallic3;
			float _Metallic326_g17 = _Metallic316;
			float metallicValue77_g17 = ( ( _Metallic013_g17 * break67_g17.r ) + ( _Metallic116_g17 * break67_g17.g ) + ( _Metallic221_g17 * break67_g17.b ) + ( _Metallic326_g17 * break67_g17.a ) );
			float metallicValue056 = metallicValue77_g17;
			float _Metallic499 = _Metallic4;
			float _Metallic013_g16 = _Metallic499;
			half4 break67_g16 = tex2DNode31_g16;
			float _Metallic5100 = _Metallic5;
			float _Metallic116_g16 = _Metallic5100;
			float _Metallic696 = _Metallic6;
			float _Metallic221_g16 = _Metallic696;
			float _Metallic797 = _Metallic7;
			float _Metallic326_g16 = _Metallic797;
			float metallicValue77_g16 = ( ( _Metallic013_g16 * break67_g16.r ) + ( _Metallic116_g16 * break67_g16.g ) + ( _Metallic221_g16 * break67_g16.b ) + ( _Metallic326_g16 * break67_g16.a ) );
			float metallicValue1118 = metallicValue77_g16;
			float metallic129 = ( metallicValue056 + metallicValue1118 );
			o.Metallic = metallic129;
			float _Smoothness027 = _Smoothness0;
			float _Smoothness014_g17 = _Smoothness027;
			half4 break80_g17 = tex2DNode31_g17;
			float _Smoothness118 = _Smoothness1;
			float _Smoothness119_g17 = _Smoothness118;
			float _Smoothness223 = _Smoothness2;
			float _Smoothness223_g17 = _Smoothness223;
			float _Smoothness315 = _Smoothness3;
			float _Smoothness327_g17 = _Smoothness315;
			float smoothnessValue82_g17 = ( ( _Smoothness014_g17 * break80_g17.r ) + ( _Smoothness119_g17 * break80_g17.g ) + ( _Smoothness223_g17 * break80_g17.b ) + ( _Smoothness327_g17 * break80_g17.a ) );
			float smoothnessValue057 = smoothnessValue82_g17;
			float _Smoothness494 = _Smoothness4;
			float _Smoothness014_g16 = _Smoothness494;
			half4 break80_g16 = tex2DNode31_g16;
			float _Smoothness595 = _Smoothness5;
			float _Smoothness119_g16 = _Smoothness595;
			float _Smoothness698 = _Smoothness6;
			float _Smoothness223_g16 = _Smoothness698;
			float _Smoothness793 = _Smoothness7;
			float _Smoothness327_g16 = _Smoothness793;
			float smoothnessValue82_g16 = ( ( _Smoothness014_g16 * break80_g16.r ) + ( _Smoothness119_g16 * break80_g16.g ) + ( _Smoothness223_g16 * break80_g16.b ) + ( _Smoothness327_g16 * break80_g16.a ) );
			float smoothnessValue1119 = smoothnessValue82_g16;
			float smoothness133 = ( smoothnessValue057 + smoothnessValue1119 );
			o.Smoothness = smoothness133;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18000
1244;197;1906;885;2236.417;106.7359;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;5;-3537.489,-2432.581;Inherit;False;2620.827;1205.286;;54;92;95;96;98;88;99;84;100;89;90;83;93;91;94;87;73;66;67;68;69;58;70;71;72;85;97;86;59;19;23;22;14;18;16;11;15;6;9;12;27;26;31;17;21;7;13;10;24;8;25;29;20;30;28;Inputs;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;88;-2842.811,-1488.449;Float;False;Property;_Metallic7;Metallic7;18;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-3475.489,-1618.057;Float;False;Property;_Smoothness2;Smoothness2;21;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;83;-1515.058,-2377.599;Float;True;Property;_Control1;Control1;2;0;Create;True;0;0;False;0;None;None;False;black;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;92;-2844.721,-1612.909;Float;False;Property;_Smoothness6;Smoothness6;25;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;87;-2853.721,-1919.909;Float;False;Property;_Metallic5;Metallic5;16;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;13;-2070.795,-1503.025;Float;True;Property;_Splat3;Splat3;6;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;89;-2856.721,-2152.409;Float;False;Property;_Metallic4;Metallic4;15;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;67;-1505.157,-1716.046;Float;True;Property;_Splat6;Splat6;9;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;86;-2854.721,-2063.409;Float;False;Property;_Smoothness4;Smoothness4;23;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;69;-1510.157,-1926.045;Float;True;Property;_Splat5;Splat5;8;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;85;-2846.721,-1701.909;Float;False;Property;_Metallic6;Metallic6;17;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;68;-1506.701,-1500.49;Float;True;Property;_Splat7;Splat7;10;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;66;-1515.157,-2158.046;Float;True;Property;_Splat4;Splat4;7;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;90;-2840.811,-1399.449;Float;False;Property;_Smoothness7;Smoothness7;26;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-3471.579,-1404.597;Float;False;Property;_Smoothness3;Smoothness3;22;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;10;-2069.251,-1718.582;Float;True;Property;_Splat2;Splat2;5;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-3482.489,-1836.057;Float;False;Property;_Smoothness1;Smoothness1;20;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-3473.579,-1493.597;Float;False;Property;_Metallic3;Metallic3;14;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-3487.489,-2157.557;Float;False;Property;_Metallic0;Metallic0;11;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-3485.489,-2068.557;Float;False;Property;_Smoothness0;Smoothness0;19;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-3477.489,-1707.057;Float;False;Property;_Metallic2;Metallic2;13;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;8;-2074.251,-1928.582;Float;True;Property;_Splat1;Splat1;4;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;31;-2079.251,-2382.581;Float;True;Property;_Control0;Control0;1;0;Create;True;0;0;False;0;None;None;False;black;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;7;-2079.251,-2160.582;Float;True;Property;_Splat0;Splat0;3;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;91;-2851.721,-1830.909;Float;False;Property;_Smoothness5;Smoothness5;24;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-3484.489,-1925.057;Float;False;Property;_Metallic1;Metallic1;12;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;15;-3143.58,-1402.597;Float;False;_Smoothness3;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;96;-2518.722,-1699.909;Float;False;_Metallic6;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;100;-2520.722,-1916.909;Float;False;_Metallic5;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;93;-2512.812,-1397.449;Float;False;_Smoothness7;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;84;-1217.058,-2373.599;Float;False;_Control1;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;94;-2526.722,-2061.409;Float;False;_Smoothness4;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;73;-1207.158,-1712.046;Float;False;_Splat6;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;98;-2516.722,-1610.909;Float;False;_Smoothness6;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;97;-2514.812,-1486.449;Float;False;_Metallic7;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;72;-1212.158,-1922.045;Float;False;_Splat5;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;70;-1208.702,-1496.49;Float;False;_Splat7;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;11;-1771.251,-1714.582;Float;False;_Splat2;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;99;-2528.722,-2150.409;Float;False;_Metallic4;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;71;-1217.158,-2154.046;Float;False;_Splat4;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;18;-3154.49,-1834.057;Float;False;_Smoothness1;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;102;-3547.73,304.941;Inherit;False;1423.094;1149.181;;17;119;118;117;116;115;114;113;112;111;110;109;108;107;106;105;104;103;Second layer;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;19;-3151.49,-1922.057;Float;False;_Metallic1;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;14;-3159.49,-2155.557;Float;False;_Metallic0;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;23;-3147.49,-1616.057;Float;False;_Smoothness2;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;16;-3145.58,-1491.597;Float;False;_Metallic3;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;12;-1772.795,-1499.026;Float;False;_Splat3;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;22;-3149.49,-1705.057;Float;False;_Metallic2;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;26;-1781.251,-2378.581;Float;False;_Control0;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;95;-2523.722,-1828.909;Float;False;_Smoothness5;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;9;-1776.251,-1924.582;Float;False;_Splat1;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.CommentaryNode;65;-3539.165,-1017.926;Inherit;False;1423.094;1149.181;;17;38;47;46;52;49;48;50;51;53;57;56;55;40;42;41;39;101;First layer;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;6;-1781.251,-2156.582;Float;False;_Splat0;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;27;-3157.49,-2066.557;Float;False;_Smoothness0;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;115;-3469.104,594.9399;Inherit;False;73;_Splat6;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;110;-3476.104,354.9407;Inherit;False;84;_Control1;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;108;-3479.545,857.0566;Inherit;False;100;_Metallic5;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;42;-3459.539,-649.9265;Inherit;False;12;_Splat3;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;103;-3502.545,1266.058;Inherit;False;98;_Smoothness6;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;111;-3505.545,1186.058;Inherit;False;95;_Smoothness5;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;106;-3475.545,1015.058;Inherit;False;97;_Metallic7;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;107;-3501.545,1344.058;Inherit;False;93;_Smoothness7;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;40;-3463.539,-807.9264;Inherit;False;9;_Splat1;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;112;-3468.104,672.9397;Inherit;False;70;_Splat7;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;104;-3506.545,1106.058;Inherit;False;94;_Smoothness4;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;47;-3467.98,-385.8093;Inherit;False;22;_Metallic2;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;39;-3464.539,-887.9265;Inherit;False;6;_Splat0;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;105;-3480.545,777.0569;Inherit;False;99;_Metallic4;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;51;-3493.98,-56.80935;Inherit;False;23;_Smoothness2;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;41;-3460.539,-727.9264;Inherit;False;11;_Splat2;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;50;-3496.98,-136.8093;Inherit;False;18;_Smoothness1;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;38;-3467.539,-967.9264;Inherit;False;26;_Control0;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;49;-3466.98,-307.8093;Inherit;False;16;_Metallic3;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;114;-3473.104,434.9402;Inherit;False;71;_Splat4;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;46;-3470.98,-465.8095;Inherit;False;19;_Metallic1;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;52;-3497.98,-216.8092;Inherit;False;27;_Smoothness0;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;113;-3472.104,514.9401;Inherit;False;72;_Splat5;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;48;-3471.98,-545.8093;Inherit;False;14;_Metallic0;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;53;-3492.98,21.19055;Inherit;False;15;_Smoothness3;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;109;-3476.545,937.0574;Inherit;False;96;_Metallic6;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;116;-3008.601,647.4875;Inherit;False;Sample4SplatsPBR;-1;;16;a6f307978ba7ac84eb0587d072b0df93;0;13;54;SAMPLER2D;0;False;55;SAMPLER2D;0;False;56;SAMPLER2D;0;False;57;SAMPLER2D;0;False;58;SAMPLER2D;0;False;59;FLOAT;0;False;61;FLOAT;0;False;63;FLOAT;0;False;65;FLOAT;0;False;60;FLOAT;0;False;62;FLOAT;0;False;64;FLOAT;0;False;66;FLOAT;0;False;3;COLOR;0;FLOAT;79;FLOAT;92
Node;AmplifyShaderEditor.FunctionNode;101;-3000.036,-675.3787;Inherit;False;Sample4SplatsPBR;-1;;17;a6f307978ba7ac84eb0587d072b0df93;0;13;54;SAMPLER2D;0;False;55;SAMPLER2D;0;False;56;SAMPLER2D;0;False;57;SAMPLER2D;0;False;58;SAMPLER2D;0;False;59;FLOAT;0;False;61;FLOAT;0;False;63;FLOAT;0;False;65;FLOAT;0;False;60;FLOAT;0;False;62;FLOAT;0;False;64;FLOAT;0;False;66;FLOAT;0;False;3;COLOR;0;FLOAT;79;FLOAT;92
Node;AmplifyShaderEditor.CommentaryNode;134;-1923.133,-1016.12;Inherit;False;1003;688.0636;;14;121;120;122;124;123;125;126;127;128;129;131;132;130;133;Blend Addictive;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;55;-2495.613,-678.7977;Float;False;splatColor0;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;117;-2504.178,644.0685;Float;False;splatColor1;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;58;-3480.193,-2358.685;Float;False;Property;_Color;Color;0;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;56;-2492.943,-580.0084;Float;False;metallicValue0;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;59;-3148.241,-2360.32;Float;False;_Color;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;120;-1869.316,-915.3834;Inherit;False;55;splatColor0;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;119;-2498.536,842.989;Float;False;smoothnessValue1;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;118;-2501.508,742.8578;Float;False;metallicValue1;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;57;-2489.971,-479.8773;Float;False;smoothnessValue0;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;121;-1870.723,-819.7946;Inherit;False;117;splatColor1;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;130;-1868.133,-532.0562;Inherit;False;57;smoothnessValue0;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;127;-1870.133,-632.5565;Inherit;False;118;metallicValue1;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;126;-1865.133,-721.5565;Inherit;False;56;metallicValue0;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;122;-1562.87,-874.6176;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;123;-1615.276,-966.1201;Inherit;False;59;_Color;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;131;-1873.133,-443.0561;Inherit;False;119;smoothnessValue1;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;132;-1569.133,-493.0561;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;124;-1378.276,-882.1201;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;128;-1566.133,-682.5565;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;125;-1172.276,-887.1201;Float;False;albedoColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;133;-1166.133,-494.0561;Float;False;smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;129;-1163.133,-683.5565;Float;False;metallic;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;63;-1498.471,190.0957;Inherit;False;129;metallic;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;62;-1522.471,67.09568;Inherit;False;125;albedoColor;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;64;-1517.471,305.0956;Inherit;False;133;smoothness;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;135;-1206.269,100.0835;Half;False;True;-1;2;;0;0;Standard;Polaris/BuiltinRP/Terrain/PBR_8Splats;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;True;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;15;0;28;0
WireConnection;96;0;85;0
WireConnection;100;0;87;0
WireConnection;93;0;90;0
WireConnection;84;0;83;0
WireConnection;94;0;86;0
WireConnection;73;0;67;0
WireConnection;98;0;92;0
WireConnection;97;0;88;0
WireConnection;72;0;69;0
WireConnection;70;0;68;0
WireConnection;11;0;10;0
WireConnection;99;0;89;0
WireConnection;71;0;66;0
WireConnection;18;0;17;0
WireConnection;19;0;29;0
WireConnection;14;0;30;0
WireConnection;23;0;21;0
WireConnection;16;0;24;0
WireConnection;12;0;13;0
WireConnection;22;0;20;0
WireConnection;26;0;31;0
WireConnection;95;0;91;0
WireConnection;9;0;8;0
WireConnection;6;0;7;0
WireConnection;27;0;25;0
WireConnection;116;54;110;0
WireConnection;116;55;114;0
WireConnection;116;56;113;0
WireConnection;116;57;115;0
WireConnection;116;58;112;0
WireConnection;116;59;105;0
WireConnection;116;61;108;0
WireConnection;116;63;109;0
WireConnection;116;65;106;0
WireConnection;116;60;104;0
WireConnection;116;62;111;0
WireConnection;116;64;103;0
WireConnection;116;66;107;0
WireConnection;101;54;38;0
WireConnection;101;55;39;0
WireConnection;101;56;40;0
WireConnection;101;57;41;0
WireConnection;101;58;42;0
WireConnection;101;59;48;0
WireConnection;101;61;46;0
WireConnection;101;63;47;0
WireConnection;101;65;49;0
WireConnection;101;60;52;0
WireConnection;101;62;50;0
WireConnection;101;64;51;0
WireConnection;101;66;53;0
WireConnection;55;0;101;0
WireConnection;117;0;116;0
WireConnection;56;0;101;79
WireConnection;59;0;58;0
WireConnection;119;0;116;92
WireConnection;118;0;116;79
WireConnection;57;0;101;92
WireConnection;122;0;120;0
WireConnection;122;1;121;0
WireConnection;132;0;130;0
WireConnection;132;1;131;0
WireConnection;124;0;123;0
WireConnection;124;1;122;0
WireConnection;128;0;126;0
WireConnection;128;1;127;0
WireConnection;125;0;124;0
WireConnection;133;0;132;0
WireConnection;129;0;128;0
WireConnection;135;0;62;0
WireConnection;135;3;63;0
WireConnection;135;4;64;0
ASEEND*/
//CHKSM=EA81135A233BBC75B4F502C18DFCC4DCA6848CA6