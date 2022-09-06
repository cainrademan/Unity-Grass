Shader "Unlit/Grass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TaperAmount ("_TaperAmount", Float) = 0
        _p1Flexibility ("_p1Flexibility", Float) = 1
        _p2Flexibility ("_p2Flexibility", Float) = 1
        _WaveAmplitude("_WaveAmplitude", Float) = 1
        _WaveSpeed("_WaveSpeed", Float) = 1
        _WavePower("_WavePower", Float) = 1
        _SinOffsetRange("_SinOffsetRange", Float) = 1
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

            struct GrassBlade {

                float3 position;
                float2 facing;
                float hash;
                float height;
                float width;
                float tilt;
                float bend;
        };
    
        

            StructuredBuffer<GrassBlade> _GrassBlades;
            StructuredBuffer<int> Triangles;
            StructuredBuffer<float3> Positions;
            StructuredBuffer<float4> Colors;


            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _TaperAmount;
            float _p1Flexibility;
            float _p2Flexibility;
            float _WaveAmplitude;
            float _WaveSpeed;
            float _WavePower;
            float _SinOffsetRange;

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
                //float3 tangent = normalize(bezierTangent(_P0, _P1,_P2,_P3, t));
                //float3 normal = -normalize(cross(tangent, float3(0,0,1)));      
                
                float width = (blade.width) * (1-_TaperAmount*t);
                newPos.z += side * width;


                //Rotate blade
                float2 grassFacing = blade.facing;

               //float2 grassFacing = normalize(float2(-1,1));
                float grassFacingAngle = atan2(grassFacing.y,grassFacing.x);


                //float rotAngle = acos(dot(grassFacing, float2(1,0)));

                float3x3 rotMat = AngleAxis3x3(grassFacingAngle, float3(0,1,0));




                newPos = mul(newPos,rotMat);
                newPos += blade.position;

                //position = mul(position,rotMat);
                //position += blade.position;
                //position += PositionsBuffer[instance_id];

                

                

                //convert the vertex position from world space to clip space
                // mul(UNITY_MATRIX_VP, float4(position, 1));

                //position *= 10;
                o.color = color;
                o.vertex = mul(UNITY_MATRIX_VP, float4(newPos, 1));
                //o.vertex = UnityObjectToClipPos(_Positions[vertexID]);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i, fixed facing : VFACE) : SV_Target
            {

                fixed4 col;

                //col = facing > 0 ? float4(1,0,0,1) : float4(0,1,0,1);


                col = i.color;

                // sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);
                //fixed4 col = float4(1,0,0,1);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
