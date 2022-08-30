Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Heightmap ("Heightmap", 2D) = "white" {}
        _HeightMul ("Height multiplier", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _Heightmap;
            float4 _Heightmap_ST;
            float _HeightMul;

            v2f vert (appdata v)
            {

                float3 worldSpacePos = mul(unity_ObjectToWorld, v.vertex);

                float2 worldUV = worldSpacePos.xz;

                worldUV = worldUV * _Heightmap_ST.xy + _Heightmap_ST.zw;


                float height = tex2Dlod( _Heightmap, float4(worldUV.x, worldUV.y, 0,0));

                v2f o;

                float4 raisedVertex = v.vertex;

                raisedVertex.y += height*_HeightMul;

                o.vertex = UnityObjectToClipPos(raisedVertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
