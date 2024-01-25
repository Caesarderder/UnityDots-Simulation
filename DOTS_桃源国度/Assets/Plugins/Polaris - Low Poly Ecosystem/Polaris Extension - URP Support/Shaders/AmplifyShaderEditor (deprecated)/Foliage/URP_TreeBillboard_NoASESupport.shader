Shader "Polaris/Deprecated/URP/Foliage/TreeBillboard"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("MainTex", 2D) = "white" {}
        _BumpMap("BumpMap", 2D) = "bump" {}
        _Cutoff("Cutoff", Range(0 , 1)) = 0.5
    }


        SubShader
        {

            Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

            Cull Back
            HLSLINCLUDE
            #pragma target 3.0
            ENDHLSL

            Pass
            {

                Tags { "LightMode" = "UniversalForward" }

                Name "Base"
                Blend One Zero
                ZWrite On
                ZTest LEqual
                Offset 0 , 0
                ColorMask RGBA

                HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x


            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag

            #define ASE_SRP_VERSION 50702
            #define _NORMALMAP 1
            #define _AlphaClip 1


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            float4 _Color;
            sampler2D _MainTex;
            sampler2D _BumpMap;
            float _Cutoff;

            struct GraphVertexInput
            {
                float4 vertex : POSITION;
                float3 ase_normal : NORMAL;
                float4 ase_tangent : TANGENT;
                float4 texcoord1 : TEXCOORD1;
                float4 ase_texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct GraphVertexOutput
            {
                float4 clipPos                : SV_POSITION;
                float4 lightmapUVOrVertexSH	  : TEXCOORD0;
                half4 fogFactorAndVertexLight : TEXCOORD1; // x: fogFactor, yzw: vertex light
                float4 shadowCoord            : TEXCOORD2;
                float4 tSpace0					: TEXCOORD3;
                float4 tSpace1					: TEXCOORD4;
                float4 tSpace2					: TEXCOORD5;
                float4 ase_texcoord7 : TEXCOORD7;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };


            //Custom function for getting billboard image texture coordinate
            float4 _ImageTexcoords[256];
            int _ImageCount;

            void GetImageTexcoord(float3 worldNormal, inout float4 texcoord)
            {
                float dotZ = dot(worldNormal, float3(0,0,1));
                float dotX = dot(worldNormal, float3(1,0,0));
                float rad = atan2(dotZ, dotX);
                rad = (rad + TWO_PI) % TWO_PI;
                float f = rad / TWO_PI - 0.5 / _ImageCount;
                int imageIndex = f * _ImageCount;

                float4 rect = _ImageTexcoords[imageIndex];
                float2 min = rect.xy;
                float2 max = rect.xy + rect.zw;

                float2 result = float2(
                    lerp(min.x, max.x, texcoord.x),
                    lerp(min.y, max.y, texcoord.y));
                texcoord = float4(result,0,0);
            }

            GraphVertexOutput vert(GraphVertexInput v)
            {
                GraphVertexOutput o = (GraphVertexOutput)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                GetImageTexcoord(TransformObjectToWorldNormal(v.ase_normal), v.ase_texcoord);
                o.ase_texcoord7.xy = v.ase_texcoord.xy;

                //setting value to unused interpolator channels and avoid initialization warnings
                o.ase_texcoord7.zw = 0;
                float3 vertexValue = float3(0, 0, 0);
                #ifdef ASE_ABSOLUTE_VERTEX_POS
                v.vertex.xyz = vertexValue;
                #else
                v.vertex.xyz += vertexValue;
                #endif
                v.ase_normal = v.ase_normal;

                // Vertex shader outputs defined by graph
                float3 lwWNormal = TransformObjectToWorldNormal(v.ase_normal);
                float3 lwWorldPos = TransformObjectToWorld(v.vertex.xyz);
                float3 lwWTangent = TransformObjectToWorldDir(v.ase_tangent.xyz);
                float3 lwWBinormal = normalize(cross(lwWNormal, lwWTangent) * v.ase_tangent.w);
                o.tSpace0 = float4(lwWTangent.x, lwWBinormal.x, lwWNormal.x, lwWorldPos.x);
                o.tSpace1 = float4(lwWTangent.y, lwWBinormal.y, lwWNormal.y, lwWorldPos.y);
                o.tSpace2 = float4(lwWTangent.z, lwWBinormal.z, lwWNormal.z, lwWorldPos.z);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);

                // We either sample GI from lightmap or SH.
                // Lightmap UV and vertex SH coefficients use the same interpolator ("float2 lightmapUV" for lightmap or "half3 vertexSH" for SH)
                // see DECLARE_LIGHTMAP_OR_SH macro.
                // The following funcions initialize the correct variable with correct data
                OUTPUT_LIGHTMAP_UV(v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy);
                OUTPUT_SH(lwWNormal, o.lightmapUVOrVertexSH.xyz);

                half3 vertexLight = VertexLighting(vertexInput.positionWS, lwWNormal);
                half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
                o.clipPos = vertexInput.positionCS;

            #ifdef _MAIN_LIGHT_SHADOWS
                o.shadowCoord = GetShadowCoord(vertexInput);
            #endif
                return o;
            }

            half4 frag(GraphVertexOutput IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                float3 WorldSpaceNormal = normalize(float3(IN.tSpace0.z,IN.tSpace1.z,IN.tSpace2.z));
                float3 WorldSpaceTangent = float3(IN.tSpace0.x,IN.tSpace1.x,IN.tSpace2.x);
                float3 WorldSpaceBiTangent = float3(IN.tSpace0.y,IN.tSpace1.y,IN.tSpace2.y);
                float3 WorldSpacePosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
                float3 WorldSpaceViewDirection = SafeNormalize(_WorldSpaceCameraPos.xyz - WorldSpacePosition);

                float4 _Color5 = _Color;
                float2 uv015 = IN.ase_texcoord7.xy * float2(1,1) + float2(0,0);
                float4 tex2DNode13 = tex2D(_MainTex, uv015);
                float4 albedoColor18 = (_Color5 * tex2DNode13);

                float2 uv027 = IN.ase_texcoord7.xy * float2(1,1) + float2(0,0);
                float3 break30 = reflect(UnpackNormalScale(tex2D(_BumpMap, uv027), 1.0) , float3(0,-1,0));
                float3 appendResult33 = (float3(break30.x , (1.0 - break30.y) , (1.0 - break30.z)));
                float3 normal34 = appendResult33;

                float alpha19 = tex2DNode13.a;

                float _Cutoff11 = _Cutoff;


                float3 Albedo = albedoColor18.rgb;
                float3 Normal = normal34;
                float3 Emission = 0;
                float3 Specular = float3(0.5, 0.5, 0.5);
                float Metallic = 0.0;
                float Smoothness = 0.0;
                float Occlusion = 1;
                float Alpha = alpha19;
                float AlphaClipThreshold = _Cutoff11;

                InputData inputData;
                inputData.positionWS = WorldSpacePosition;

        #ifdef _NORMALMAP
                inputData.normalWS = normalize(TransformTangentToWorld(Normal, half3x3(WorldSpaceTangent, WorldSpaceBiTangent, WorldSpaceNormal)));
        #else
            #if !SHADER_HINT_NICE_QUALITY
                inputData.normalWS = WorldSpaceNormal;
            #else
                inputData.normalWS = normalize(WorldSpaceNormal);
            #endif
        #endif

        #if !SHADER_HINT_NICE_QUALITY
                // viewDirection should be normalized here, but we avoid doing it as it's close enough and we save some ALU.
                inputData.viewDirectionWS = WorldSpaceViewDirection;
        #else
                inputData.viewDirectionWS = normalize(WorldSpaceViewDirection);
        #endif

                inputData.shadowCoord = IN.shadowCoord;

                inputData.fogCoord = IN.fogFactorAndVertexLight.x;
                inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
                inputData.bakedGI = SAMPLE_GI(IN.lightmapUVOrVertexSH.xy, IN.lightmapUVOrVertexSH.xyz, inputData.normalWS);

                half4 color = UniversalFragmentPBR(
                    inputData,
                    Albedo,
                    Metallic,
                    Specular,
                    Smoothness,
                    Occlusion,
                    Emission,
                    Alpha);

            #ifdef TERRAIN_SPLAT_ADDPASS
                color.rgb = MixFogColor(color.rgb, half3(0, 0, 0), IN.fogFactorAndVertexLight.x);
            #else
                color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
            #endif

        #if _AlphaClip
                clip(Alpha - AlphaClipThreshold);
        #endif

        #if ASE_LW_FINAL_COLOR_ALPHA_MULTIPLY
                color.rgb *= color.a;
        #endif
                return color;
            }

            ENDHLSL
        }


        Pass
        {

            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
                // Required to compile gles 2.0 with standard srp library
                #pragma prefer_hlslcc gles
                #pragma exclude_renderers d3d11_9x


                //--------------------------------------
                // GPU Instancing
                #pragma multi_compile_instancing

                #pragma vertex ShadowPassVertex
                #pragma fragment ShadowPassFragment

                #define ASE_SRP_VERSION 50702
                #define _AlphaClip 1


                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

                struct GraphVertexInput
                {
                    float4 vertex : POSITION;
                    float3 ase_normal : NORMAL;
                    float4 ase_texcoord : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                sampler2D _MainTex;
                float _Cutoff;

                struct VertexOutput
                {
                    float4 clipPos      : SV_POSITION;
                    float4 ase_texcoord7 : TEXCOORD7;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                //Custom function for getting billboard image texture coordinate
                float4 _ImageTexcoords[256];
                int _ImageCount;

                void GetImageTexcoord(float3 worldNormal, inout float4 texcoord)
                {
                    float dotZ = dot(worldNormal, float3(0,0,1));
                    float dotX = dot(worldNormal, float3(1,0,0));
                    float rad = atan2(dotZ, dotX);
                    rad = (rad + TWO_PI) % TWO_PI;
                    float f = rad / TWO_PI - 0.5 / _ImageCount;
                    int imageIndex = f * _ImageCount;

                    float4 rect = _ImageTexcoords[imageIndex];
                    float2 min = rect.xy;
                    float2 max = rect.xy + rect.zw;

                    float2 result = float2(
                        lerp(min.x, max.x, texcoord.x),
                        lerp(min.y, max.y, texcoord.y));
                    texcoord = float4(result,0,0);
                }

                // x: global clip space bias, y: normal world space bias
                float3 _LightDirection;

                VertexOutput ShadowPassVertex(GraphVertexInput v)
                {
                    VertexOutput o;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    GetImageTexcoord(TransformObjectToWorldNormal(v.ase_normal), v.ase_texcoord);
                    o.ase_texcoord7.xy = v.ase_texcoord.xy;

                    //setting value to unused interpolator channels and avoid initialization warnings
                    o.ase_texcoord7.zw = 0;
                    float3 vertexValue = float3(0,0,0);
                    #ifdef ASE_ABSOLUTE_VERTEX_POS
                    v.vertex.xyz = vertexValue;
                    #else
                    v.vertex.xyz += vertexValue;
                    #endif

                    v.ase_normal = v.ase_normal;

                    float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                    float3 normalWS = TransformObjectToWorldDir(v.ase_normal);

                    float invNdotL = 1.0 - saturate(dot(_LightDirection, normalWS));
                    float scale = invNdotL * _ShadowBias.y;

                    // normal bias is negative since we want to apply an inset normal offset
                    positionWS = _LightDirection * _ShadowBias.xxx + positionWS;
                    positionWS = normalWS * scale.xxx + positionWS;
                    float4 clipPos = TransformWorldToHClip(positionWS);

                    // _ShadowBias.x sign depens on if platform has reversed z buffer
                    //clipPos.z += _ShadowBias.x;

                #if UNITY_REVERSED_Z
                    clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
                #endif
                    o.clipPos = clipPos;

                    return o;
                }

                half4 ShadowPassFragment(VertexOutput IN) : SV_TARGET
                {
                    UNITY_SETUP_INSTANCE_ID(IN);

                   float2 uv015 = IN.ase_texcoord7.xy * float2(1,1) + float2(0,0);
                   float4 tex2DNode13 = tex2D(_MainTex, uv015);
                   float alpha19 = tex2DNode13.a;

                   float _Cutoff11 = _Cutoff;


                    float Alpha = alpha19;
                    float AlphaClipThreshold = _Cutoff11;

             #if _AlphaClip
                    clip(Alpha - AlphaClipThreshold);
            #endif
                    return 0;
                }

                ENDHLSL
            }


            Pass
            {

                Name "DepthOnly"
                Tags { "LightMode" = "DepthOnly" }

                ZWrite On
                ColorMask 0

                HLSLPROGRAM
                    // Required to compile gles 2.0 with standard srp library
                    #pragma prefer_hlslcc gles
                    #pragma exclude_renderers d3d11_9x

                    //--------------------------------------
                    // GPU Instancing
                    #pragma multi_compile_instancing

                    #pragma vertex vert
                    #pragma fragment frag

                    #define ASE_SRP_VERSION 50702
                    #define _AlphaClip 1


                    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
                    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
                    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

                    sampler2D _MainTex;
                    float _Cutoff;

                    struct GraphVertexInput
                    {
                        float4 vertex : POSITION;
                        float3 ase_normal : NORMAL;
                        float4 ase_texcoord : TEXCOORD0;
                        UNITY_VERTEX_INPUT_INSTANCE_ID
                    };

                    struct VertexOutput
                    {
                        float4 clipPos      : SV_POSITION;
                        float4 ase_texcoord : TEXCOORD0;
                        UNITY_VERTEX_INPUT_INSTANCE_ID
                        UNITY_VERTEX_OUTPUT_STEREO
                    };

                    //Custom function for getting billboard image texture coordinate
                    float4 _ImageTexcoords[256];
                    int _ImageCount;

                    void GetImageTexcoord(float3 worldNormal, inout float4 texcoord)
                    {
                        float dotZ = dot(worldNormal, float3(0,0,1));
                        float dotX = dot(worldNormal, float3(1,0,0));
                        float rad = atan2(dotZ, dotX);
                        rad = (rad + TWO_PI) % TWO_PI;
                        float f = rad / TWO_PI - 0.5 / _ImageCount;
                        int imageIndex = f * _ImageCount;

                        float4 rect = _ImageTexcoords[imageIndex];
                        float2 min = rect.xy;
                        float2 max = rect.xy + rect.zw;

                        float2 result = float2(
                            lerp(min.x, max.x, texcoord.x),
                            lerp(min.y, max.y, texcoord.y));
                        texcoord = float4(result,0,0);
                    }

                    VertexOutput vert(GraphVertexInput v)
                    {
                        VertexOutput o = (VertexOutput)0;
                        UNITY_SETUP_INSTANCE_ID(v);
                        UNITY_TRANSFER_INSTANCE_ID(v, o);
                        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                        GetImageTexcoord(TransformObjectToWorldNormal(v.ase_normal), v.ase_texcoord);
                        o.ase_texcoord.xy = v.ase_texcoord.xy;

                        //setting value to unused interpolator channels and avoid initialization warnings
                        o.ase_texcoord.zw = 0;
                        float3 vertexValue = float3(0,0,0);
                        #ifdef ASE_ABSOLUTE_VERTEX_POS
                        v.vertex.xyz = vertexValue;
                        #else
                        v.vertex.xyz += vertexValue;
                        #endif

                        v.ase_normal = v.ase_normal;

                        o.clipPos = TransformObjectToHClip(v.vertex.xyz);
                        return o;
                    }

                    half4 frag(VertexOutput IN) : SV_TARGET
                    {
                        UNITY_SETUP_INSTANCE_ID(IN);

                        float2 uv015 = IN.ase_texcoord.xy * float2(1,1) + float2(0,0);
                        float4 tex2DNode13 = tex2D(_MainTex, uv015);
                        float alpha19 = tex2DNode13.a;

                        float _Cutoff11 = _Cutoff;


                        float Alpha = alpha19;
                        float AlphaClipThreshold = _Cutoff11;

                 #if _AlphaClip
                        clip(Alpha - AlphaClipThreshold);
                #endif
                        return 0;
                    }
                    ENDHLSL
                }

                    // This pass it not used during regular rendering, only for lightmap baking.

                    Pass
                    {

                        Name "Meta"
                        Tags { "LightMode" = "Meta" }

                        Cull Off

                        HLSLPROGRAM
                        // Required to compile gles 2.0 with standard srp library
                        #pragma prefer_hlslcc gles
                        #pragma exclude_renderers d3d11_9x

                        #pragma vertex vert
                        #pragma fragment frag

                        #define ASE_SRP_VERSION 50702
                        #define _AlphaClip 1


                        uniform float4 _MainTex_ST;

                        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
                        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
                        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

                        float4 _Color;
                        sampler2D _MainTex;
                        float _Cutoff;

                        #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
                        #pragma shader_feature EDITOR_VISUALIZATION


                        struct GraphVertexInput
                        {
                            float4 vertex : POSITION;
                            float3 ase_normal : NORMAL;
                            float4 texcoord1 : TEXCOORD1;
                            float4 texcoord2 : TEXCOORD2;
                            float4 ase_texcoord : TEXCOORD0;
                            UNITY_VERTEX_INPUT_INSTANCE_ID
                        };

                        struct VertexOutput
                        {
                            float4 clipPos      : SV_POSITION;
                            float4 ase_texcoord : TEXCOORD0;
                            UNITY_VERTEX_INPUT_INSTANCE_ID
                            UNITY_VERTEX_OUTPUT_STEREO
                        };

                        //Custom function for getting billboard image texture coordinate
                        float4 _ImageTexcoords[256];
                        int _ImageCount;

                        void GetImageTexcoord(float3 worldNormal, inout float4 texcoord)
                        {
                            float dotZ = dot(worldNormal, float3(0,0,1));
                            float dotX = dot(worldNormal, float3(1,0,0));
                            float rad = atan2(dotZ, dotX);
                            rad = (rad + TWO_PI) % TWO_PI;
                            float f = rad / TWO_PI - 0.5 / _ImageCount;
                            int imageIndex = f * _ImageCount;

                            float4 rect = _ImageTexcoords[imageIndex];
                            float2 min = rect.xy;
                            float2 max = rect.xy + rect.zw;

                            float2 result = float2(
                                lerp(min.x, max.x, texcoord.x),
                                lerp(min.y, max.y, texcoord.y));
                            texcoord = float4(result,0,0);
                        }

                        VertexOutput vert(GraphVertexInput v)
                        {
                            VertexOutput o = (VertexOutput)0;
                            UNITY_SETUP_INSTANCE_ID(v);
                            UNITY_TRANSFER_INSTANCE_ID(v, o);
                            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                            GetImageTexcoord(TransformObjectToWorldNormal(v.ase_normal), v.ase_texcoord);
                            o.ase_texcoord.xy = v.ase_texcoord.xy;

                            //setting value to unused interpolator channels and avoid initialization warnings
                            o.ase_texcoord.zw = 0;

                            float3 vertexValue = float3(0,0,0);
                            #ifdef ASE_ABSOLUTE_VERTEX_POS
                            v.vertex.xyz = vertexValue;
                            #else
                            v.vertex.xyz += vertexValue;
                            #endif

                            v.ase_normal = v.ase_normal;

                            o.clipPos = MetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord1.xy, unity_LightmapST, unity_DynamicLightmapST);
                            return o;
                        }

                        half4 frag(VertexOutput IN) : SV_TARGET
                        {
                            UNITY_SETUP_INSTANCE_ID(IN);

                            float4 _Color5 = _Color;
                            float2 uv015 = IN.ase_texcoord.xy * float2(1,1) + float2(0,0);
                            float4 tex2DNode13 = tex2D(_MainTex, uv015);
                            float4 albedoColor18 = (_Color5 * tex2DNode13);

                            float alpha19 = tex2DNode13.a;

                            float _Cutoff11 = _Cutoff;


                            float3 Albedo = albedoColor18.rgb;
                            float3 Emission = 0;
                            float Alpha = alpha19;
                            float AlphaClipThreshold = _Cutoff11;

                     #if _AlphaClip
                            clip(Alpha - AlphaClipThreshold);
                    #endif

                            MetaInput metaInput = (MetaInput)0;
                            metaInput.Albedo = Albedo;
                            metaInput.Emission = Emission;

                            return MetaFragment(metaInput);
                        }
                        ENDHLSL
                    }

        }
            Fallback "Hidden/InternalErrorShader"


}