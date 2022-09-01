using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[ExecuteInEditMode]
public class BezierTest : MonoBehaviour
{

    public Transform p0;
    public Transform p1;
    public Transform p2;
    public Transform p3;

    public float _VertexPlacementPower;

    Vector3 pos0;
    Vector3 pos1;
    Vector3 pos2;
    Vector3 pos3;

    public GameObject grassBlade;
    //public Mesh mesh;
    Transform transform;

    public Material mat;

    public Mesh originalMesh;
    MeshFilter meshFilter;

    Mesh clonedMesh;
    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log(mesh.colors);

        //mesh = grassBlade.GetComponent<Mesh>();

        //MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
       // meshRenderer.sharedMaterial = mat;

        //MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

       // Mesh mesh = new Mesh();

        transform = grassBlade.GetComponent<Transform>();



        pos0 = new Vector3();
        pos1 = new Vector3();
        pos2 = new Vector3();
        pos3 = new Vector3();

        float height = 1;
        float width = 0.5f;



        //Vector3[] vertices = new Vector3[15]
        //{
        //    new Vector3(0, 0, 0),
        //    new Vector3(0, 0, 0),
        //    new Vector3(0, 0, 0),
        //    new Vector3(0, 0, 0),
        //    new Vector3(0, 0, 0),
        //    new Vector3(0, 0, 0),
        //    new Vector3(0, 0, 0),
        //    new Vector3(0, 0, 0),
        //    new Vector3(0, 0, 0),
        //    new Vector3(0, 0, 0),
        //    new Vector3(0, 0, 0),
        //    new Vector3(0, 0, 0),
        //    new Vector3(0, 0, 0),
        //    new Vector3(0, 0, 0),
        //    new Vector3(0, 0, 0),

        //};

        //mesh.vertices = vertices;

        //int[] tris = new int[39]
        //{
        //    0, 3, 1,
        //    0, 2, 3,
        //    2, 5, 3,
        //    2, 4, 5,
        //    4, 7, 5,
        //    4, 6, 7,
        //    6, 9, 7,
        //    6, 8, 9,
        //    8, 11, 9,
        //    8, 10, 11,
        //    10, 13, 11,
        //    10, 12, 13,
        //    12, 14, 13,
        //};

        //mesh.triangles = tris;


        meshFilter = grassBlade.GetComponent<MeshFilter>();
        //originalMesh = meshFilter.sharedMesh; //1
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
            Debug.Log(newColors[i]);
        }

        clonedMesh.colors = newColors;

        meshFilter.mesh = clonedMesh;

    }

    public void redistributePoints() {

        Color[] newColors = new Color[originalMesh.colors.Length];

        for (int i = 0; i < originalMesh.colors.Length; i++)
        {
            Color col = originalMesh.colors[i];
            float r = Mathf.Pow(col.r, _VertexPlacementPower);

            col.r = r;

            newColors[i] = col;
            Debug.Log(newColors[i]);
        }

        clonedMesh.colors = newColors;

        meshFilter.mesh = clonedMesh;

    }
    void OnApplicationQuit()
    {
        meshFilter.mesh = originalMesh;
    }
    // Update is called once per frame
    void Update()
    {

        pos0 = transform.InverseTransformPoint(p0.position);
        pos1 = transform.InverseTransformPoint(p1.position);
        pos2 = transform.InverseTransformPoint(p2.position);
        pos3 = transform.InverseTransformPoint(p3.position);

        mat.SetVector("_P0", pos0);
        mat.SetVector("_P1", pos1);
        mat.SetVector("_P2", pos2);
        mat.SetVector("_P3", pos3);
    }
}
