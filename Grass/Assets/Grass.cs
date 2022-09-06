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



    //ComputeBuffer meshTriangles;
    //ComputeBuffer meshPositions;
    ComputeBuffer grassBladesBuffer;
    ComputeBuffer meshTriangles;
    //Maybe put all vertex info in one buffer contained in a struct?
    ComputeBuffer meshPositions;
    ComputeBuffer meshColors;
    //public GameObject prefab;

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
        bendRandId = Shader.PropertyToID("_GrassBendRandom");


    public Texture heightMap;
    public float jitterStrength;


    //Bounds planeBounds;

    float GrassSpacing;
    Vector3 PlaneCentre;
    [SerializeField]
    Material material;

    [SerializeField]
    Mesh mesh;

    Bounds bounds;
    void UpdateGPUParams()
    {
        //Debug.Log(plane.GetComponent<Renderer>().bounds.size);

        

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

        int groups = Mathf.CeilToInt(resolution / 8f);
        computeShader.Dispatch(0, groups, groups, 1);

        material.SetBuffer("_GrassBlades", grassBladesBuffer);

    }

    void Awake()
    {
        numInstances = resolution * resolution;
        grassBladesBuffer = new ComputeBuffer(resolution * resolution, sizeof(float) * 10);


    }

    // Start is called before the first frame update
    void Start()
    {   
        //numInstances = resolution * resolution;
        UpdateGPUParams();

        //gpu buffers for the mesh
        int[] triangles = mesh.triangles;
        meshTriangles = new ComputeBuffer(triangles.Length, sizeof(int));
        meshTriangles.SetData(triangles);
        Vector3[] positions = mesh.vertices;
        meshPositions = new ComputeBuffer(positions.Length, sizeof(float) * 3);
        meshPositions.SetData(positions);

        Color[] colors = mesh.colors;
        meshColors = new ComputeBuffer(colors.Length, sizeof(float) * 4);
        meshColors.SetData(colors);

        material.SetBuffer("Triangles", meshTriangles);
        material.SetBuffer("Positions", meshPositions);
        material.SetBuffer("Colors", meshColors);

        bounds = new Bounds(Vector3.zero, Vector3.one * 1000f);







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

        Graphics.DrawProcedural(material, bounds, MeshTopology.Triangles, meshTriangles.count, numInstances);
    }

    void OnDestroy()
    {

        grassBladesBuffer.Dispose();
        meshTriangles.Dispose();
        meshPositions.Dispose();
        meshColors.Dispose();
    }
}
