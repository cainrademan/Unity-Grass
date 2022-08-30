Shader "Unlit/Grass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"


            //StructuredBuffer<float3> _Positions;

            StructuredBuffer<float3> PositionsBuffer;
            StructuredBuffer<int> Triangles;
            StructuredBuffer<float3> Positions;

            //struct appdata
            //{
            //    float4 vertex : POSITION;
            //    float2 uv : TEXCOORD0;
            //};

            struct v2f
            {
                //float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (uint vertex_id: SV_VertexID, uint instance_id: SV_InstanceID)
            {
                v2f o;


                int positionIndex = Triangles[vertex_id];
                float3 position = Positions[positionIndex];
                //add sphere position
                position += PositionsBuffer[instance_id];
                //convert the vertex position from world space to clip space
                // mul(UNITY_MATRIX_VP, float4(position, 1));

                //position *= 10;

                o.vertex = mul(UNITY_MATRIX_VP, float4(position, 1));
                //o.vertex = UnityObjectToClipPos(_Positions[vertexID]);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 col = float4(1,0,0,1);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
