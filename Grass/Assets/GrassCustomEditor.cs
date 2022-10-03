using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Grass))]
public class GrassCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Grass grassScript = (Grass)target;

        //GradientMapper grassScript = target as GradientMapper;
        //DrawDefaultInspector();
        if (grassScript.testing)
        {
            EditorGUILayout.HelpBox("Testing is active.", MessageType.Warning, true);
        }
        if (GUILayout.Button("Make Gradient Map"))
        {
            grassScript.testing = false;
            if (!AssetDatabase.IsValidFolder("Assets/GradientMaps"))
            {
                AssetDatabase.CreateFolder("Assets/", "GradientMaps");
                AssetDatabase.SaveAssets();
            }
            if (!Directory.Exists(Application.dataPath + "GradientMaps"))
            {
                Directory.CreateDirectory(Application.dataPath + "/GradientMaps/");
                GradientMapper.totalMaps = 0;
            }
            else
            {
                GradientMapper.totalMaps = Directory.GetFiles(Application.dataPath + "/GradientMaps/").Length;
            }

            byte[] bytes = grassScript.texture.EncodeToPNG();
            while (File.Exists(Application.dataPath + "/GradientMaps/gradient_map_" + GradientMapper.totalMaps.ToString() + ".png"))
            {
                GradientMapper.totalMaps++;
            }
            File.WriteAllBytes(Application.dataPath + "/GradientMaps/gradient_map_" + GradientMapper.totalMaps.ToString() + ".png", bytes);
            AssetDatabase.Refresh();

            Debug.Log("Gradient map saved!");
        }
    

    
        if (GUILayout.Button("Update parameters"))
        {
            grassScript.updateGrassArtistParameters();
        }
    }
}
