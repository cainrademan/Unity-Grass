#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using Newtonsoft.Json.Linq;

namespace Quixel
{
    /// <summary>
    /// Completely rewritten importer with lots of commenting... Should be quite a bit faster than the previous one too.
    /// For those looking to customize, please be aware that some of the methods in here do not use built in Unity API calls.
    /// Ajwad Imran - Technical Artist @ Quixel.
    /// Lee Devonald - (Former) Technical Artist @ Quixel.
    /// </summary>
    public class MegascansImporter : Editor
    {
        private bool plant = false;
        private string assetName;
        private string type;
        private string mapName;
        private string folderNamingConvention;

        private string path;
        private string activeLOD;
        private int dispType;
        private int texPack;
        private int lodFadeMode;
        private int shaderType;
        private bool setupCollision = false;

        private bool applyToSelection;
        private bool addAssetToScene;
        private bool importAllTextures;

        private string texPath;
        private string matPath;

        private Material finalMat;
        private Material billboardMat;

        private bool highPoly = false;
        private bool isAlembic = false;
        private bool hasBillboardLOD = false;
        private bool hasBillboardLODOnly = false;

        /// <summary>
        /// Takes an imported JSON object, and breaks it into relevant components and data.
        /// Then calls relevant functions for actual import of asset.
        /// </summary>
        /// <param name="objectList"></param>
        public string ImportMegascansAssets(JObject objectList)
        {
            var startTime = System.DateTime.Now;
            activeLOD = (string)objectList["activeLOD"];
            string minLOD = (string)objectList["minLOD"];
            assetName = (string)objectList["name"];
            type = (string)objectList["type"];

            isAlembic = false;
            plant = false;
            highPoly = false;
            hasBillboardLOD = false;
            hasBillboardLODOnly = false;
            mapName = "";
            folderNamingConvention = (string)objectList["folderNamingConvention"];

            //get mesh components from the current object.
            JArray meshComps = (JArray)objectList["meshList"];

            //run a check to see if we're using Unity 5 or below, and then if we're trying to import a high poly mesh. if so, let the user know we are aborting the import.
            if (meshComps.Count > 0)
            {
                isAlembic = Path.GetExtension((string)meshComps[0]["path"]) == ".abc";
            }

            hasBillboardLOD = MegascansMeshUtils.ContainsLowestLOD((JArray)objectList["lodList"], minLOD, activeLOD);

            if (type.ToLower().Contains("3dplant"))
            {
                plant = true;
                if (minLOD == activeLOD)
                {
                    hasBillboardLODOnly = true;
                }
            }

            try
            {
                LoadPreferences();
                shaderType = MegascansUtilities.GetShaderType();
                MegascansUtilities.CalculateNumberOfOperations(objectList, dispType, texPack, shaderType, hasBillboardLODOnly);
                path = ConstructPath(objectList);

                if (path == null || path == "")
                {
                    Debug.Log("Asset: " + (string)objectList["name"] + " already exist in the Project. Please delete/rename the existing folder and re-import this asset.");
                    AssetDatabase.Refresh();
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Error setting import path.");
                Debug.Log(ex.ToString());
                MegascansUtilities.HideProgressBar();
            }

            try
            {
                //process textures
                ProcessTextures(objectList);
                if (finalMat == null && !(plant && hasBillboardLODOnly))
                {
                    Debug.Log("Could not import the textures and create the material.");
                    return null;
                }
                else
                {
                    if (type.ToLower().Contains("surface") && applyToSelection)
                    {
                        foreach (MeshRenderer render in MegascansUtilities.GetSelectedMeshRenderers())
                        {
                            render.material = finalMat;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Error importing textures.");
                Debug.Log(ex.ToString());
                MegascansUtilities.HideProgressBar();
            }

            //process meshes
            if (meshComps == null && !type.Contains("surface"))
            {
                Debug.LogError("No meshes found. Please double check your export settings.");
                Debug.Log("Import failed.");
                return null;
            }

            if (meshComps.Count > 0)
            {
                if (activeLOD == "high")
                {
                    //detect if we're trying to import a high poly mesh...
                    string msg = "You are about to import a high poly mesh. \nThese meshes are usually millions of polygons and can cause instability to your project. \nWould you like to proceed?";
                    highPoly = EditorUtility.DisplayDialog("WARNING!", msg, "Yes", "No");
                }
                try
                {
                    //process meshes and prefabs
                    PrefabData prefData = new PrefabData(path, assetName, folderNamingConvention, lodFadeMode, highPoly, addAssetToScene, setupCollision, hasBillboardLOD, isAlembic, false, false, finalMat, billboardMat, new List<string>(), new List<List<string>>());
                    MegascansMeshUtils.ProcessMeshes(objectList, path, highPoly, plant, prefData);
                }
                catch (Exception ex)
                {
                    Debug.Log("Error importing meshes.");
                    Debug.Log(ex.ToString());
                    MegascansUtilities.HideProgressBar();
                }
            }

            var endTime = System.DateTime.Now;
            var totalTime = endTime - startTime;
            Debug.Log("Asset Import Time: " + totalTime);
            AssetDatabase.Refresh();
            MegascansUtilities.HideProgressBar();
            Resources.UnloadUnusedAssets();
            GC.Collect();
            return path;
        }

        #region Texture Processing Methods

        void ProcessTextures(JObject objectList)
        {
            texPath = MegascansUtilities.ValidateFolderCreate(path, "Textures");
            matPath = Path.Combine(MegascansUtilities.ValidateFolderCreate(path, "Materials"), folderNamingConvention);

            if (!(plant && hasBillboardLODOnly))
            {
                MegascansUtilities.UpdateProgressBar(1.0f, "Processing Asset " + assetName, "Creating material...");
                finalMat = MegascansMaterialUtils.CreateMaterial(shaderType, matPath, isAlembic, dispType, texPack);
                ImportAllTextures(finalMat, (JArray)objectList["components"]);
                ImportAllTextures(finalMat, (JArray)objectList["packedTextures"]);
            }

            if (plant && hasBillboardLOD)
            {
                texPath = MegascansUtilities.ValidateFolderCreate(texPath, "Billboard");
                matPath += "_Billboard";
                MegascansUtilities.UpdateProgressBar(1.0f, "Processing Asset " + assetName, "Creating material...");
                billboardMat = MegascansMaterialUtils.CreateMaterial(shaderType, matPath, isAlembic, dispType, texPack);
                ImportAllTextures(billboardMat, (JArray)objectList["components-billboard"]);
                ImportAllTextures(billboardMat, (JArray)objectList["packed-billboard"]);
            }
        }

        void ImportAllTextures(Material mat, JArray texturesList)
        {
            try
            {
                List<string> typesOfTexturesAvailable = new List<string>();
                for (int i = 0; i < texturesList.Count; i++)
                {
                    typesOfTexturesAvailable.Add((string)texturesList[i]["type"]);
                }

                string destTexPath;
                Texture2D tex;

                for (int i = 0; i < texturesList.Count; i++)
                {
                    mapName = (string)texturesList[i]["type"];
                    MegascansUtilities.UpdateProgressBar(1.0f, "Processing Asset " + assetName, "Importing texture: " + mapName);

                    if ((string)texturesList[i]["type"] == "albedo" || ((string)texturesList[i]["type"] == "diffuse" && !typesOfTexturesAvailable.Contains("albedo")))
                    {
                        destTexPath = Path.Combine(texPath, (string)texturesList[i]["nameOverride"]);
                        MegascansTextureProcessor texPrcsr = new MegascansTextureProcessor((string)texturesList[i]["path"], destTexPath);
                        tex = texPrcsr.ImportTexture();

                        mat.SetTexture("_MainTex", tex);
                        mat.SetTexture("_BaseColorMap", tex);

                        if (shaderType == 1)
                        {
                            mat.SetTexture("_BaseMap", tex);
                            mat.SetColor("_BaseColor", Color.white);
                        }

                        if (MegascansUtilities.AlbedoHasOpacity((JObject)texturesList[i]["channelsData"]))
                        {
                            float alphaCutoff = 0.33f;
                            texPrcsr.AdjustAlphaCutoff();

                            if (shaderType > 0)
                            {
                                mat.SetFloat("_AlphaClip", 1);
                                mat.SetFloat("_Cutoff", 0.1f);
                                mat.SetFloat("_Mode", 1);
                                mat.SetFloat("_Cull", 0);
                                mat.EnableKeyword("_ALPHATEST_ON");
                            }
                            else
                            {
                                mat.SetInt("_AlphaCutoffEnable", 1);
                                mat.SetFloat("_AlphaCutoff", alphaCutoff);
                                mat.SetInt("_DoubleSidedEnable", 1);

                                mat.SetOverrideTag("RenderType", "TransparentCutout");
                                mat.SetInt("_ZTestGBuffer", (int)UnityEngine.Rendering.CompareFunction.Equal);
                                mat.SetInt("_CullMode", (int)UnityEngine.Rendering.CullMode.Off);
                                mat.SetInt("_CullModeForward", (int)UnityEngine.Rendering.CullMode.Back);
                                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                                mat.SetInt("_ZWrite", 1);
                                mat.renderQueue = 2450;
                                mat.SetInt("_ZTestGBuffer", (int)UnityEngine.Rendering.CompareFunction.Equal);

                                mat.EnableKeyword("_ALPHATEST_ON");
                                mat.EnableKeyword("_DOUBLESIDED_ON");
                                mat.DisableKeyword("_BLENDMODE_ALPHA");
                            }
                        }
                    }
                    else if ((string)texturesList[i]["type"] == "specular")
                    {
                        if (texPack > 0)
                        {
                            destTexPath = Path.Combine(texPath, (string)texturesList[i]["nameOverride"]);
                            MegascansTextureProcessor texPrcsr = new MegascansTextureProcessor((string)texturesList[i]["path"], destTexPath);
                            tex = texPrcsr.ImportTexture();

                            mat.SetTexture("_SpecGlossMap", tex);
                            mat.SetTexture("_SpecularColorMap", tex);
                            mat.SetColor("_SpecColor", new UnityEngine.Color(1.0f, 1.0f, 1.0f));
                            mat.SetColor("_SpecularColor", new UnityEngine.Color(1.0f, 1.0f, 1.0f));
                            mat.SetFloat("_WorkflowMode", 0);
                            mat.SetFloat("_MaterialID", 4);
                            mat.EnableKeyword("_METALLICSPECGLOSSMAP");
                            mat.EnableKeyword("_SPECGLOSSMAP");
                            mat.EnableKeyword("_SPECULAR_SETUP");
                            mat.EnableKeyword("_SPECULARCOLORMAP");
                            mat.EnableKeyword("_MATERIAL_FEATURE_SPECULAR_COLOR");
                        }
                    }
                    else if ((string)texturesList[i]["type"] == "masks")
                    {
                        if (texPack < 1 || shaderType < 1)
                        {
                            destTexPath = Path.Combine(texPath, (string)texturesList[i]["nameOverride"]);
                            MegascansTextureProcessor texPrcsr = new MegascansTextureProcessor((string)texturesList[i]["path"], destTexPath, false, false);
                            tex = texPrcsr.ImportTexture();

                            mat.SetTexture("_MaskMap", tex);
                            mat.SetTexture("_MetallicGlossMap", tex);
                            mat.EnableKeyword("_MASKMAP");
                            mat.SetFloat("_MaterialID", 1);
                            mat.EnableKeyword("_METALLICSPECGLOSSMAP");
                            mat.EnableKeyword("_METALLICGLOSSMAP");

                            bool hasMetalness;
                            bool hasAO;
                            bool hasGloss;

                            MegascansUtilities.MaskMapComponents((JObject)texturesList[i]["channelsData"], out hasMetalness, out hasAO, out hasGloss);

                            if (!hasMetalness)
                            {
                                mat.SetFloat("_Metallic", 1.0f);
                            }

                            if (hasAO)
                            {
                                mat.SetTexture("_OcclusionMap", tex);
                                mat.EnableKeyword("_OCCLUSIONMAP");
                            }
                        }

                    }
                    else if ((string)texturesList[i]["type"] == "normal")
                    {
                        string normalMapPath = (string)texturesList[i]["path"];
                        if (activeLOD == "high" && !normalMapPath.Contains("NormalBump"))
                        {
                            for (int x = 0; x < 10; x++)
                            {
                                string n = normalMapPath.Replace("_LOD" + x.ToString(), "Bump");
                                if (File.Exists(n))
                                {
                                    normalMapPath = n;
                                    break;
                                }

                            }
                            if (normalMapPath.Contains("NormalBump"))
                                continue;
                        }

                        destTexPath = Path.Combine(texPath, (string)texturesList[i]["nameOverride"]);
                        MegascansTextureProcessor texPrcsr = new MegascansTextureProcessor(normalMapPath, destTexPath, true, false);
                        tex = texPrcsr.ImportTexture();
                        mat.SetTexture("_BumpMap", tex);
                        mat.SetTexture("_NormalMap", tex);
                        mat.EnableKeyword("_NORMALMAP_TANGENT_SPACE");
                        mat.EnableKeyword("_NORMALMAP");
                    }
                    else if ((string)texturesList[i]["type"] == "ao" && texPack > 0)
                    {
                        destTexPath = Path.Combine(texPath, (string)texturesList[i]["nameOverride"]);
                        MegascansTextureProcessor texPrcsr = new MegascansTextureProcessor((string)texturesList[i]["path"], destTexPath, false, false);
                        tex = texPrcsr.ImportTexture();
                        mat.SetTexture("_OcclusionMap", tex);
                        mat.EnableKeyword("_OCCLUSIONMAP");
                    }
                    else if ((string)texturesList[i]["type"] == "displacement")
                    {
                        if (dispType > 0)
                        {
                            destTexPath = Path.Combine(texPath, (string)texturesList[i]["nameOverride"]);
                            MegascansTextureProcessor texPrcsr = new MegascansTextureProcessor((string)texturesList[i]["path"], destTexPath, false, false);
                            tex = texPrcsr.ImportTexture();
                            mat.SetTexture("_HeightMap", tex);
                            mat.SetTexture("_ParallaxMap", tex);
                            mat.EnableKeyword("_DISPLACEMENT_LOCK_TILING_SCALE");
                            if (shaderType == 0)
                                mat.EnableKeyword("_HEIGHTMAP");
                            if (dispType == 1)
                            {
                                mat.EnableKeyword("_VERTEX_DISPLACEMENT");
                                mat.EnableKeyword("_VERTEX_DISPLACEMENT_LOCK_OBJECT_SCALE");
                            }
                            else if (dispType == 2)
                            {
                                mat.EnableKeyword("_PARALLAXMAP");
                                mat.EnableKeyword("_PIXEL_DISPLACEMENT");
                                mat.EnableKeyword("_PIXEL_DISPLACEMENT_LOCK_OBJECT_SCALE");
                            }
                        }
                    }
                    else if ((string)texturesList[i]["type"] == "translucency")
                    {
                        destTexPath = Path.Combine(texPath, (string)texturesList[i]["nameOverride"]);
                        MegascansTextureProcessor texPrcsr = new MegascansTextureProcessor((string)texturesList[i]["path"], destTexPath);
                        tex = texPrcsr.ImportTexture();

                        mat.SetTexture("_SubsurfaceMaskMap", tex);
                        mat.EnableKeyword("_SUBSURFACE_MASK_MAP");
                        mat.SetInt("_DiffusionProfile", 1);
                        mat.SetFloat("_EnableSubsurfaceScattering", 1);
                        if (!typesOfTexturesAvailable.Contains("transmission"))
                        {
                            mat.SetTexture("_ThicknessMap", tex);
                            mat.EnableKeyword("_THICKNESSMAP");
                        }
                        if (plant)
                        {
                            mat.SetInt("_DiffusionProfile", 2);
                            mat.SetFloat("_CoatMask", 0.0f);
                            mat.SetInt("_EnableWind", 1);
                            mat.EnableKeyword("_VERTEX_WIND");
                        }
                        MegascansMaterialUtils.AddSSSSettings(mat, shaderType);
                    }
                    else if ((string)texturesList[i]["type"] == "transmission")
                    {
                        destTexPath = Path.Combine(texPath, (string)texturesList[i]["nameOverride"]);
                        MegascansTextureProcessor texPrcsr = new MegascansTextureProcessor((string)texturesList[i]["path"], destTexPath, false, false);
                        tex = texPrcsr.ImportTexture();

                        mat.SetTexture("_ThicknessMap", tex);
                        mat.EnableKeyword("_THICKNESSMAP");
                        mat.SetInt("_DiffusionProfile", 2);
                        MegascansMaterialUtils.AddSSSSettings(mat, shaderType);

                    }
                    else if (importAllTextures)
                    {
                        mapName = (string)texturesList[i]["type"];
                        string mapPath = (string)texturesList[i]["path"];
                        string otherTexFolder = MegascansUtilities.ValidateFolderCreate(texPath, "Others");
                        destTexPath = Path.Combine(otherTexFolder, (string)texturesList[i]["nameOverride"]);
                        MegascansTextureProcessor texPrcsr = new MegascansTextureProcessor(mapPath, destTexPath);
                        tex = texPrcsr.ImportTexture();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Exception::MegascansImporter::ImportAllTextures:: " + ex.ToString());
                MegascansUtilities.HideProgressBar();
            }
        }

        #endregion

        #region Formatting Utilities

        void LoadPreferences()
        {
            path = MegascansUtilities.FixPath(EditorPrefs.GetString("QuixelDefaultPath", "Quixel/Megascans/"));
            dispType = EditorPrefs.GetInt("QuixelDefaultDisplacement");
            texPack = EditorPrefs.GetInt("QuixelDefaultTexPacking");
            shaderType = EditorPrefs.GetInt("QuixelDefaultShader");
            lodFadeMode = EditorPrefs.GetInt("QuixelDefaultLodFadeMode", 1);
            setupCollision = EditorPrefs.GetBool("QuixelDefaultSetupCollision", true);
            applyToSelection = EditorPrefs.GetBool("QuixelDefaultApplyToSelection", false);
            addAssetToScene = EditorPrefs.GetBool("QuixelDefaultAddAssetToScene", false);
            importAllTextures = EditorPrefs.GetBool("QuixelDefaultImportAllTextures", false);
        }

        /// <summary>
        /// Returns the final directory for our asset, creating subfolders where necessary in the 'Assets' directory.
        /// </summary>
        string ConstructPath(JObject objectList)
        {
            /// Make sure path is "Assets/...." not "D:/Unity Projects/My Project/Assets/...." otherwise the AssetDatabase cannot write files to it.
            /// Lastly I also match the path with the Application DataPath in order to make sure this is the right path selected from the Bridge.

            AssetDatabase.Refresh();

            string defPath = "";
            bool addNextPathPart = false;

            if ((string)objectList["exportPath"] != "")
            {
                path = (string)objectList["exportPath"];
            }
            else
            {
                defPath = "Assets";
                addNextPathPart = true;
            }

            string[] pathParts = MegascansUtilities.FixSlashes(path).Split('/');

            List<string> finalPathParts = new List<string>();

            foreach (string part in pathParts)
            {
                if (part == "Assets" && !addNextPathPart)
                {
                    addNextPathPart = true;
                }

                if (addNextPathPart)
                {
                    finalPathParts.Add(part);
                }
            }

            if (!addNextPathPart)
            {
                return null;
            }

            //First, create the user specified path from the importer settings.

            if (finalPathParts.Count > 0)
            {
                for (int i = 0; i < finalPathParts.Count; i++)
                {
                    defPath = MegascansUtilities.ValidateFolderCreate(defPath, finalPathParts[i]); //FixSlashes(Path.Combine(defPath, finalPathParts[i]));//ValidateFolderCreate(defPath, finalPathParts[i]);
                }
            }

            if (!AssetDatabase.IsValidFolder(defPath))
            {
                return null;
            }

            //then create check to see if the asset type subfolder exists, create it if it doesn't.
            defPath = MegascansUtilities.ValidateFolderCreate(defPath, MegascansUtilities.GetAssetType((string)objectList["path"]));
            defPath = MegascansUtilities.ValidateFolderCreate(defPath, folderNamingConvention);
            return defPath;
        }
        #endregion
    }
}
#endif