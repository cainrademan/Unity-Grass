using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class BezierTest : MonoBehaviour
{

    public Transform p0;
    public Transform p1;
    public Transform p2;
    public Transform p3;

    Vector3 pos0;
    Vector3 pos1;
    Vector3 pos2;
    Vector3 pos3;

    public GameObject grassBlade;
    Mesh mesh;
    Transform transform;

    public Material mat;

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log(mesh.colors);

        mesh = grassBlade.GetComponent<Mesh>();
        transform = grassBlade.GetComponent<Transform>();

        pos0 = new Vector3();
        pos1 = new Vector3();
        pos2 = new Vector3();
        pos3 = new Vector3();
        //foreach (Color col in mesh.colors) {

        //    Debug.Log(col);

        //}
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
