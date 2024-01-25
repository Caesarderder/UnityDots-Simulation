Shader "Polaris/BuiltinRP/Foliage/TreeBillboard"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		[Normal] _BumpMap ("Normal Map", 2D) = "bump" {}
		_Cutoff ("Alpha cutoff", Range(0.01,0.9)) = 0.3
    }
    SubShader
    {
        Tags 
		{ 
			"Queue"  = "AlphaTest"
			"RenderType" = "TransparentCutout"
            "DisableBatching" = "True"
            "IgnoreProjector" = "True"
		}

        CGPROGRAM
		#include "GriffinBillboardCommon.cginc" 
        #pragma surface surf Lambert alphatest:_Cutoff addshadow vertex:TreeBillboardVert nolightmap nodirlightmap
        #pragma multi_compile_instancing
        #pragma instancing_options nolodfade nolightmap
        #pragma target 3.0 

        sampler2D _MainTex;
		sampler2D _BumpMap;
        fixed4 _Color; 

        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here 
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.imageTexcoord) * _Color; 
            o.Albedo = c;
             
			fixed4 normalColor = (tex2D(_BumpMap, IN.imageTexcoord));
			fixed3 normal = UnpackNormal(normalColor);
			
			o.Normal = normal;
            o.Alpha = c.a;
        } 
        ENDCG
    }
    FallBack "Diffuse"
}
