using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

#if UNITY_EDITOR

namespace Quixel
{

    public class MegascansTerrainTools
    {
        /*
         (Verify that the pipeline is HDRP.) 
         * *******************************************************************************
         * ********************** Unity 2018.1 and 2018.2 ********************************
         * *******************************************************************************
         Terrain material blend setup steps:
         1. Get selected materials and make a terrain LayeredLit Material.
         2. Set Splat Prop types for the terrain.
         3. Set created materail to the terrain.
         4. Enable splat maps for the terrain.
         5. Set Layer Mask property of the terrain material to use Splat Alpha Map.
         6. Voila start painting!

         * *******************************************************************************
         * ********************** Unity 2018.3 onwards ***********************************
         * *******************************************************************************
         * Terrain material blend setup steps:
         1. Get selected materials and make Terrain Layers from them.
         2. Assign newly created terrain layers to terrain data.
         3. Voila start painting!

        */

        static int maxMaterialAllowed = 4;

        public static void SetupTerrain()
        {
            string materialName = EditorPrefs.GetString("QuixelDefaultMaterialName", "Terrain Material");
            string materialPath = EditorPrefs.GetString("QuixelDefaultMaterialPath", "Quixel/");
            string tilingNumber = EditorPrefs.GetString("QuixelDefaultTiling", "10");

            string[] versionParts = Application.unityVersion.Split('.');
            int majorVersion = int.Parse(versionParts[0]);

            if (majorVersion < 2018)
            {
                Debug.Log("This Unity version doesn't support this feature.");
                return;
            }
            else
            {
                float tiling;

                try
                {
                    tiling = float.Parse(tilingNumber);
                }
                catch (Exception ex)
                {
                    Debug.Log("Exception: " + ex.ToString());
                    tiling = 1f;
                }
                maxMaterialAllowed = MegascansUtilities.isLegacy() ? 4 : 8;

                List<Material> sourceMaterials = new List<Material>();
                Terrain terrain = null;

                try
                {
                    //Get currently highlighted materials in the project window.
                    sourceMaterials = GetSelectedMaterialsInProjectHierarchy();
                    //Verify two or more materails were seleted.
                    if (sourceMaterials.Count < 2 && MegascansUtilities.isLegacy())
                    {
                        Debug.Log("Not enough materials to create a blend material. Please select more materials.");
                        return;
                    }

                    //Get selected terrain in the scene view.
                    terrain = getCurrentlySelectedTerrain();
                    //Verify a terrain is selected.
                    if (terrain == null)
                    {
                        Debug.Log("No terrain selected. Please select a terrain.");
                        //return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log("Error getting select terrain/materials.");
                    Debug.Log(ex);
                }

                if (MegascansUtilities.isLegacy())
                {
                    try
                    {
                        //Set up the material here.
                        Material terrainMaterial = SetupTerrainMaterialDeprecated(sourceMaterials, materialPath, materialName, tiling);
                        //Get terrain data for splat maps.
                        TerrainData terrainData = terrain.terrainData;
                        //Get the textures from source materials to add to the painting.
                        if (terrainData)
                        {
#pragma warning disable
                            terrainData.splatPrototypes = getMaterialTexturesForSplatMap(sourceMaterials, tiling);
                        }
                        terrain.materialType = Terrain.MaterialType.Custom;
                        terrain.materialTemplate = terrainMaterial;
                        EnableSplatmaps(terrainData);
                        Texture2D alphaMap = terrainData.alphamapTextures[0];
                        terrainMaterial.SetTexture("_LayerMaskMap", alphaMap);
                        Debug.Log("Terrain blend material successfully created!");
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("Error Generating terrain blend material!");
                        Debug.Log(ex);
                    }
                }
                else
                {
#if UNITY_2018_3 || UNITY_2018_4 || UNITY_2019 || UNITY_2020 || UNITY_2021
                    //Create the material here.
                    //Material terrainMaterial = SetupTerrainMaterial(materialPath, materialName);
                    List<TerrainLayer> terrainLayers = new List<TerrainLayer>();

                    foreach (TerrainLayer tl in terrain.terrainData.terrainLayers)
                    {
                        terrainLayers.Add(tl);
                    }

                    // Unity's terrain system uses the second terrain layer as default material on terrain.
                    // To counter this we swap the position of the materials in our list.
                    if (sourceMaterials.Count > 1 && terrainLayers.Count == 0)
                    {
                        Material tempMat = sourceMaterials[0];
                        sourceMaterials[0] = sourceMaterials[1];
                        sourceMaterials[1] = tempMat;
                    }

                    foreach (Material mat in sourceMaterials)
                    {
                        if (terrainLayers.Count <= 8)
                        {
                            terrainLayers.Add(createTerrainLayer(mat, tiling));
                        }
                    }

                    if (terrainLayers.Count > 0)
                    {
                        terrain.terrainData.terrainLayers = terrainLayers.ToArray();
                    }
#endif
                }

            }

        }

        public static Material CreateMaterial(string mPath, string name, string shaderName)
        {
            string path = MegascansUtilities.FixPath(mPath);

            /// Unity doesn't allow you to create objects in directories which don't exist.
            /// So in this function, we create any and all necessary subdirectories that are required.
            /// We return the final subdirectory, which is used later in the asset creation too.

            //first, create the user specified path from the importer settings.
            string[] pathParts = MegascansUtilities.FixSlashes(path).Split('/');
            string defPath = "Assets";
            if (pathParts.Length > 0)
            {
                for (int i = 0; i < pathParts.Length; ++i)
                {
                    defPath = MegascansUtilities.ValidateFolderCreate(defPath, pathParts[i]);
                }
            }
            defPath = defPath + "/" + name + ".mat";
            Material terrainMaterial = new Material(Shader.Find(shaderName));
            AssetDatabase.CreateAsset(terrainMaterial, defPath);
            AssetDatabase.Refresh();
            return terrainMaterial;
        }

        //This method sets the right parameters depending upon the selected settings.

        public static Material SetupTerrainMaterial(string materialPath, string materialName)
        {
            Material terrainMaterial = CreateMaterial(materialPath, materialName, "HDRenderPipeline/TerrainLit");
            terrainMaterial.enableInstancing = true;
            return terrainMaterial;
        }

        public static Material SetupTerrainMaterialDeprecated(List<Material> sourceMaterials, string materialPath, string materialName, float tiling)
        {
            int numberOfLayers = Mathf.Clamp(sourceMaterials.Count + 1, 2, 4);
            Material terrainMaterial = CreateMaterial(materialPath, materialName, "HDRenderPipeline/LayeredLit");
            terrainMaterial.SetFloat("_LayerCount", numberOfLayers);
            terrainMaterial.EnableKeyword("_LAYEREDLIT_" + numberOfLayers.ToString() + "_LAYERS");

            numberOfLayers--;

            if (numberOfLayers > 3)
            {
                SetFloatsAndTexturesDeprecated(terrainMaterial, sourceMaterials[3], "0", tiling);
                SetFloatsAndTexturesDeprecated(terrainMaterial, sourceMaterials[0], "1", tiling);
                SetFloatsAndTexturesDeprecated(terrainMaterial, sourceMaterials[1], "2", tiling);
                SetFloatsAndTexturesDeprecated(terrainMaterial, sourceMaterials[2], "3", tiling);
            }
            else if (numberOfLayers > 2)
            {
                SetFloatsAndTexturesDeprecated(terrainMaterial, sourceMaterials[2], "0", tiling);
                SetFloatsAndTexturesDeprecated(terrainMaterial, sourceMaterials[0], "1", tiling);
                SetFloatsAndTexturesDeprecated(terrainMaterial, sourceMaterials[1], "2", tiling);
                SetFloatsAndTexturesDeprecated(terrainMaterial, sourceMaterials[2], "3", tiling);
            }
            else if (numberOfLayers > 1)
            {
                SetFloatsAndTexturesDeprecated(terrainMaterial, sourceMaterials[1], "0", tiling);
                SetFloatsAndTexturesDeprecated(terrainMaterial, sourceMaterials[0], "1", tiling);
                SetFloatsAndTexturesDeprecated(terrainMaterial, sourceMaterials[1], "2", tiling);
            }

            return terrainMaterial;
        }

        public static void SetFloatsAndTexturesDeprecated(Material terrainMaterial, Material sourceMaterial, string suffix, float tiling)
        {
            terrainMaterial.EnableKeyword("_MASKMAP" + suffix);
            terrainMaterial.EnableKeyword("_NORMALMAP" + suffix);
            terrainMaterial.EnableKeyword("_NORMALMAP_TANGENT_SPACE" + suffix);
            terrainMaterial.EnableKeyword("_MASKMAP" + suffix);
            terrainMaterial.SetTexture("_BaseColorMap" + suffix, sourceMaterial.mainTexture);
            terrainMaterial.SetTexture("_MaskMap" + suffix, sourceMaterial.GetTexture("_MaskMap"));
            terrainMaterial.SetTexture("_NormalMap" + suffix, sourceMaterial.GetTexture("_NormalMap"));
            terrainMaterial.SetFloat("_NormalMapSpace" + suffix, 0f);
            terrainMaterial.SetFloat("_Metallic" + suffix, 1f);
            terrainMaterial.SetTextureScale("_BaseColorMap" + suffix, new Vector2(tiling, tiling));
            terrainMaterial.SetTextureScale("_MaskMap" + suffix, new Vector2(tiling, tiling));
            terrainMaterial.SetTextureScale("_NormalMap" + suffix, new Vector2(tiling, tiling));
        }

        public static List<Material> GetSelectedMaterialsInProjectHierarchy()
        {
            List<Material> selectedMaterials = new List<Material>();
            List<string> selectedMaterialPaths = new List<string>();
            List<UnityEngine.Object> selection = new List<UnityEngine.Object>();

            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                if (obj.GetType() == typeof(Material) && selectedMaterials.Count <= maxMaterialAllowed)
                {
                    selectedMaterials.Add((Material)obj);
                }
                else
                {
                    selection.Add(obj);
                }
            }

            List<string> selectedFolders = MegascansUtilities.GetSelectedFolders(selection);
            foreach (string folder in selectedFolders)
            {
                selectedMaterialPaths = selectedMaterialPaths.Concat(MegascansUtilities.GetFiles(folder, ".mat")).ToList();
            }

            foreach (string mPath in selectedMaterialPaths)
            {
                Material mat = (Material)AssetDatabase.LoadAssetAtPath(mPath, typeof(Material));
                if (mat != null && selectedMaterials.Count <= maxMaterialAllowed) //4 is the max number of layers currently allowed in the HDRenderPipeline/LayeredLit shader.
                {
                    selectedMaterials.Add(mat);
                }
            }

            return selectedMaterials;
        }

