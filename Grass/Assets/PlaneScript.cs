using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneScript : MonoBehaviour
{

    public Mesh mesh;
    // Start is called before the first frame update
    void Start()
    {

        

        MeshCollider meshc = gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;


        meshc.sharedMesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
