Shader "Hidden/ClumpingVoronoi"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NumClumps("_NumClumps", Float) = 2
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };



            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _NumClumps;
            float2 N22(float2 p){
            
                float3 a = frac(p.xyx*float3(123.34,234.34,345.65));
                a += dot(a, a+34.45);
                return frac(float2(a.x*a.y,a.y*a.z));
            
            }

            float4 frag (v2f i) : SV_Target
            {


                float pointsMask = 0;

                float radius = 0.01;
                float falloff = 0.01;

                float minDist = 100000;

                float id = 12;


                float2 clumpCentre = float2(0,0);
                for (int j =1; j < 40; j++){
                    float2 jj = float2(j,j);
                    float2 p =  N22(jj);
                    
                    float d = distance(p, i.uv);


                    if (d<minDist){
                    
                        minDist = d;
                        id = fmod((int)j,(int)_NumClumps);

                        clumpCentre = p;
                    }

                }

                float3 col = float3(id,clumpCentre );

                return float4(col,1);
            }
            ENDCG
        }
    }
}
