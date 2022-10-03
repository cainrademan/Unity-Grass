using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//[ExecuteInEditMode]
public class Grass : MonoBehaviour
{

    [SerializeField, Range(10, 2000)]
    int resolution = 10;

    int numInstances;

    public GameObject plane;

    [SerializeField]
    ComputeShader computeShader;

    public Camera cam;

    ComputeBuffer grassBladesBuffer;
    ComputeBuffer meshTriangles;
    //Maybe put all vertex info in one buffer contained in a struct?
    ComputeBuffer meshPositions;
    ComputeBuffer meshColors;
    ComputeBuffer meshUvs;

    ComputeBuffer argsBuffer;

    ComputeBuffer clumpParametersBuffer;

    private const int ARGS_STRIDE = sizeof(int) * 4;

    public Texture WindTex;
    [SerializeField, Range(0, 1)]
    public float _WindControl;
    public float _BigWindSpeed;
    public float _BigWindScale;
    public float _BigWindRotateAmount;
    public float _SmallWindSpeed;    
    public float _SmallWindScale;
    public float _SmallWindRotateAmount;

    //public GameObject prefab;
    public float _VertexPlacementPower;
    public float _GrassBaseHeight;
    public float _GrassHeightRandom;
    public float _GrassBaseWidth;
    public float _GrassWidthRandom ;
    public float _GrassBaseTilt;
    public float _GrassTiltRandom;
    public float _GrassBaseBend;
    public float _GrassBendRandom;
    public float _FrustumCullNearOffset;

    public float _DistanceCullStartDist;
    public float _DistanceCullEndDist;
    public float _DistanceCullC;
    public float _DistanceCullM;
    public float _DistanceCullMinimumGrassAmount;

    public bool DISTANCE_CULL_ENABLED;

    public float _Test;
    public float _Test2;

    public int clumpTexHeight;
    public int clumpTexWidth;
    public Material clumpingVoronoiMat;


    static readonly int
        grassBladesBufferID = Shader.PropertyToID("_GrassBlades"),
        resolutionId = Shader.PropertyToID("_Resolution"),
        grassSpacingId = Shader.PropertyToID("_GrassSpacing"),
        planeCentreId = Shader.PropertyToID("_PlaneCentre"),
        jitterStrengthId = Shader.PropertyToID("_JitterStrength"),
        heightMapId = Shader.PropertyToID("HeightMap"),

        heightId = Shader.PropertyToID("_GrassBaseHeight"),
        heightRandId = Shader.PropertyToID("_GrassHeightRandom"),
        widthId = Shader.PropertyToID("_GrassBaseWidth"),
        widthRandId = Shader.PropertyToID("_GrassWidthRandom"),
        tiltId = Shader.PropertyToID("_GrassBaseTilt"),
        tiltRandId = Shader.PropertyToID("_GrassTiltRandom"),
        bendId = Shader.PropertyToID("_GrassBaseBend"),
        bendRandId = Shader.PropertyToID("_GrassBendRandom"),

        distanceCullStartDistId = Shader.PropertyToID("_DistanceCullStartDist"),
        distanceCullEndDistId = Shader.PropertyToID("_DistanceCullEndDist"),
        distanceCullCId = Shader.PropertyToID("_DistanceCullC"),
        distanceCullMId = Shader.PropertyToID("_DistanceCullM"),


        worldSpaceCameraPositionId = Shader.PropertyToID("_WSpaceCameraPos"),

        clumpParametersId = Shader.PropertyToID("_ClumpParameters"),
        clumpScaleId = Shader.PropertyToID("_ClumpScale"),

        windTexID = Shader.PropertyToID("WindTex"),
        clumpTexID = Shader.PropertyToID("ClumpTex"),
        ClumpGradientMapId = Shader.PropertyToID("ClumpGradientMap"),
        vpMatrixID = Shader.PropertyToID("_VP_MATRIX");


    public Texture heightMap;
    public float jitterStrength;

    float GrassSpacing;
    Vector3 PlaneCentre;
    [SerializeField]
    Material material;

    public Mesh originalMesh;
    MeshFilter meshFilter;

    Mesh clonedMesh;

