Shader "Unlit/BezierTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _P0 ("Point 0", Vector) = (0, 0, 0, 0)
        _P1 ("Point 1", Vector) = (0, 0, 0, 0)
        _P2 ("Point 2", Vector) = (0, 0, 0, 0)
        _P3 ("Point 3", Vector) = (0, 0, 0, 0)
        _Width("Width", Range (0, 1)) = 1
        _TaperAmount("_TaperAmount", Range (0, 1)) = 1
        _WaveAmplitude("_WaveAmplitude", Range (0, 1)) = 1
        _WaveSpeed("_WaveSpeed", Float) = 1
        _WavePower("_WavePower", Float) = 1
        _SinOffsetRange("_SinOffsetRange", Range (0, 1)) = 1
        _LerpWaveGeneration("_LerpWaveGeneration", Range (0, 1)) = 1
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
                fixed4 color : COLOR0;
            };
            float _SinOffsetRange;
            float _LerpWaveGeneration;
            float3 _P0;
            float3 _P1;
            float3 _P2;
            float3 _P3;
            float _WaveAmplitude;
            float _Width;
            float _TaperAmount;
            float _WaveSpeed;
            float _WavePower;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            float3 cubicBezier(float3 p0, float3 p1, float3 p2, float3 p3, float t ){
                float3 a = lerp(p0, p1, t);
                float3 b = lerp(p2, p3, t);
                float3 c = lerp(p1, p2, t);
                float3 d = lerp(a, c, t);
                float3 e = lerp(c, b, t);
                return lerp(d,e,t); 
            }

            float3 bezierTangent(float3 p0, float3 p1, float3 p2, float3 p3, float t ){
            
                float omt = 1-t;
                float omt2 = omt*omt;
                float t2= t*t;

                float3 tangent = 
                    p0* (-omt2) +
                    p1 * (3 * omt2 - 2 *omt) +
                    p2 * (-3 * t2 + 2 * t) +
                    p3 * (t2);
                     
                return normalize(tangent);
            }
            //w(p) = sin(c1 � a(p))� cos(c3 � a(p)) (9)
