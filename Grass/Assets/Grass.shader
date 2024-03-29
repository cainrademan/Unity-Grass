Shader "Unlit/Grass"
{
    Properties
    {
        _ClumpColorBlend("_ClumpColorBlend", Range (0, 1)) = 1
         [Header(Albedo)]
        _GrassAlbedo("Grass albedo", 2D) = "white" {}
        _AlbedoScale("Albedo Scale", Float) = 0
        _AlbedoStrength("Albedo Strength", Float) = 0
        [Header(Gloss)]
        _GrassGloss("Grass gloss", 2D) = "white" {}
        _GlossScale("Gloss Scale", Float) = 0
        [Header(Shape)]
        _TaperAmount ("Taper Amount", Float) = 0
        _CurvedNormalAmount("Curved Normal Amount", Float) = 1
        _p1Flexibility ("p1Flexibility", Float) = 1
        _p2Flexibility ("p2Flexibility", Float) = 1
        [Header(Animation)]
        _WaveAmplitude("Wave Amplitude", Float) = 1
        _WaveSpeed("Wave Speed", Float) = 1
        _WavePower("Wave Power", Float) = 1
        _SinOffsetRange("Sin OffsetRange", Float) = 1
        _PushTipOscillationForward("_PushTipOscillationForward", Float) = 1
        [Header(Shading)]
        _Kspec("Specular Strength", Float) = 0
        _Kd("Diffuse Strength", Float) = 0
        _Kamb("Ambient Strength", Float) = 0
        _ShininessLower("Lower Shininess", Float) = 1
        _ShininessUpper("Upper Shininess", Float) = 1
        _SpecularLengthAtten("Specular Length Atten", Float) = 1
        _TipCol ("Tip Color", Color) = (.25, .5, .5, 1)
        _TipColLowerDist("_TipColLowerDist", Float) = 0.8
        _TipColUpperDist("_TipColUpperDist", Float) = 1
        _TopColor ("Top Color", Color) = (.25, .5, .5, 1)
        _BottomColor ("Bottom Color", Color) = (.25, .5, .5, 1)
        _LengthShadingStrength("Length shading multiplier", Float) = 1
        _LengthShadingBaseLuminance("Length shading offset", Float) = 1
        [Header(Distance shading)]
        _BlendSurfaceNormalDistLower("Start Distance (Blend Surface Normal)", Float) = 1
        _BlendSurfaceNormalDistUpper("End Distance (Blend Surface Normal)", Float) = 1
        _DistantDiff("Distant Diffuse Strength", Float) = 1
        _DistantSpec("Distant Specular Strength", Float) = 1
        _BottomColDistanceBrightness ("_BottomColDistanceBrightness", Float) = 1
        [Header(Test variables)]
        _Test("_Test", Float) = 0
        _Test2("_Test2", Float) = 0
        _Test3("_Test3", Float) = 0
        _Test4("_Test4", Float) = 0
        //_GradientMap ("Gradient map", 2D) = "white" {}
      

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

            #pragma multi_compile_local __ USE_CLUMP_COLORS

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
                float3 color;
                float windForce;
                float sideBend;
                float clumpColorDistanceFade;
                
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
                float4 vertexColor : TEXCOORD6;
            };
            float3 _WSpaceCameraPos;
            float _WindControl;

            sampler2D _MainTex;
            sampler2D _WindTex;
            sampler2D _GrassAlbedo;
            sampler2D _GrassGloss;
            //sampler2D _GradientMap;
            float _BottomColDistanceBrightness;
            float _TipColLowerDist;
            float _TipColUpperDist;
            float _SpecularLengthAtten;
            float _PushTipOscillationForward;
            float4 _MainTex_ST;
            float4 _WindTex_ST;
            float _TaperAmount;
            float _p1Flexibility;
            float _p2Flexibility;
            float _WaveAmplitude;
            float _WaveSpeed;
            float _WavePower;
            float _SinOffsetRange;
            float4 _TipCol;
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
            float _BlendSurfaceNormalDistUpper;
            float _CurvedNormalAmount;
            float _BlendSurfaceNormalDistLower;
            float _ClumpColorBlend;
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
                float4 vertColor = Colors[positionIndex];
                float2 uv = Uvs[positionIndex];
                //Get the t and side information from the vertex color
                float t = vertColor.r;
                float side = vertColor.g;
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

                //float3 p1;
                //float3 p2; 

                //p1.y =  0.33* p3.y;
                //p2.y =  0.66* p3.y;

                //p1.x =  0.33* p3.x;
                //p2.x =  0.66* p3.x;
                float3 p1 = 0.33* p3;
                float3 p2 = 0.66 * p3;

                p1 += bezCtrlOffsetDir * bend * _p1Flexibility;
                p2 += bezCtrlOffsetDir * bend * _p2Flexibility;


                float p1Weight = 0.33;
                float p2Weight = 0.66;
                float p3Weight = 1;

                //float sinOffset = -0.003;
                float hash= blade.hash;
                
                float windForce = blade.windForce;

                //windForce = saturate(  ((windForce - 0.5) * max(_WindTexContrast, 0)) + 0.5f   );

               

                _WaveAmplitude = lerp(0,_WaveAmplitude,_WindControl);
                _WaveSpeed = lerp(0,_WaveSpeed,_WindControl);

                float mult = 1-bend;

               // _WaveAmplitude = _WaveAmplitude;



                //float p1ffset = pow(p1Weight,_WavePower)*(_WaveAmplitude/100) * sin((_Time+hash*2*3.1415)*_WaveSpeed +p1Weight*2*3.1415*_SinOffsetRange); 
                float p2ffset =  pow(p2Weight,_WavePower)*(_WaveAmplitude/100) * sin((_Time+hash*2*3.1415)*_WaveSpeed +p2Weight*2*3.1415*_SinOffsetRange)*windForce; 
                float p3ffset =  pow(p3Weight,_WavePower)*(_WaveAmplitude/100) * sin((_Time+hash*2*3.1415)*_WaveSpeed +p3Weight*2*3.1415*_SinOffsetRange)*windForce; 


                p3ffset = (p3ffset) -  _PushTipOscillationForward*mult*(pow(p3Weight,_WavePower)*_WaveAmplitude/100)/2;


                ////_P0 += bezCtrlOffsetDir*  pOffset;
                //p1 += bezCtrlOffsetDir*  p1ffset;
                p2 += bezCtrlOffsetDir*  p2ffset;
                p3 += bezCtrlOffsetDir*  p3ffset;



                //Evaluate Bezier curve
                float3 newPos = cubicBezier(p0, p1,p2,p3, t);

                float3 midPoint = newPos;

                    //for normals, unneeded now
                float3 tangent = normalize(bezierTangent(p0, p1,p2,p3, t));
                float3 normal = normalize(cross(tangent, float3(0,0,1))) ;      
                

                float3 curvedNormal = normal;
                curvedNormal.z += side * pow(_CurvedNormalAmount,1);

                curvedNormal = normalize(curvedNormal);

                float width = (blade.width) * (1-_TaperAmount*t);
                newPos.z += side * width;               

                float angle = blade.rotAngle;

                float sideBend = blade.sideBend;



                float3x3 rotMat = AngleAxis3x3(-angle, float3(0,1,0));
                float3x3 sideRot = AngleAxis3x3(sideBend,normalize(tangent));

                newPos = newPos - midPoint;

                normal = mul(sideRot,normal);
                curvedNormal = mul(sideRot,curvedNormal);
                newPos = mul(sideRot,newPos);

                newPos = newPos + midPoint;


                normal = mul(rotMat,normal);
                curvedNormal = mul(rotMat,curvedNormal);
                newPos = mul(rotMat,newPos);
                

                newPos += blade.position;


                

                
                float3 surfaceNorm = blade.surfaceNorm;
                
                
                //o.windTest = testCol;
                o.uv = uv;
                o.worldPos = newPos;
                o.surfaceNorm = surfaceNorm;
                o.viewDir = normalize(_WSpaceCameraPos-newPos);
                o.curvedNorm = normalize(curvedNormal);
                o.originalNorm = normalize(normal);
                o.vertexColor = float4(vertColor.xyz, blade.clumpColorDistanceFade);
                o.color = fixed4(blade.color,1);
                //o.color = fixed4(windForce.xxx,1);
                o.vertex = mul(UNITY_MATRIX_VP, float4(newPos, 1));

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i, fixed facing : VFACE) : SV_Target
            {
                float3 curvedNorm = normalize(i.curvedNorm);

                float3 originalNorm = normalize(i.originalNorm);

                float3 n;

                if (facing > 0){
                
                    n = curvedNorm;

                }
                else {
                
                    n = -reflect(-curvedNorm,originalNorm);
                
                }

                float distToCam = distance(i.worldPos, _WSpaceCameraPos);

                float surfaceNormalBlendSmoothstep = smoothstep(_BlendSurfaceNormalDistLower,_BlendSurfaceNormalDistUpper, distToCam);

               n = lerp(n, normalize(i.surfaceNorm), surfaceNormalBlendSmoothstep);

                n= normalize(n);

                float3 l = normalize(_WorldSpaceLightPos0);

                float gloss = tex2D(_GrassGloss, i.uv*float2(_GlossScale, _GlossScale));

                float3 r = normalize(reflect(-l,n)) ;

                float3 v = normalize(i.viewDir);

                float ks = 1;

                float shininess = lerp(_ShininessLower, _ShininessUpper, gloss);

                _Kspec = lerp(_Kspec, _DistantSpec, surfaceNormalBlendSmoothstep);

                float spec = _Kspec* pow(saturate(dot(r,v)),shininess)   *      pow(i.vertexColor.r,_SpecularLengthAtten);

                _Kd = lerp(_Kd, _DistantDiff, surfaceNormalBlendSmoothstep);

                float diff =  _Kd * saturate(dot(n,l));

                float light =  _Kamb + 
                                diff 
                               + spec;
                
                 
                float lengthShading =  saturate(i.vertexColor.r * _LengthShadingStrength + _LengthShadingBaseLuminance);

                //float lengthShading = i.vertexColor.r;
                //lengthShading = saturate(  ((lengthShading - 0.5) * max(_LengthShadingStrength, 0)) + 0.5f   + _LengthShadingBaseLuminance );

                light *= lengthShading;

                float grassAlbedo =  (tex2D(_GrassAlbedo, i.uv*float2(_AlbedoScale, 1))     ) ;

                float noAlbedoMask = floor(grassAlbedo);

                grassAlbedo = saturate(grassAlbedo*_AlbedoStrength+noAlbedoMask);

                //fixed4 grassCol = tex2D(_GradientMap, float2(i.vertexColor.r, 0));

                float bottomColBrightness = lerp(1, _BottomColDistanceBrightness, surfaceNormalBlendSmoothstep);

                fixed4 grassCol = lerp(_BottomColor*bottomColBrightness,_TopColor, i.vertexColor.r);

                float sstep = smoothstep(_TipColLowerDist,_TipColUpperDist,i.vertexColor.r );

                grassCol  = lerp(grassCol,_TipCol, sstep);


                fixed4 col;

                #if USE_CLUMP_COLORS 
                    fixed4 clumpCol =  lerp(grassCol, i.color*grassCol, _ClumpColorBlend*i.vertexColor.w);
                    col =  light* grassAlbedo * clumpCol;

                #else
                    col =  light* grassAlbedo * grassCol;
                #endif

               
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
