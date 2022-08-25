Shader "Unlit/Footman"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma instancing_options procedural:ConfigureProcedural
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                uint id : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
                float3 normal : NORMAL;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            StructuredBuffer<float4x4> _InstanceBuffer;
            StructuredBuffer<float3> _AnimBuffer;
            int _Frames;
            int _VertexCount;

            v2f vert (appdata v, uint instanceID: SV_InstanceID)
            {
                v2f vout;

                vout.uv = v.uv;

                // Normals
                //float3 norm = v.normal;
                //norm = mul(_InstanceBuffer[instanceID], norm);
                //float3 worldNormal = UnityObjectToWorldNormal(norm);
                //vout.normal = worldNormal;

                //// Lighting
                //half nl = saturate(dot(worldNormal, normalize(_WorldSpaceLightPos0.xyz)));
                //nl = clamp(nl, 0, 1);
                //vout.color = half4(nl, nl, nl, 1);

                // Animation
                int frame = frac(_Time.y + (instanceID * 0.347)) * _Frames;
                int i = v.id + frame * _VertexCount;
                float4 vertex = float4(_AnimBuffer[i], 1);

                // Vertex
                float4 pos = mul(_InstanceBuffer[instanceID], vertex);
                vout.vertex = UnityObjectToClipPos(pos.xyz);

                return vout;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
