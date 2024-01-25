Shader "MK/Toon/Gradient Skybox"
{
    Properties
    {
        _Color0 ("Color 1", Color) = (1, 0, 0, 1)
        _Color1 ("Color 2", Color) = (0, 1, 0, 1)
        _Rotation ("Rotation", Vector) = (0, 0, 0, 0)
        _Exponent ("Exp", Range (0, 5)) = 1
        _Intensity ("Intensity", Range (0, 3)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest

            #define PI 3.14159h

            #include "UnityCG.cginc"

            uniform half3 _Color0;
            uniform half3 _Color1;
            uniform float3 _Rotation;
            uniform half _Exponent;
            uniform half _Intensity;

            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput
            {
                float3 uv : TEXCOORD0;
                half3 direction : TEXCOORD1;
                float4 positionClip : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float3 ToRad(float3 degree)
            {
                return degree * PI / 180;
            }

            VertexOutput vert(VertexInput vertexInput)
            {
                VertexOutput vertexOutput;
                UNITY_SETUP_INSTANCE_ID(vertexInput);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(vertexOutput);
                vertexOutput.positionClip = UnityObjectToClipPos(vertexInput.vertex);
                vertexOutput.uv = vertexInput.vertex.xyz;
                float3 rotation = ToRad(_Rotation);
                float3 rotationSin, rotationCos;
                sincos(rotation, rotationSin, rotationCos);

                vertexOutput.direction = normalize(half3
                (
                    -rotationCos.z * rotationSin.y * rotationSin.x - rotationSin.z * rotationCos.x,
                    -rotationSin.z * rotationSin.y * rotationSin.x + rotationCos.z * rotationCos.x,
                    rotationCos.y * rotationSin.x
                ));
                return vertexOutput;
            }

            half4 frag(VertexOutput vertexOutput) : SV_Target
            {
                half i = dot(normalize(vertexOutput.uv), vertexOutput.direction) * 0.5 + 0.5;
                half3 color = lerp(_Color0, _Color1, pow(smoothstep(0.0, 1.0, i), _Exponent)) * _Intensity;

                return half4(color.rgb, 1);
            }
            ENDCG
        }
    }
}
