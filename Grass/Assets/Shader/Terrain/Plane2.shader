Shader "Custom/Plane2"
{
    Properties
    {
        _NearColor ("_NearColor", Color) = (1,1,1,1)
        _FarColor ("_FarColor", Color) = (1,1,1,1)
        _ColDistanceBlendStart ("_ColDistanceBlendStart", Float) = 1
        _ColDistanceBlendEnd ("_ColDistanceBlendEnd", Float) = 1
        _Brightness ("_Brightness", Float) = 1
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Heightmap ("Heightmap", 2D) = "white" {}
        _HeightMul ("Height multiplier", Float) = 1
        _Offset ("_Offset", Float) = 0.01
        _NormalMap ("Norm", 2D) = "white" {}

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf NoLighting vertex:vert addshadow 

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.5

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float2 worldUV;
            float3 norm;
            float3 customColor;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _NearColor;
        fixed4 _FarColor;

        float _ColDistanceBlendStart;
        float _ColDistanceBlendEnd;

        sampler2D _Heightmap;
        sampler2D _NormalMap;
        float4 _NormalMap_ST;
        float4 _Heightmap_ST;
        float _HeightMul;
        float _Brightness;
        float _Offset;



        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float4 getTransformedVertex(float4 v){
        
            float4 worldSpacePos = mul(unity_ObjectToWorld, v);

             float2 worldUV = worldSpacePos.xz;

             worldUV = worldUV * (1/_Heightmap_ST.xy) + _Heightmap_ST.zw;


             float height = tex2Dlod( _Heightmap, float4(worldUV.x, worldUV.y, 0,0)).r;

             float4 raisedVertexWS = worldSpacePos;

             raisedVertexWS.xyz += float3(0, 1, 0) * height * (_HeightMul);

             float4 raisedVertexOS = mul(unity_WorldToObject, raisedVertexWS);

             //v.xyz += float3(0, 1, 0) * height * (_HeightMul/1000);
//    return vertex;

             //raisedVertex.y += height*(_HeightMul/1000);

             return raisedVertexOS;
        
        }

//        float4 getVertex(float4 vertex)
//{

//        float3 worldSpacePos = mul(unity_ObjectToWorld, vertex);

//             float2 worldUV = worldSpacePos.xz;

//             worldUV = worldUV * (1/_Heightmap_ST.xy) + _Heightmap_ST.zw;
//    float3 normal = float3(0, 1, 0);
//    fixed height = tex2Dlod(_Heightmap, float4(worldUV, 0, 0)).r;
//    vertex.xyz += normal * height * (_HeightMul/1000);
//    return vertex;
//}

        void vert (inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            //float3 worldSpacePos = mul(unity_ObjectToWorld, v.vertex);

            // float2 worldUV = worldSpacePos.xz;

            // worldUV = worldUV * (1/_Heightmap_ST.xy) + _Heightmap_ST.zw;


            // UNITY_INITIALIZE_OUTPUT(Input,o);
            // o.worldUV = worldUV;

            float3 bitangent = float3(1, 0, 0);
            float3 tangent   = float3(0, 0, 1);

            //float offset = 0.01;

            float4 vertexBitangent = getTransformedVertex(v.vertex + float4(bitangent,0)*_Offset);
            float4 vertexTangent = getTransformedVertex(v.vertex + float4(tangent,0)*_Offset);
            float4 raisedVertex = getTransformedVertex(v.vertex);

            float3 newBitangent = (vertexBitangent-raisedVertex ).xyz;
            float3 newTangent = (vertexTangent-raisedVertex).xyz;

            float3 norm = cross(newTangent,newBitangent);
            
            float4 worldSpacePos = mul(unity_ObjectToWorld, raisedVertex);

            float d = distance(worldSpacePos, _WorldSpaceCameraPos);

            float colBlendVal = smoothstep(_ColDistanceBlendStart,_ColDistanceBlendEnd, d);



            float3 col = lerp(_NearColor,_FarColor, colBlendVal)*_Brightness;

             o.customColor = col;
             v.vertex = raisedVertex;
             v.normal = norm;
            // o.norm = norm;

    //        float3 bitangent = float3(1, 0, 0);
    //float3 tangent   = float3(0, 0, 1);
    //float offset = 0.01;
    //float4 vertexBitangent = getTransformedVertex(v.vertex + float4(bitangent * offset, 0));
    //float4 vertex          = getTransformedVertex(v.vertex);
    //float4 vertexTangent   = getTransformedVertex(v.vertex + float4(tangent   * offset, 0));
    //float3 newBitangent = (vertexBitangent - vertex).xyz;
    //float3 newTangent   = (vertexTangent   - vertex).xyz;
    //v.normal = cross(newTangent, newBitangent);
    //v.vertex.y = vertex.y;

             

      }

      fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
     {
         fixed4 c;
         c.rgb = s.Albedo; 
         c.a = s.Alpha;
         return c;
     }

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
           fixed4 c = tex2D (_MainTex, IN.uv_MainTex);

           //c = lerp(c, _Color, 0.5);

           //c= float4(float3(51, 26, 0)/255,1);

           fixed4 l = c.r * 0.3 + c.g * 0.59 + c.b * 0.11;

           o.Albedo = l*IN.customColor;
           //o.Albedo = float4(IN.uv_MainTex,0,1);
           //fixed4 c = tex2D (_Heightmap, IN.worldUV);
            //float3 norm = tex2D (_NormalMap, IN.worldUV);
            


           //o.Albedo = float4(IN.norm,1);
           //o.Normal = UnpackNormal(tex2D (_NormalMap, IN.uv_MainTex));
            //o.Albedo = height.xxx;
            // Metallic and smoothness come from slider variables
            //o.Metallic = _Metallic;
            //o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }

      

        ENDCG
    }
    FallBack "Diffuse"
}
