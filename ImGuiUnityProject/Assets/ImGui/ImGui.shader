Shader "UI/ImGui"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint vertexId : SV_VertexID;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            struct Vertex{
                float2 pos;
                float2 uv;
                uint color;
            };

            StructuredBuffer<uint> _indexBuffer;
            StructuredBuffer<Vertex> _vertexBuffer;

            float4x4 projection;
            uint idxOffset;
            uint vtxOffset;

            v2f vert (appdata v)
            {
                int triangleNum = v.vertexId / 3;
                int localIndex = v.vertexId % 3;

                if (localIndex == 2) {
                    localIndex = 1;
                } else if (localIndex == 1) {
                    localIndex = 2;
                }

                int index = localIndex + triangleNum * 3;

                Vertex mesh_vertex = 
                    _vertexBuffer[_indexBuffer[idxOffset + index] + vtxOffset];
                v2f o;
                

                o.vertex = mul(projection, float4(mesh_vertex.pos, 0.0, 1.0));
                o.uv = mesh_vertex.uv;

                float r = (mesh_vertex.color & 0x000000FF);
                float g = (mesh_vertex.color & 0x0000FF00) >> 8;
                float b = (mesh_vertex.color & 0x00FF0000) >> 16;
                float a = (mesh_vertex.color & 0xFF000000) >> 24;

                o.color = float4(r,g,b,a) / 255.0;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = i.color * tex2D(_MainTex, i.uv).bbbb;
                return col;
            }
            ENDCG
        }
    }
}
