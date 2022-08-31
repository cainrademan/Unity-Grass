using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierTest : MonoBehaviour
{

    public Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log(mesh.colors);

        foreach (Color col in mesh.colors) {

            Debug.Log(col);

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
