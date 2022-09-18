Shader "Unlit/Grass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GrassAlbedo("Grass albedo", 2D) = "white" {}
        _AlbedoScale("_AlbedoScale", Float) = 0
        _GrassGloss("Grass gloss", 2D) = "white" {}
        _GlossScale("_GlossScale", Float) = 0
        _AlbedoStrength("_AlbedoStrength", Float) = 0
        _TaperAmount ("_TaperAmount", Float) = 0
        _p1Flexibility ("_p1Flexibility", Float) = 1
        _p2Flexibility ("_p2Flexibility", Float) = 1
        _WaveAmplitude("_WaveAmplitude", Float) = 1
        _WaveSpeed("_WaveSpeed", Float) = 1
        _WavePower("_WavePower", Float) = 1
        _SinOffsetRange("_SinOffsetRange", Float) = 1
        _Test("_Test", Float) = 0
        _Test2("_Test2", Float) = 0
        _Test3("_Test3", Float) = 0
        _Test4("_Test4", Float) = 0
        _Kspec("kspec", Float) = 0
        _Kd("kd", Float) = 0
        _Kamb("ka", Float) = 0
        _ShininessLower("_ShininessLower", Float) = 1
        _ShininessUpper("_ShininessUpper", Float) = 1
        _TopColor ("Top Color", Color) = (.25, .5, .5, 1)
        _BottomColor ("Bottom Color", Color) = (.25, .5, .5, 1)
        _AmbientLight("Ambient Light intensity", Float) = 1
        _LengthShadingStrength("_LengthShadingStrength", Float) = 1
        _LengthShadingBaseLuminance("_LengthShadingBaseLuminance", Float) = 1
      

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase"}
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
            #include "Lighting.cginc"

            struct GrassBlade {

                float3 position;
                //float2 facing;
                float rotAngle;
                float hash;
                float height;
                float width;
                float tilt;
                float bend;
                float3 surfaceNorm;
        };
    
        

            StructuredBuffer<GrassBlade> _GrassBlades;
            StructuredBuffer<int> Triangles;
            StructuredBuffer<float3> Positions;
            StructuredBuffer<float4> Colors;
            StructuredBuffer<float2> Uvs;

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                fixed4 color : COLOR0;
                float2 uv : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };
            float3 _WSpaceCameraPos;

            sampler2D _MainTex;
            sampler2D _GrassAlbedo;
            sampler2D _GrassGloss;
            float4 _MainTex_ST;
            float _TaperAmount;
            float _p1Flexibility;
            float _p2Flexibility;
            float _WaveAmplitude;
            float _WaveSpeed;
            float _WavePower;
            float _SinOffsetRange;
            float4 _TopColor;
            float4 _BottomColor;
            float _AmbientLight;
            float _Kamb;
            float _Kd;
            float _Kspec;
            float _ShininessLower;
            float _ShininessUpper;
            float _Test;
            float _Test3;
            float _Test4;
            float _AlbedoScale;
            float _GlossScale;
            float _AlbedoStrength;
            float _Test2;
            float _LengthShadingStrength;
            float _LengthShadingBaseLuminance;
         //   float3x3 AngleAxis3x3(float angle, float3 axis)
	        //{
		       // float c, s;
		       // sincos(angle, s, c);

		       // float t = 1 - c;
		       // float x = axis.x;
		       // float y = axis.y;
		       // float z = axis.z;

		       // return float3x3(
			      //  t * x * x + c, t * x * y - s * z, t * x * z + s * y,
			      //  t * x * y + s * z, t * y * y + c, t * y * z - s * x,
			      //  t * x * z - s * y, t * y * z + s * x, t * z * z + c
			      //  );
	        //}

            float2x2 rotate2d(float _angle){
                return float2x2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle) );
                }


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

            v2f vert (uint vertex_id: SV_VertexID, uint instance_id: SV_InstanceID)
            {
                v2f o;
                //
                //Extract vertex position and color from mesh
                //NOTE: I think we are going through each vertex multiple times by going over each triangle index in the mesh
                //Try go over individual vertices instead!
                int positionIndex = Triangles[vertex_id];
                float3 position = Positions[positionIndex];
                float4 color = Colors[positionIndex];
                float2 uv = Uvs[positionIndex];
                //Get the t and side information from the vertex color
                float t = color.r;
                float side = color.g;
                side = (side*2)-1;

                //Get the blade attribute data calculated in the compute shader
                GrassBlade blade = _GrassBlades[instance_id];

                //Calculate p0, p1, p2, p3 for the spline

                float3 p0 = float3(0,0,0);
                
                float height = blade.height;
                //float height = 5;
                float tilt = blade.tilt;
                float bend = blade.bend;
                float p3y =  tilt*height;
                float p3x = sqrt(height*height - p3y*p3y);
                float3 p3 = float3(-p3x,p3y,0);

                    //NOTE: Change this to more efficient. Work in only the x,y plane, ignore z
                float3 bladeDir = normalize(p3);
                float3 bezCtrlOffsetDir = normalize(cross(bladeDir, float3(0,0,1)));

                float3 p1 = 0.33* p3;
                float3 p2 = 0.66 * p3;

                p1 += bezCtrlOffsetDir * bend * _p1Flexibility;
                p2 += bezCtrlOffsetDir * bend * _p2Flexibility;


                //Animation
                float p1Weight = 0.33;
                float p2Weight = 0.66;
                float p3Weight = 1;

                //float sinOffset = -0.003;
                float hash= blade.hash;
   
                float p1ffset = pow(p1Weight,_WavePower)* (_WaveAmplitude/100) * sin((_Time+hash*2*3.1415)*_WaveSpeed +p1Weight*2*3.1415*_SinOffsetRange); 
                float p2ffset = pow(p2Weight,_WavePower)* (_WaveAmplitude/100) * sin((_Time+hash*2*3.1415)*_WaveSpeed +p2Weight*2*3.1415*_SinOffsetRange); 
                float p3ffset = pow(p3Weight,_WavePower)* (_WaveAmplitude/100) * sin((_Time+hash*2*3.1415)*_WaveSpeed +p3Weight*2*3.1415*_SinOffsetRange); 

                ////_P0 += bezCtrlOffsetDir*  pOffset;
                p1 += bezCtrlOffsetDir*  p1ffset;
                p2 += bezCtrlOffsetDir*  p2ffset;
                p3 += bezCtrlOffsetDir*  p3ffset;


                //Evaluate Bezier curve
                float3 newPos = cubicBezier(p0, p1,p2,p3, t);

                    //for normals, unneeded now
                float3 tangent = normalize(bezierTangent(p0, p1,p2,p3, t));
                float3 normal = -normalize(cross(tangent, float3(0,0,1))) ;      
                
                normal.z += side * pow(_Test3/1000,_Test4);

                normal = normalize(normal);

                float width = (blade.width) * (1-_TaperAmount*t);
                newPos.z += side * width;

                float grassFacingAngle = blade.rotAngle;

                //float3x3 rotMat = AngleAxis3x3(grassFacingAngle, float3(0,1,0));

                float2x2 rotMat2 = rotate2d(grassFacingAngle);

                //normal = mul(rotMat,normal);
                normal.xz = mul(rotMat2,normal.xz);

                newPos.xz = mul(rotMat2,newPos.xz);
                //newPos = mul(rotMat,newPos);

                newPos += blade.position;


                

                
                float3 surfaceNorm = blade.surfaceNorm;

                float distToCam = distance(newPos, _WSpaceCameraPos);

                float surfaceNormalBlendSmoothstep = smoothstep(_Test,_Test2, distToCam);

                float3 finalNorm = lerp(normal, surfaceNorm, surfaceNormalBlendSmoothstep);

                o.uv = uv;
                o.viewDir = normalize(_WSpaceCameraPos-newPos);
                o.normal = normalize(finalNorm);
                o.color = color;
                o.vertex = mul(UNITY_MATRIX_VP, float4(newPos, 1));
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i, fixed facing : VFACE) : SV_Target
            {
                float3 n = -i.normal;

                n = facing > 0 ? n : -n;

                float3 l = normalize(_WorldSpaceLightPos0);

                float gloss = tex2D(_GrassGloss, i.uv*float2(_GlossScale, 1));



                float3 r = reflect(-l,n);
                float3 v = normalize(i.viewDir);

                float ks = 1;

                //float shininess = lerp(_ShininessLower, _ShininessUpper, gloss);

                //float spec = _Kspec* pow(saturate(dot(r,v)),_ShininessUpper);

                float3 H = normalize(v + l); // Half direction
                float NdotH = max(0, dot(n, H));
                float specular = pow(NdotH, _ShininessUpper) * _Kspec;


                float diff =  _Kd * saturate(dot(n,l));

                float light =  _Kamb + 
                                diff 
                               + specular;
                

                //light = light);

                 light = saturate(light);
                 
                float lengthShading =  (i.color.r * _LengthShadingStrength + _LengthShadingBaseLuminance);

                light *= lengthShading;

                light = saturate(light);

                float grassAlbedo =  (tex2D(_GrassAlbedo, i.uv*float2(_AlbedoScale, 1))     ) ;

                float noAlbedoMask = floor(grassAlbedo);

                


                grassAlbedo = saturate(grassAlbedo*_AlbedoStrength+noAlbedoMask);




                fixed4 col = lerp(_BottomColor,_TopColor, i.color.r) *light;



                col = fixed4(light.xxx,1);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
