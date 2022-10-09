using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 50; i++) {

            float div = (float)i / 4;
            float m = i % 4;
            float m2 = Mathf.Repeat(i,4);

            Debug.Log("-------" + i  +"----------");
            Debug.Log(i + " div 4: " + div);
            Debug.Log(i +  " float remainder: " + m2);
            Debug.Log(i + " remainder: " + m);
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