//a(p) = pu �px +t +
//pi
//4
//|cos(c2 � pi �pz)|+epsilon

            //float addWindOffset(float3 pos){
            
            //    float a = 

            //}

            v2f vert (appdata_full v)
            {

                float t = v.color.r;
                float side = v.color.g;
                side = (side*2)-1;


                float3 bladeDir = normalize(_P3-float3(0,0,0));

                float3 bezCtrlOffsetDir = -normalize(cross(bladeDir, float3(0,0,1)));



                //Deprecated
                //float pOffset = pow(t,_WavePower)* (_WaveAmplitude/100) *sin(_Time*_WaveSpeed +t*2*3.1415); 
                //Current best
                //float pOffset = pow(t,_WavePower)* (_WaveAmplitude/100) *sin(_Time*_WaveSpeed +t*2*3.1415*_SinOffsetRange); 
                //float p2Offset = pow(t,_WavePower)* (_WaveAmplitude/100) *sin(_Time*_WaveSpeed +t*2*3.1415); 
                //float p3Offset = pow(t,_WavePower)* (_WaveAmplitude/100) *sin(_Time*_WaveSpeed +t*2*3.1415); 

                //Potential funkey method
                //float tPower = 1;

                //float p1AmplitudeModifier = lerp(pow(p1Weight,_WavePower),pow(t,tPower), _LerpWaveGeneration);
                //float p2AmplitudeModifier = lerp(pow(p2Weight,_WavePower),pow(t,tPower), _LerpWaveGeneration);
                //float p3AmplitudeModifier = lerp(pow(p3Weight,_WavePower),pow(t,tPower), _LerpWaveGeneration);

                ////float pOffset = pow(p0Weight,_WavePower)* (_WaveAmplitude/100) *sin(_Time*_WaveSpeed +p0Weight*2*3.1415*_SinOffsetRange); 
                //float p1ffset = p1AmplitudeModifier* (_WaveAmplitude/100) * sin(_Time*_WaveSpeed +p1Weight*2*3.1415*_SinOffsetRange); 
                //float p2ffset = p2AmplitudeModifier* (_WaveAmplitude/100) * sin(_Time*_WaveSpeed +p2Weight*2*3.1415*_SinOffsetRange); 
                //float p3ffset = p3AmplitudeModifier* (_WaveAmplitude/100) * sin(_Time*_WaveSpeed +p3Weight*2*3.1415*_SinOffsetRange); 

                ////_P0 += bezCtrlOffsetDir*  pOffset;
                //_P1 += bezCtrlOffsetDir*  p1ffset;
                //_P2 += bezCtrlOffsetDir*  p2ffset;
                //_P3 += bezCtrlOffsetDir*  p3ffset;

                //Current 2nd best
                //float pOffset = pow(t,_WavePower)* (_WaveAmplitude/100) *sin(_Time*_WaveSpeed +t*2*3.1415*_SinOffsetRange); 
                //_P0 += bezCtrlOffsetDir*  pOffset;
                //_P1 += bezCtrlOffsetDir*  pOffset;
                //_P2 += bezCtrlOffsetDir*  pOffset;
                //_P3 += bezCtrlOffsetDir*  pOffset;


                //Current best
                //float p0Weight = 0.25;
                float p1Weight = 0.33;
                float p2Weight = 0.66;
                float p3Weight = 1;

                //float sinOffset = -0.003;

   
                float p1ffset = pow(p1Weight,_WavePower)* (_WaveAmplitude/100) * sin(_Time*_WaveSpeed +p1Weight*2*3.1415*_SinOffsetRange); 
                float p2ffset = pow(p2Weight,_WavePower)* (_WaveAmplitude/100) * sin(_Time*_WaveSpeed +p2Weight*2*3.1415*_SinOffsetRange); 
                float p3ffset = pow(p3Weight,_WavePower)* (_WaveAmplitude/100) * sin(_Time*_WaveSpeed +p3Weight*2*3.1415*_SinOffsetRange); 

                ////_P0 += bezCtrlOffsetDir*  pOffset;
                _P1 += bezCtrlOffsetDir*  p1ffset;
                _P2 += bezCtrlOffsetDir*  p2ffset;
                _P3 += bezCtrlOffsetDir*  p3ffset;
                


                float3 newPos = cubicBezier(_P0, _P1,_P2,_P3, t);

                float3 tangent = normalize(bezierTangent(_P0, _P1,_P2,_P3, t));


                float3 normal = -normalize(cross(tangent, float3(0,0,1)));               

                //Larger negative value means deeper bend
                //float sinOffset = -1/3000;
                

                

                //_P3 += bezCtrlOffsetDir*((_WaveAmplitude) *sin(_Time*_WaveSpeed) -0.01/30); 

                //newPos = cubicBezier(_P0, _P1,_P2,_P3, t);

                

                
                //v.vertex.z = 0;

                //v.color = pow(v.color,2.23);

                float width = (_Width/20) * (1-_TaperAmount*t);
                newPos.z += side * width;

                //-----Not using control points
                //float3 waveOffsetVec = normal * (_WaveAmplitude/10) * (sin(_Time * _WaveSpeed) + sin(2*_Time * _WaveSpeed)) *pow(t,_WavePower);

                //float3 waveOffsetVec = normal * (_WaveAmplitude/10) * (sin(_Time * _WaveSpeed)*cos(2*_Time * _WaveSpeed)) *pow(t,_WavePower);

                //float3 waveOffsetVec = normal * (_WaveAmplitude/40) * (sin(_Time * _WaveSpeed + t*2*3.1415)) *pow(t,_WavePower) + 90/100 ;

                
                //float3 waveOffsetVec = normal * (_WaveAmplitude/10) * (sin(_Time * _WaveSpeed)*cos(2*_Time * _WaveSpeed)) *pow(t,_WavePower);
                //------------


                v.vertex.xyz = newPos;

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 col = i.color;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
