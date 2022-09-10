using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//[ExecuteInEditMode]
public class Grass : MonoBehaviour
{

    [SerializeField, Range(10, 1000)]
    int resolution = 10;

    int numInstances;

    public GameObject plane;

    //public int mul;

    [SerializeField]
    ComputeShader computeShader;

    public Camera cam;

    //ComputeBuffer meshTriangles;
    //ComputeBuffer meshPositions;
    ComputeBuffer grassBladesBuffer;
    ComputeBuffer meshTriangles;
    //Maybe put all vertex info in one buffer contained in a struct?
    ComputeBuffer meshPositions;
    ComputeBuffer meshColors;

    ComputeBuffer argsBuffer;

    private const int ARGS_STRIDE = sizeof(int) * 4;

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

        vpMatrixID = Shader.PropertyToID("_VP_MATRIX");


    public Texture heightMap;
    public float jitterStrength;


    //Bounds planeBounds;

    float GrassSpacing;
    Vector3 PlaneCentre;
    [SerializeField]
    Material material;

    //[SerializeField]
    //Mesh mesh;

    public Mesh originalMesh;
    MeshFilter meshFilter;

    Mesh clonedMesh;

    Bounds bounds;
    void UpdateGPUParams()
    {
        //Debug.Log(plane.GetComponent<Renderer>().bounds.size);

        grassBladesBuffer.SetCounterValue(0);

        Bounds planeBounds = plane.GetComponent<Renderer>().bounds;

        Vector3 planeDims = planeBounds.size;

        float planeArea = planeDims.x * planeDims.z;
        ////Debug.Log(numInstances);


        //GrassSpacing = planeArea / numInstances;

        //GrassSpacing = Mathf.Sqrt(GrassSpacing);

        GrassSpacing = planeDims.x / resolution;

        PlaneCentre = new Vector3(planeDims.x / 2, 0, planeDims.z / 2);


        //NOTE: This doesnt need to be each frame. Do in Start instead
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(grassSpacingId, GrassSpacing);
        computeShader.SetFloat(jitterStrengthId, jitterStrength);
        computeShader.SetVector(planeCentreId, PlaneCentre);
        computeShader.SetTexture(0, heightMapId, heightMap);
        computeShader.SetBuffer(0, grassBladesBufferID, grassBladesBuffer);

        computeShader.SetFloat(heightId, _GrassBaseHeight);
        computeShader.SetFloat(heightRandId, _GrassHeightRandom);
        computeShader.SetFloat(widthId, _GrassBaseWidth);
        computeShader.SetFloat(widthRandId, _GrassWidthRandom);
        computeShader.SetFloat(tiltId, _GrassBaseTilt);
        computeShader.SetFloat(tiltRandId, _GrassTiltRandom);
        computeShader.SetFloat(bendId, _GrassBaseBend);
        computeShader.SetFloat(bendRandId, _GrassBendRandom);

        Matrix4x4 VP = cam.projectionMatrix * cam.worldToCameraMatrix;

        computeShader.SetMatrix(vpMatrixID, VP);


        int groups = Mathf.CeilToInt(resolution / 8f);
        computeShader.Dispatch(0, groups, groups, 1);


        ComputeBuffer.CopyCount(grassBladesBuffer, argsBuffer, sizeof(int));


        material.SetBuffer("_GrassBlades", grassBladesBuffer);

        //numInstances = grassBladesBuffer.count;
        //Debug.Log(grassBladesBuffer.count);
    }

    void Awake()
    {
        numInstances = resolution * resolution;
        //grassBladesBuffer = new ComputeBuffer(resolution * resolution, sizeof(float) * 10);
        grassBladesBuffer = new ComputeBuffer(resolution * resolution, sizeof(float) * 10, ComputeBufferType.Append);
        grassBladesBuffer.SetCounterValue(0);

    }

    // Start is called before the first frame update
    void Start()
    {

        clonedMesh = new Mesh(); //2

        clonedMesh.name = "clone";
        clonedMesh.vertices = originalMesh.vertices;
        clonedMesh.triangles = originalMesh.triangles;
        clonedMesh.normals = originalMesh.normals;
        clonedMesh.uv = originalMesh.uv;
        //clonedMesh.colors = originalMesh.colors;
        //3
        //mesh = grassBlade.GetComponent<Mesh>();

        Color[] newColors = new Color[originalMesh.colors.Length];

        for (int i = 0; i < originalMesh.colors.Length; i++)
        {
            Color col = originalMesh.colors[i];
            float r = Mathf.Pow(col.r, _VertexPlacementPower);

            col.r = r;

            newColors[i] = col;
            //Debug.Log(newColors[i]);
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

        material.SetBuffer("Triangles", meshTriangles);
        material.SetBuffer("Positions", meshPositions);
        material.SetBuffer("Colors", meshColors);

        bounds = new Bounds(Vector3.zero, Vector3.one * 1000f);


        argsBuffer = new ComputeBuffer(1, ARGS_STRIDE, ComputeBufferType.IndirectArguments);

        Debug.Log(numInstances);

        argsBuffer.SetData(new int[] { meshTriangles.count, 0, 0,0});



        //numInstances = resolution * resolution;
        UpdateGPUParams();








        //output = new Vector3[resolution * resolution];

        //positionsBuffer.GetData(output);



        //if (output.Length == 0)
        //{

        //    Debug.Log("emtpy");

        //}
        //else {

        //    Debug.Log("not empt");

        //}
        //int i = 0;
        //foreach (Vector3 v in output) {
        //    i++;
        //    Debug.Log(v + " " + i);
        //Transform[] instances = new Transform[numInstances];
        //for (int i = 0; i < numInstances; i++)
        //{
        //    Instantiate(prefab, output[i] * mul, Quaternion.identity);
        //}
        //}




        //Graphics.DrawProcedural(material, bounds, MeshTopology.Triangles, meshTriangles.count, numInstances);
        //Graphics.DrawMeshInstancedProcedural(
        //    mesh, 0, material, bounds, positionsBuffer.count
        //);
    }

    // Update is called once per frame
    void Update()
    {

        UpdateGPUParams();

        //Graphics.DrawProcedural(material, bounds, MeshTopology.Triangles, meshTriangles.count, numInstances);
        Graphics.DrawProceduralIndirect(material, bounds, MeshTopology.Triangles, argsBuffer, 
            0, null, null, UnityEngine.Rendering.ShadowCastingMode.Off, true, gameObject.layer);
    }

    void OnDestroy()
    {

        grassBladesBuffer.Dispose();
        meshTriangles.Dispose();
        meshPositions.Dispose();
        meshColors.Dispose();
        argsBuffer.Dispose();
    }
}
