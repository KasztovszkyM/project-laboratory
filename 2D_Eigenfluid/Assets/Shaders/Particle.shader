Shader "Unlit/ParticleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            StructuredBuffer<float2> _ParticlePositions;
            int _nParticles;

            v2f vert(uint id : SV_VertexID)
            {
                v2f o;
                if (id < _nParticles)
                {
                    float2 particlePos = _ParticlePositions[id];
                    o.pos = UnityObjectToClipPos(float4(particlePos, 0.0, 1.0));
                }
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(1, 0.94f, 0.56f, 1);
            }
            ENDCG
        }
    }
}
