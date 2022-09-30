using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Grass))]
public class GrassCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Grass grassScript = (Grass)target;
        if (GUILayout.Button("Update parameters"))
        {
            grassScript.updateGrassArtistParameters();
        }
    }
}
