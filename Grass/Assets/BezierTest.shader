Shader "Unlit/BezierTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _P0 ("Point 0", Vector) = (0, 0, 0, 0)
        _P1 ("Point 1", Vector) = (0, 0, 0, 0)
        _P2 ("Point 2", Vector) = (0, 0, 0, 0)
        _P3 ("Point 3", Vector) = (0, 0, 0, 0)
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

            //float3 cubicBezier(float3 p0, float3 p1, float3 p2, float3 p3, float t ){
            //    float3 a = lerp(p0, p1, t);
            //    float3 b = lerp(p2, p3, t);
            //    float3 c = lerp(p1, p2, t);
            //    float3 d = lerp(p1, p2, t);


            //}

            v2f vert (appdata v)
            {
                float t = v.uv.y;


                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
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