        public static SplatPrototype[] getMaterialTexturesForSplatMap(List<Material> materials, float tiling)
        {
            SplatPrototype[] splatPropsTypes = new SplatPrototype[materials.Count];

            for (int i = 0; i < materials.Count; i++)
            {
                SplatPrototype prop = new SplatPrototype();
                prop.texture = (Texture2D)materials[i].mainTexture;
                prop.normalMap = (Texture2D)materials[i].GetTexture("_NormalMap");
                prop.tileSize = new Vector2(tiling, tiling);
                splatPropsTypes[i] = prop;
            }

            return splatPropsTypes;
        }

        public static Terrain getCurrentlySelectedTerrain()
        {
            foreach (UnityEngine.Object obj in Selection.objects)
            {
                if (obj.GetType() == typeof(GameObject))
                {
                    GameObject terrain = (GameObject)obj;
                    if (terrain.GetComponent<Terrain>())
                    {
                        return terrain.GetComponent<Terrain>();
                    }
                }
            }
            return null;
        }

        public static void EnableSplatmaps(TerrainData terrainData)
        {
            UnityEngine.Object[] data = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(terrainData));
            foreach (UnityEngine.Object o in data)
            {
                if (o is Texture2D)
                {
                    (o as Texture2D).hideFlags = HideFlags.None;
                    AssetDatabase.SaveAssets();
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(o));
                }
            }
        }
