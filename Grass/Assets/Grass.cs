using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    ComputeBuffer positionsBuffer;
    ComputeBuffer meshTriangles;
    ComputeBuffer meshPositions;
    public GameObject prefab;

    static readonly int
        positionsId = Shader.PropertyToID("_Positions"),
        resolutionId = Shader.PropertyToID("_Resolution"),
        grassSpacingId = Shader.PropertyToID("_GrassSpacing"),
        planeCentreId = Shader.PropertyToID("_PlaneCentre"),
        jitterStrengthId = Shader.PropertyToID("_JitterStrength"),
        heightMapId = Shader.PropertyToID("HeightMap");


    public Texture heightMap;
    public float jitterStrength;


    //Bounds planeBounds;

    float GrassSpacing;
    Vector3 PlaneCentre;
    [SerializeField]
    Material material;

    [SerializeField]
    Mesh mesh;

    Vector3[] output;
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

        //Debug.Log(mul);
        //float step = 2f / resolution;
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(grassSpacingId, GrassSpacing);
        computeShader.SetFloat(jitterStrengthId, jitterStrength);
        computeShader.SetVector(planeCentreId, PlaneCentre);
        computeShader.SetTexture(0, heightMapId, heightMap);
        computeShader.SetBuffer(0, positionsId, positionsBuffer);

        int groups = Mathf.CeilToInt(resolution / 8f);
        computeShader.Dispatch(0, groups, groups, 1);

        material.SetBuffer("PositionsBuffer", positionsBuffer);

    }

    void Awake()
    {
        numInstances = resolution * resolution;
        positionsBuffer = new ComputeBuffer(resolution * resolution, 3 * 4);


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

        material.SetBuffer("Triangles", meshTriangles);
        material.SetBuffer("Positions", meshPositions);

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
        positionsBuffer.Dispose();
        meshTriangles.Dispose();
        meshPositions.Dispose();
    }
}
