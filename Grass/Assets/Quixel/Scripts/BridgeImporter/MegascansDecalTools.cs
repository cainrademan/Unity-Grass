using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace Quixel
{

    public class MegascansDecalTools : MonoBehaviour
    {

        /*
             * Terrain material blend setup steps:
             1. Create a material with HDRenderPipeline/Decal shader.
             2. Create a new Rendering>Decal Projector object in scene.
             3. Assign that material to newly created decal projector.
             4. Assign textures to that material.
        */

        public static void SetupDecalProjector()
        {
            try
            {
#if !HDRP
                Debug.Log("HDRP features are disabled. You can enable them by going to Windows > Quixel > Enable HDRP Features");
                return;
#endif
#pragma warning disable
                string decalBlendStr = EditorPrefs.GetString("QuixelDefaultDecalBlend", "100");
                string decalSizeStr = EditorPrefs.GetString("QuixelDefaultDecalSize", "1");

                float decalBlend = 100f;
                try
                {
                    decalBlend = (0.01f * Mathf.Clamp(float.Parse(decalBlendStr), 0f, 100f));
                }
                catch (Exception ex)
                {
                    decalBlend = 100f;
                    Debug.Log("Exception: " + ex.ToString());
                }

                Material selectedMaterial = GetSelectedMaterial();
                if (selectedMaterial == null)
                {
                    Debug.Log("Error creating decal projector. No material selected.");
                    return;
                }

                float decalSize = 1f;
                try
                {
                    decalSize = float.Parse(decalSizeStr);
                }
                catch (Exception ex)
                {
                    Debug.Log("Exception: " + ex.ToString());
                    decalSize = 1f;
                }

                string path = AssetDatabase.GetAssetPath(selectedMaterial);
                Material decalMaterial = CreateDecalMaterial(path, selectedMaterial, decalBlend);
                CreateDecalPrefab(path, decalMaterial, decalSize);
                Debug.Log("Decal Projector created!");

            }
            catch (Exception ex)
            {
                Debug.Log("Error creating decal projector.");
                Debug.Log(ex);
            }
        }

        public static Material GetSelectedMaterial()
        {
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                if (obj.GetType() == typeof(Material))
                    return (Material)obj;
            }
            return null;
        }

        public static Material CreateDecalMaterial(string selectedMaterialPath, Material selectedMaterial, float decalBlend)
        {
            Material decalMaterial = new Material(Shader.Find("Standard"));
#if UNITY_2018_3 || UNITY_2018_4 || UNITY_2019
            decalMaterial.shader = Shader.Find("HDRP/Decal");
#else
            decalMaterial.shader = Shader.Find("HDRenderPipeline/Decal");
#endif
            string decalMaterialPath = selectedMaterialPath.Replace(".mat", "_Decal.mat");
            AssetDatabase.CreateAsset(decalMaterial, decalMaterialPath);
            AssetDatabase.Refresh();
            decalMaterial.enableInstancing = true;

            //Enable material keywords
            decalMaterial.EnableKeyword("_ALPHATEST_ON");
            decalMaterial.EnableKeyword("_ALBEDOCONTRIBUTION");
            decalMaterial.EnableKeyword("_COLORMAP");
            decalMaterial.EnableKeyword("_DISPLACEMENT_LOCK_TILING_SCALE");
            decalMaterial.EnableKeyword("_DOUBLESIDED_ON");
            decalMaterial.EnableKeyword("_MASKMAP");
            decalMaterial.EnableKeyword("_METALLICSPECGLOSSMAP");
            decalMaterial.EnableKeyword("_NORMALMAP");
            decalMaterial.EnableKeyword("_NORMALMAP_TANGENT_SPACE");
            decalMaterial.EnableKeyword("_PIXEL_DISPLACEMENT");
            decalMaterial.EnableKeyword("_PIXEL_DISPLACEMENT_LOCK_OBJECT_SCALE");
            //Set material textures
            decalMaterial.SetTexture("_BaseColorMap", selectedMaterial.mainTexture);
            decalMaterial.SetTexture("_MaskMap", selectedMaterial.GetTexture("_MaskMap"));
            decalMaterial.SetTexture("_NormalMap", selectedMaterial.GetTexture("_NormalMap"));
            //Set material keywords
            decalMaterial.SetFloat("_AlbedoMode", 1f);
            if (!MegascansUtilities.isLegacy())
            {
                decalMaterial.SetFloat("_MaskBlendSrc", 0f);
            }
            decalMaterial.SetFloat("_DecalBlend", decalBlend);

            return decalMaterial;
        }

        public static void CreateDecalPrefab(string materialPath, Material decalMaterial, float size)
        {
#if HDRP
            string assetPath = materialPath.Substring(0, materialPath.IndexOf("/Materials/"));
            string materialName = Path.GetFileName(materialPath);
            string prefabName = materialName.Replace(".mat", "");
            string prefabPath = MegascansUtilities.ValidateFolderCreate(assetPath, "Prefabs");
            GameObject g = new GameObject(prefabName);
#if UNITY_2019_4 || UNITY_2020 || UNITY_2021
            g.transform.rotation = Quaternion.Euler(45f, 45f, 45f);
#endif
#if UNITY_2019_3 || UNITY_2019_4 || UNITY_2020 || UNITY_2021
            g.AddComponent<UnityEngine.Rendering.HighDefinition.DecalProjector>();
            UnityEngine.Rendering.HighDefinition.DecalProjector decalProjector = g.GetComponent<UnityEngine.Rendering.HighDefinition.DecalProjector>();
#elif UNITY_2018_3 || UNITY_2018_4 || UNITY_2019_1 || UNITY_2019_2
            g.AddComponent<UnityEngine.Experimental.Rendering.HDPipeline.DecalProjectorComponent>();
            UnityEngine.Experimental.Rendering.HDPipeline.DecalProjectorComponent decalProjector = g.GetComponent<UnityEngine.Experimental.Rendering.HDPipeline.DecalProjectorComponent>();
#endif

#if UNITY_2018_3 || UNITY_2018_4 || UNITY_2019_1
            decalProjector.m_Material = decalMaterial;
            decalProjector.m_Size = new Vector3(size, size, size);
#else
            decalProjector.material = decalMaterial;
            decalProjector.size = new Vector3(size, size, size);
#endif
            string finalName = prefabPath + "/" + prefabName + "_Decal" + ".prefab";
            UnityEngine.Object pf = null;
            try
            {
                pf = AssetDatabase.LoadAssetAtPath(finalName, typeof(UnityEngine.Object));
            }
            catch (Exception ex)
            {
                Debug.Log("Error verifying prefab.");
                Debug.Log(ex);
            }
            if (!pf)
            {
                PrefabUtility.CreatePrefab(finalName, g);
            }
            else
            {
                PrefabUtility.ReplacePrefab(g, pf, ReplacePrefabOptions.ReplaceNameBased);
            }
            DestroyImmediate(g);
#endif
        }
    }
}
#endif