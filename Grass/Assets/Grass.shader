Shader "Unlit/Grass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GrassAlbedo("Grass albedo", 2D) = "white" {}
        _AlbedoScale("_AlbedoScale", Float) = 0
        _GrassGloss("Grass gloss", 2D) = "white" {}
        //_WindTex("_WindTex", 2D) = "white" {}
        //_WindStrength("_WindStrength", Float) = 1
        //_BigWindSpeed("_BigWindSpeed", Float) = 1
        //_SmallWindSpeed("_SmallWindSpeed", Float) = 1
        //_WindRotateAmount("_WindRotateAmount", Float) = 1
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
        _DistantSpec("_DistantSpec", Float) = 1
        _DistantDiff("_DistantDiff", Float) = 1
      

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
                float3 curvedNorm : TEXCOORD0;
                fixed4 color : COLOR0;
                float2 uv : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                float3 originalNorm : TEXCOORD3;
                float3 worldPos : TEXCOORD4;
                float3 surfaceNorm : TEXCOORD5;
                float3 windTest : TEXCOORD6;
            };
            float3 _WSpaceCameraPos;

            sampler2D _MainTex;
            sampler2D _WindTex;
            sampler2D _GrassAlbedo;
            sampler2D _GrassGloss;
            float4 _MainTex_ST;
            float4 _WindTex_ST;
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
            float _DistantDiff;
            float _DistantSpec;
            float _WindStrength;
            float _SmallWindSpeed;
            float _BigWindSpeed;
            float _WindRotateAmount;

            float3x3 AngleAxis3x3(float angle, float3 axis)
	        {
		        float c, s;
		        sincos(angle, s, c);

		        float t = 1 - c;
		        float x = axis.x;
		        float y = axis.y;
		        float z = axis.z;

		        return float3x3(
			        t * x * x + c, t * x * y - s * z, t * x * z + s * y,
			        t * x * y + s * z, t * y * y + c, t * y * z - s * x,
			        t * x * z - s * y, t * y * z + s * x, t * z * z + c
			        );
	        }

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

            float2 slerp(float2 start, float2 end, float percent)
{
     // Dot product - the cosine of the angle between 2 vectors.
     float d = dot(start, end);     
     // Clamp it to be in the range of Acos()
     // This may be unnecessary, but floating point
     // precision can be a fickle mistress.
     d = clamp(d, -1.0, 1.0);
     // Acos(dot) returns the angle between start and end,
     // And multiplying that by percent returns the angle between
     // start and the final result.
     float theta = acos(d)*percent;
     float2 RelativeVec = normalize(end - start*d); // Orthonormal basis
     // The final result.
     return ((start*cos(theta)) + (RelativeVec*sin(theta)));
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
                //float2 grassFacing = blade.facing;

                //float2 worldUV = blade.position.xz;

                //float2 bigWindUV = worldUV * (_WindTex_ST.xx);

                //bigWindUV += _Time * float2(1,0) *_BigWindSpeed;

                //float bigWind = (tex2Dlod( _WindTex, float4(bigWindUV.x, bigWindUV.y, 0,0)).x   );

                //float bigTheta = ((bigWind*2)-1)* 3.14159;

                //float2 bigWindDir = (float2(cos(bigTheta), sin(bigTheta)));

                //float2 grassSideVec = normalize(float2(-grassFacing.y, grassFacing.x));

                //float rotateBladeFromBigWindAmount = dot(grassSideVec, bigWindDir); 

                //float bigWindRotateAngle = rotateBladeFromBigWindAmount * (3.14159/2) * _WindRotateAmount;

                //float2 smallWindUV = worldUV * (_WindTex_ST.yy);

                //smallWindUV += _Time * float2(1,0) *_SmallWindSpeed;

                //float smallWind = (tex2Dlod( _WindTex, float4(smallWindUV.x, smallWindUV.y, 0,0)).y   )*2-1;

                //float smallWindRotateAngle = (smallWind)* 3.14159 * _WindStrength;

                //float angle = atan2(grassFacing.y,grassFacing.x);

                //angle += bigWindRotateAngle + smallWindRotateAngle;

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
                float3 normal = normalize(cross(tangent, float3(0,0,1))) ;      
                

                //normal.z += side * pow(_Test3,_Test4);

                float3 curvedNormal = normal;
                curvedNormal.z += side * pow(_Test3,1);

                curvedNormal = normalize(curvedNormal);

                float width = (blade.width) * (1-_TaperAmount*t);
                newPos.z += side * width;

                //float grassFacingAngle = blade.rotAngle;

                

                             

                //o.windTest = float3(windDir,0);

                
                

                //windRotateAngle = bigTheta *_WindStrength;

                //float3 testCol;

                //if (rotateBladeFromWindAmount >= 0){
                
                //    testCol = float3(0,0,1) * rotateBladeFromWindAmount;
                
                //}
                //else {
                
                //    testCol = float3(1,0,0) * -rotateBladeFromWindAmount;
                
                //}


               
               

                //float2 combinedDir = normalize(lerp(grassFacing, windDir, _Test4));

                //float2 combinedDir = slerp(grassFacing, windDir, _Test4);

                //combinedDir = windDir;

                //grassFacing = windDir;                

                float angle = blade.rotAngle;

                float3x3 rotMat = AngleAxis3x3(angle, float3(0,1,0));

                //float3x3 windMat = AngleAxis3x3(windAngle*_WindStrength, float3(0,1,0));
                //float2x2 rotMat2 = rotate2d(grassFacingAngle);

                //normal = mul(mul(rotMat,normal), windMat);
                //curvedNormal = mul(mul(rotMat,curvedNormal), windMat);
                //newPos = mul(mul(rotMat,newPos), windMat);
                normal = mul(rotMat,normal);
                curvedNormal = mul(rotMat,curvedNormal);
                newPos = mul(rotMat,newPos);

                //normal = normalize(normal);

                //normal.xz = mul(rotMat2,normal.xz);

                //newPos.xz = mul(rotMat2,newPos.xz);
                

                newPos += blade.position;


                

                
                float3 surfaceNorm = blade.surfaceNorm;

                //float distToCam = distance(newPos, _WSpaceCameraPos);

                //float surfaceNormalBlendSmoothstep = smoothstep(_Test,_Test2, distToCam);

                //float3 finalNorm = lerp(normal, surfaceNorm, surfaceNormalBlendSmoothstep);
                
                
                //o.windTest = testCol;
                o.uv = uv;
                o.worldPos = newPos;
                o.surfaceNorm = surfaceNorm;
                o.viewDir = normalize(_WSpaceCameraPos-newPos);
                o.curvedNorm = normalize(curvedNormal);
                o.originalNorm = normalize(normal);
                o.color = color;
                o.vertex = mul(UNITY_MATRIX_VP, float4(newPos, 1));

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i, fixed facing : VFACE) : SV_Target
            {
                float3 curvedNorm = i.curvedNorm;

                float3 originalNorm = i.originalNorm;

                float3 n;

                if (facing > 0){
                
                    n = curvedNorm;

                }
                else {
                
                    n = -reflect(-curvedNorm,originalNorm);
                
                }

                float distToCam = distance(i.worldPos, _WSpaceCameraPos);

                float surfaceNormalBlendSmoothstep = smoothstep(_Test,_Test2, distToCam);

                n = lerp(n, i.surfaceNorm, surfaceNormalBlendSmoothstep);

                n= normalize(n);

                //n = facing > 0 ? n : float3(-n.x, -n.y, n.z);


                
                //n = facing > 0 ? n : -n;
                float3 l = normalize(_WorldSpaceLightPos0);

                float gloss = tex2D(_GrassGloss, i.uv*float2(_GlossScale, 1));


                //float reflectMask = round(saturate(dot(l,n)));


                float3 r = normalize(reflect(-l,n)) ;
                float3 v = normalize(i.viewDir);

                float ks = 1;

                float shininess = lerp(_ShininessLower, _ShininessUpper, gloss);

                _Kspec = lerp(_Kspec, _DistantSpec, surfaceNormalBlendSmoothstep);

                float spec = _Kspec* pow(saturate(dot(r,v)),shininess);

                //S


                //float spec = saturate(dot(r, l));

                //float3 H = normalize(v + l); // Half direction
                //float NdotH = max(0, dot(n, H));
                //float specular = pow(NdotH, _ShininessUpper) * _Kspec;

                _Kd = lerp(_Kd, _DistantDiff, surfaceNormalBlendSmoothstep);

                float diff =  _Kd * saturate(dot(n,l));

                float light =  _Kamb + 
                                diff 
                               + spec;
                

                //light = light);

                //light = saturate(light);
                 
                float lengthShading =  (i.color.r * _LengthShadingStrength + _LengthShadingBaseLuminance);

                light *= lengthShading;

                //light = saturate(light);

                float grassAlbedo =  (tex2D(_GrassAlbedo, i.uv*float2(_AlbedoScale, 1))     ) ;

                float noAlbedoMask = floor(grassAlbedo);

                


                grassAlbedo = saturate(grassAlbedo*_AlbedoStrength+noAlbedoMask);




                fixed4 col = lerp(_BottomColor,_TopColor, i.color.r) *light* grassAlbedo;

                //col = fixed4(i.windTest,1);

                //col = fixed4(shininess.xxx,1);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