    [Serializable]
    public struct ClumpParametersStruct
    {
        //Base height : float
        //Height random : float
        //Base width : float
        //Width random : float
        //Base tilt : float
        //Tilt random : float
        //Base bend : float
        //Bend random : float
        //public Color clumpColor;
        public float pullToCentre;
        public float pointInSameDirection;
        public float baseHeight;
        public float heightRandom;
        public float baseWidth;
        public float widthRandom;
        public float baseTilt;
        public float tiltRandom;
        public float baseBend;
        public float bendRandom;

    };
    //--------
    [Header("Gradient map parameters")]
    public Vector2Int gradientMapDimensions = new Vector2Int(128, 32);
    public Gradient gradient;

    [Header("Enable testing")]
    public bool testing = false;

    //private SpriteRenderer spriteRenderer;
    //public Material material;

    public Texture2D texture;

    public static int totalMaps = 0;
    //---------
    public float ClumpScale;

    public List<ClumpParametersStruct> clumpParameters;
    ClumpParametersStruct[] clumpParametersArray;

    Bounds bounds;
    void UpdateGPUParams()
    {

        grassBladesBuffer.SetCounterValue(0);


        
        computeShader.SetFloat(heightId, _GrassBaseHeight);
        computeShader.SetFloat(heightRandId, _GrassHeightRandom);
        computeShader.SetFloat(widthId, _GrassBaseWidth);
        computeShader.SetFloat(widthRandId, _GrassWidthRandom);
        computeShader.SetFloat(tiltId, _GrassBaseTilt);
        computeShader.SetFloat(tiltRandId, _GrassTiltRandom);
        computeShader.SetFloat(bendId, _GrassBaseBend);
        computeShader.SetFloat(bendRandId, _GrassBendRandom);

        computeShader.SetFloat(distanceCullStartDistId, _DistanceCullStartDist);
        computeShader.SetFloat(distanceCullEndDistId, _DistanceCullEndDist);
        computeShader.SetFloat(distanceCullCId, _DistanceCullC);
        computeShader.SetFloat(distanceCullMId, _DistanceCullM);

        computeShader.SetVector(worldSpaceCameraPositionId, cam.transform.position);

        computeShader.SetFloat("_Test", _Test);
        computeShader.SetFloat("_Test2", _Test2);
        computeShader.SetFloat("_DistanceCullMinimumGrassAmount", _DistanceCullMinimumGrassAmount);


        computeShader.SetVector("_Time", Shader.GetGlobalVector("_Time"));

        computeShader.SetFloat("_BigWindSpeed", _BigWindSpeed);
        computeShader.SetFloat("_BigWindScale", _BigWindScale);
        computeShader.SetFloat("_BigWindRotateAmount", _BigWindRotateAmount);
        computeShader.SetFloat("_SmallWindSpeed", _SmallWindSpeed);
        computeShader.SetFloat("_SmallWindScale", _SmallWindScale);
        computeShader.SetFloat("_SmallWindRotateAmount", _SmallWindRotateAmount);
        computeShader.SetFloat("_WindControl", _WindControl);

        Matrix4x4 projMat = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);

        Matrix4x4 VP = projMat * cam.worldToCameraMatrix;

        computeShader.SetMatrix(vpMatrixID, VP);


        int groups = Mathf.CeilToInt(resolution / 8f);
        computeShader.Dispatch(0, groups, groups, 1);

        computeShader.SetFloat("_ClumpScale", ClumpScale);

        ComputeBuffer.CopyCount(grassBladesBuffer, argsBuffer, sizeof(int));


