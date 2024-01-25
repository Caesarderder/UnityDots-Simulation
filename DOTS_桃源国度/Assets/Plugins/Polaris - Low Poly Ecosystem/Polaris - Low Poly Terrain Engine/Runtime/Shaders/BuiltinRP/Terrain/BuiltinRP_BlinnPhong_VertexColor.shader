// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polaris/BuiltinRP/Terrain/BlinnPhong_VertexColor"
{
	Properties
	{
		_SpecColor("Specular Color",Color)=(1,1,1,1)
		_Color("Color", Color) = (1,1,1,1)
		_Specular("Specular", Range( 0 , 1)) = 0.5
		_Gloss("Gloss", Float) = 1
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
			float4 vertexColor : COLOR;
		};

		uniform float4 _Color;
		uniform float _Specular;
		uniform float _Gloss;

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 _Color5 = _Color;
			float4 albedo10 = ( _Color5 * i.vertexColor );
			o.Albedo = albedo10.rgb;
			float _Specular22 = _Specular;
			o.Specular = _Specular22;
			float _Gloss23 = _Gloss;
			o.Gloss = _Gloss23;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18000
1244;197;1906;885;1318.705;591.2894;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;6;-1027,-650;Inherit;False;587;419;;6;23;22;21;20;5;4;Properties;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;4;-941,-600;Float;False;Property;_Color;Color;1;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;11;-1163.788,-95.79129;Inherit;False;731;378;;4;7;8;9;10;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;5;-655,-597;Float;False;_Color;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;7;-1102.788,80.20869;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;8;-1113.788,-45.79129;Inherit;False;5;_Color;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-866.7881,15.20871;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-1007.711,-422.6499;Float;False;Property;_Specular;Specular;2;0;Create;True;0;0;False;0;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-879.5408,-334.6118;Float;False;Property;_Gloss;Gloss;3;0;Create;True;0;0;False;0;1;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;10;-675.7881,15.20871;Float;False;albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;22;-657.1277,-426.2672;Float;False;_Specular;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;23;-663.4377,-343.6288;Float;False;_Gloss;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;16;-207.7881,-420.7913;Inherit;False;10;albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;24;-219.705,-318.2894;Inherit;False;22;_Specular;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;25;-208.705,-230.2894;Inherit;False;23;_Gloss;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;19;69,-386;Half;False;True;-1;2;;0;0;BlinnPhong;Polaris/BuiltinRP/Terrain/BlinnPhong_VertexColor;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;True;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;0;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;5;0;4;0
WireConnection;9;0;8;0
WireConnection;9;1;7;0
WireConnection;10;0;9;0
WireConnection;22;0;20;0
WireConnection;23;0;21;0
WireConnection;19;0;16;0
WireConnection;19;3;24;0
WireConnection;19;4;25;0
ASEEND*/
//CHKSM=0365DD07D49874EBC9EE5285F280A3D11964812B