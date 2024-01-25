Shader "Hidden/GizmosField"
{
    Properties
    {
        _Color("Color", Color) = (.25, .5, .5, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        CGINCLUDE
        #include "UnityCG.cginc"

        struct AppData
        {
            uint vertexId : SV_VertexID;
            uint instanceId : SV_InstanceID;
        };

        StructuredBuffer<float> _HeightField;
        StructuredBuffer<int> _ObstacleField;
        int _Width;
        int _Height;
        float4x4 _Transform;
        float4 _Color;

        static const float2 _Vertices[4] =
        {
            float2(0.0, 0.0),
            float2(1.0, 0.0),
            float2(1.0, 1.0),
            float2(0.0, 1.0),
        };

        static const int _Triangles[6] =
        {
            0, 2, 1,
            0, 3, 2,
        };

        static const int _Lines[8] =
        {
            0, 1,
            1, 2,
            2, 3,
            3, 0,
        };

        int GetCellIndex(int2 position) { return position.y * _Width + position.x; }

        bool IsValidCell(int2 cell) { return cell.x >= 0 && cell.y >= 0 && cell.x < _Width && cell.y < _Height && _ObstacleField[GetCellIndex(cell)] == 0; }

        float GetHeight(int2 cell)
        {
            return _HeightField[GetCellIndex(cell)];
        }

        float SampleHeight(float2 position)
        {
            // Find closest cell center that coordinates are both less than of the position
            int2 cellA = (int2) floor(position - 0.5);
            int2 cellB = cellA + int2(1, 0);
            int2 cellC = cellA + int2(0, 1);
            int2 cellD = cellA + int2(1, 1);

            float2 delta = saturate(position - ((float2) cellA + 0.5));
            float2 oneMinusDelta = 1.0 - delta;

            float2 q11 = IsValidCell(cellA) ? float2(GetHeight(cellA), 1) : 0;
            float2 q12 = IsValidCell(cellB) ? float2(GetHeight(cellB), 1) : 0;
            float2 q21 = IsValidCell(cellC) ? float2(GetHeight(cellC), 1) : 0;
            float2 q22 = IsValidCell(cellD) ? float2(GetHeight(cellD), 1) : 0;

            float4 weights;
            weights.x = oneMinusDelta.x * oneMinusDelta.y;
            weights.y = oneMinusDelta.x * delta.y;
            weights.z = delta.x * oneMinusDelta.y;
            weights.w = delta.x * delta.y;

            float2 interpolated =
                q11 * weights.x +
                q21 * weights.y +
                q12 * weights.z +
                q22 * weights.w;

            if (interpolated.y == 0)
                return 0;

            return interpolated.x / interpolated.y + 0.001;
        }
        ENDCG

        Pass // Depth Prepass
        {
            ColorMask A

            CGPROGRAM
            #pragma target 4.0
            #pragma vertex Vert
            #pragma fragment Frag
            //#pragma enable_d3d11_debug_symbols

            float4 Vert(AppData i) : SV_POSITION
            {
                uint index = _Triangles[i.vertexId];
                int2 cell = int2(i.instanceId % _Width, i.instanceId / _Width);

                // Skip non valid cells
                if (!IsValidCell(cell))
                    return 0;

                float2 vertex = cell + _Vertices[index];
                float height = SampleHeight(vertex);

                float4 positionOS = mul(_Transform, float4(vertex, -height, 1));

                float4 positionSS = UnityObjectToClipPos(positionOS);
                return positionSS;
            }

            float4 Frag() : SV_Target
            {
                return 0;
            }
            ENDCG
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
            ZTest Equal

            CGPROGRAM
            #pragma target 4.0
            #pragma vertex Vert
            #pragma fragment Frag
            //#pragma enable_d3d11_debug_symbols

            float4 Vert(AppData i) : SV_POSITION
            {
                uint index = _Triangles[i.vertexId];
                int2 cell = int2(i.instanceId % _Width, i.instanceId / _Width);

                // Skip non valid cells
                if (!IsValidCell(cell))
                    return 0;

                float2 vertex = cell + _Vertices[index];
                float height = SampleHeight(vertex);

                float4 positionOS = mul(_Transform, float4(vertex, -height, 1));

                float4 positionSS = UnityObjectToClipPos(positionOS);
                return positionSS;
            }

            float4 Frag() : SV_Target
            {
                return _Color;
            }
            ENDCG
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
            ZTest Equal

            CGPROGRAM
            #pragma target 4.0
            #pragma vertex Vert
            #pragma fragment Frag
            //#pragma enable_d3d11_debug_symbols

            StructuredBuffer<float4> _ColorField;

            struct VertData
            {
                float4 positionSS : SV_POSITION;
                float4 color : COLOR;
            };

            VertData Vert(AppData i)
            {
                VertData o;

                uint index = _Triangles[i.vertexId];
                int2 cell = int2(i.instanceId % _Width, i.instanceId / _Width);

                o.color = _ColorField[GetCellIndex(cell)];

                // Skip non valid cells
                if (!IsValidCell(cell))
                {
                    o.positionSS = 0;
                    return o;
                }

                float2 vertex = cell + _Vertices[index];
                float height = SampleHeight(vertex);

                float4 positionOS = mul(_Transform, float4(vertex, -height, 1));

                o.positionSS = UnityObjectToClipPos(positionOS);
                return o;
            }

            float4 Frag(VertData i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
            //ZTest Equal

            CGPROGRAM
            #pragma target 4.0
            #pragma vertex Vert
            #pragma fragment Frag
            //#pragma enable_d3d11_debug_symbols

            float4 Vert(AppData i) : SV_POSITION
            {
                uint index = _Lines[i.vertexId];
                int2 cell = int2(i.instanceId % _Width, i.instanceId / _Width);

                // Skip non valid cells
                if (!IsValidCell(cell))
                    return 0;

                float2 vertex = cell + _Vertices[index];
                float height = SampleHeight(vertex);

                float4 positionOS = mul(_Transform, float4(vertex, -height, 1));

                float4 positionSS = UnityObjectToClipPos(positionOS);
                positionSS.z += 0.001;
                return positionSS;
            }

            float4 Frag() : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