#if UNITY_2018_3 || UNITY_2018_4 || UNITY_2019 || UNITY_2020 || UNITY_2021
        public static TerrainLayer createTerrainLayer(Material mat, float tiling)
        {
            TerrainLayer terrainLayer = new TerrainLayer();
            string path = AssetDatabase.GetAssetPath(mat);
            path = path.Replace(".mat", ".terrainlayer");
            AssetDatabase.CreateAsset(terrainLayer, path);
            AssetDatabase.Refresh();
            terrainLayer.diffuseTexture = (Texture2D)mat.mainTexture;

            //attempt to auto-detect a settings file for Lightweight or HD pipelines
            switch (MegascansUtilities.getCurrentPipeline())
            {
                case Pipeline.HDRP:
                    terrainLayer.normalMapTexture = (Texture2D)mat.GetTexture("_NormalMap");
                    if (mat.GetFloat("_MaterialID") == 4)
                    {
                        terrainLayer.maskMapTexture = (Texture2D)mat.GetTexture("_SpecularColorMap");
                        terrainLayer.metallic = 1.0f;
                    }
                    else if (mat.GetFloat("_MaterialID") == 1)
                    {
                        terrainLayer.maskMapTexture = (Texture2D)mat.GetTexture("_MaskMap");
                        terrainLayer.specular = new Color(1.0f, 1.0f, 1.0f);
                    }
                    break;
                case Pipeline.LWRP:

                    terrainLayer.normalMapTexture = (Texture2D)mat.GetTexture("_BumpMap");
                    if (mat.GetFloat("_WorkflowMode") == 1)
                    {
                        terrainLayer.maskMapTexture = (Texture2D)mat.GetTexture("_MetallicGlossMap");
                        terrainLayer.specular = new Color(1.0f, 1.0f, 1.0f);
                    }
                    else
                    {
                        terrainLayer.maskMapTexture = (Texture2D)mat.GetTexture("_SpecGlossMap");
                        terrainLayer.metallic = 1.0f;
                    }
                    break;
                case Pipeline.Standard:
                    terrainLayer.normalMapTexture = (Texture2D)mat.GetTexture("_BumpMap");
                    if (mat.shader.ToString() == "Standard (Specular setup)")
                    {
                        terrainLayer.maskMapTexture = (Texture2D)mat.GetTexture("_SpecGlossMap");
                        terrainLayer.metallic = 1.0f;
                    }
                    else
                    {
                        terrainLayer.maskMapTexture = (Texture2D)mat.GetTexture("_MetallicGlossMap");
                        terrainLayer.specular = new Color(1.0f, 1.0f, 1.0f);
                    }
                    break;
            }

            terrainLayer.tileSize = new Vector2(tiling, tiling);
            return terrainLayer;
        }

        public static void CreateTerrainLayerFromMat()
        {
            try {
                Material selectedMat = MegascansUtilities.GetSelectedMaterial();
                if (!selectedMat)
                    return;

                Debug.Log("Here");
                createTerrainLayer(selectedMat, 1f);

                Debug.Log("Successfully created the terrain layer.");
            }
            catch (Exception ex)
            {
                Debug.Log("Exception::MegascansImageUtils::Flip Green Channel:: " + ex.ToString());
                MegascansUtilities.HideProgressBar();
            }
        }

#endif

    }

}

#endif
