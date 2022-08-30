using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public int resolution;
    public GameObject obj;
    public float mul;
    // Start is called before the first frame update
    void Start()
    {
        int numInstances = resolution * resolution; 
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                Instantiate(obj, new Vector3(i,0,j) * mul, Quaternion.identity);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
