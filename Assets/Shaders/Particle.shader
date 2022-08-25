Shader "Unlit/InstancedParticle"
{
    Properties
    {
        _ColorMin("ColorMin", Color) = (0.25, 0, 0 ,1)
        _ColorMax("ColorMax", Color) = (1, 1, 1, 1)
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
                uint id : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            fixed4 _ColorMin;
            fixed4 _ColorMax;

            StructuredBuffer<float4x4> _InstanceBuffer;
            StructuredBuffer<float> _InstanceColor;

            v2f vert (appdata v, uint instanceID: SV_InstanceID)
            {
                v2f vout;

                float4 pos = mul(_InstanceBuffer[instanceID], v.vertex);
                vout.vertex = UnityObjectToClipPos(pos.xyz);
                vout.color = lerp(_ColorMin, _ColorMax, _InstanceColor[instanceID]);
                return vout;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
