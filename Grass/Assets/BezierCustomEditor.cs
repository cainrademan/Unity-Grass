using UnityEngine;
using System.Collections;
using UnityEditor;

// Creates a custom Label on the inspector for all the scripts named ScriptName
// Make sure you have a ScriptName script in your
// project, else this will not work.
[CustomEditor(typeof(BezierTest))]
public class BezierCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BezierTest bezierScript = (BezierTest)target;
        if (GUILayout.Button("Redistribute points"))
        {
            bezierScript.redistributePoints();
        }
    }
}