        material.SetBuffer("_GrassBlades", grassBladesBuffer);
        material.SetVector("_WSpaceCameraPos", cam.transform.position);
        material.SetFloat("_WindControl",_WindControl);

    }

    void Awake()
    {
        numInstances = resolution * resolution;
        grassBladesBuffer = new ComputeBuffer(resolution * resolution, sizeof(float) * 16, ComputeBufferType.Append);
        grassBladesBuffer.SetCounterValue(0);

    }

    public void updateGrassArtistParameters() {

        //clumpParametersBuffer.Dispose();

        Debug.Log(clumpParameters.Count);

        clumpParametersArray = new ClumpParametersStruct[clumpParameters.Count];

        for (int i = 0; i < clumpParameters.Count; i++)
        {

            clumpParametersArray[i] = clumpParameters[i];


        }       
        clumpParametersBuffer.SetData(clumpParametersArray);
        computeShader.SetBuffer(0, clumpParametersId, clumpParametersBuffer);

    }

    // Start is called before the first frame update
    void Start()
    {
        if (DISTANCE_CULL_ENABLED)
        {

           computeShader.EnableKeyword("DISTANCE_CULL_ENABLED");

        }
        else
        {
            computeShader.DisableKeyword("DISTANCE_CULL_ENABLED");
            
        }

        clumpingVoronoiMat.SetFloat("_NumClumps", clumpParameters.Count);

        RenderTexture startTex = RenderTexture.GetTemporary(clumpTexWidth, clumpTexHeight, 0, RenderTextureFormat.ARGB32);
        RenderTexture clumpVoronoiTex = RenderTexture.GetTemporary(clumpTexWidth, clumpTexHeight, 0, RenderTextureFormat.ARGB32)   ;
        Graphics.Blit(startTex, clumpVoronoiTex, clumpingVoronoiMat, 0);


        //Graphics.Blit(source,destination, clumpingVoronoiMat);


        RenderTexture.active = clumpVoronoiTex;
        Texture2D clumpTex = new Texture2D(clumpTexWidth, clumpTexHeight, TextureFormat.ARGB32, false);
        clumpTex.ReadPixels(new Rect(0, 0, clumpTexWidth, clumpTexHeight), 0, 0);
        clumpTex.Apply();
        RenderTexture.active = null;

        computeShader.SetTexture(0, clumpTexID, clumpTex);

        RenderTexture.ReleaseTemporary(startTex);
        RenderTexture.ReleaseTemporary(clumpVoronoiTex);

        //var keywordSpace = computeShader.keywordSpace;

        //foreach (var localKeyword in keywordSpace.keywords)
        //{
        //    // Get the current state of the local keyword
        //    bool state = computeShader.IsKeywordEnabled(localKeyword);
        //    Debug.Log(localKeyword);
        //    Debug.Log(state);
        //    // Toggle the state
        //    //computeShader.SetKeyword(localKeyword, !state);
        //}



        clonedMesh = new Mesh(); //2

        clonedMesh.name = "clone";
        clonedMesh.vertices = originalMesh.vertices;
        clonedMesh.triangles = originalMesh.triangles;
        clonedMesh.normals = originalMesh.normals;
        clonedMesh.uv = originalMesh.uv;

        Color[] newColors = new Color[originalMesh.colors.Length];

        for (int i = 0; i < originalMesh.colors.Length; i++)
        {
            Color col = originalMesh.colors[i];
            float r = Mathf.Pow(col.r, _VertexPlacementPower);

            col.r = r;

            newColors[i] = col;
        }

        clonedMesh.colors = newColors;


        //gpu buffers for the mesh
        int[] triangles = clonedMesh.triangles;
        meshTriangles = new ComputeBuffer(triangles.Length, sizeof(int));
        meshTriangles.SetData(triangles);
        //It doesnt actually matter what the vertices are. This can be removed in theory
        Vector3[] positions = clonedMesh.vertices;
        meshPositions = new ComputeBuffer(positions.Length, sizeof(float) * 3);
        meshPositions.SetData(positions);

        Color[] colors = clonedMesh.colors;
        meshColors = new ComputeBuffer(colors.Length, sizeof(float) * 4);
        meshColors.SetData(colors);

        Vector2[] uvs = clonedMesh.uv;
        meshUvs = new ComputeBuffer(uvs.Length, sizeof(float) * 2);
        meshUvs.SetData(uvs);


        material.SetBuffer("Triangles", meshTriangles);
        material.SetBuffer("Positions", meshPositions);
        material.SetBuffer("Colors", meshColors);
        material.SetBuffer("Uvs", meshUvs);

        bounds = new Bounds(Vector3.zero, Vector3.one * 1000f);


        argsBuffer = new ComputeBuffer(1, ARGS_STRIDE, ComputeBufferType.IndirectArguments);

        Debug.Log(numInstances);

        argsBuffer.SetData(new int[] { meshTriangles.count, 0, 0,0});


    

        Bounds planeBounds = plane.GetComponent<Renderer>().bounds;

        Vector3 planeDims = planeBounds.size;

        float planeArea = planeDims.x * planeDims.z;


        GrassSpacing = planeDims.x / resolution;

        PlaneCentre = new Vector3(planeDims.x / 2, 0, planeDims.z / 2);

        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(grassSpacingId, GrassSpacing);
        computeShader.SetFloat(jitterStrengthId, jitterStrength);
        computeShader.SetVector(planeCentreId, PlaneCentre);
        computeShader.SetTexture(0, heightMapId, heightMap);
        computeShader.SetBuffer(0, grassBladesBufferID, grassBladesBuffer);

        computeShader.SetFloat("_FrustumCullNearOffset", _FrustumCullNearOffset);


        computeShader.SetTexture(0, windTexID, WindTex);

        computeShader.SetTexture(0, ClumpGradientMapId, texture);

        //Set Clump parameter buffer
        //ClumpParametersStruct[] parameters = new ClumpParametersStruct[2];

        //ClumpParametersStruct parm1 = new ClumpParametersStruct();
        //parm1.baseHeight = 1.38f;
        //parm1.heightRandom = 0.1f;
        //parm1.baseWidth = 0.03f;
        //parm1.widthRandom = 0.01f;
        //parm1.baseTilt = 0.85f;
        //parm1.tiltRandom = 0.07f;
        //parm1.baseBend = 0.18f;
        //parm1.bendRandom = 0.12f;

        //ClumpParametersStruct parm2 = new ClumpParametersStruct();
        //parm2.baseHeight = 2.38f;
        //parm2.heightRandom = 0.1f;
        //parm2.baseWidth = 0.12f;
        //parm2.widthRandom = 0.01f;
        //parm2.baseTilt = 0.85f;
        //parm2.tiltRandom = 0.07f;
        //parm2.baseBend = 0.18f;
        //parm2.bendRandom = 0.12f;

        //parameters[0] = parm1;
        //parameters[1] = parm2;
        //float baseHeight;
        //float heightRandom;
        //float baseWidth;
        //float widthRandom;
        //float baseTilt;
        //float tiltRandom;
        //float baseBend;
        //float bendRandom;
        //parameters[0] = new Clu

        //parameters[0] = new float[8] { 1.38f,0.1f,0.03f,0.01f,0.85f,0.07f,0.18f,0.12f};
        //parameters[1] = new float[8] { 2.38f, 0.1f, 0.12f, 0.01f, 0.85f, 0.07f, 0.18f, 0.12f };
        Debug.Log(clumpParameters.Count);
        clumpParametersBuffer = new ComputeBuffer(clumpParameters.Count, sizeof(float) * 10);
        computeShader.SetFloat("_NumClumpParameters", clumpParameters.Count);
        updateGrassArtistParameters();

        UpdateGPUParams();
    }

    // Update is called once per frame
    void Update()
    {
        if (testing)
        {
            texture = new Texture2D(gradientMapDimensions.x, gradientMapDimensions.y);
            texture.wrapMode = TextureWrapMode.Clamp;
            for (int x = 0; x < gradientMapDimensions.x; x++)
            {
                Color color = gradient.Evaluate((float)x / (float)gradientMapDimensions.x);
                for (int y = 0; y < gradientMapDimensions.y; y++)
                {
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply();
            computeShader.SetTexture(0, ClumpGradientMapId, texture);
            //if (material.HasProperty("_GradientMap"))
            //{
            //    material.SetTexture("_GradientMap", texture);
            //}
        }
    



    UpdateGPUParams();

        //Graphics.DrawProcedural(material, bounds, MeshTopology.Triangles, meshTriangles.count, numInstances);
        Graphics.DrawProceduralIndirect(material, bounds, MeshTopology.Triangles, argsBuffer, 
            0, null, null, UnityEngine.Rendering.ShadowCastingMode.Off, true, gameObject.layer);
    }

    void OnDestroy()
    {

        grassBladesBuffer.Dispose();
        clumpParametersBuffer.Dispose();
        meshTriangles.Dispose();
        meshPositions.Dispose();
        meshColors.Dispose();
        meshUvs.Dispose();
        argsBuffer.Dispose();
    }
}


