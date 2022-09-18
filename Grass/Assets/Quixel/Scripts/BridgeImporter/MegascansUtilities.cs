#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace Quixel {
    public class MegascansUtilities : MonoBehaviour {
        
        #region Other_Utils
        /// <summary>
        /// Check whether the child folder you're trying to make already exists, if not, create it and return the directory.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        public static string ValidateFolderCreate (string parent, string child) {
            string tempPath = FixSlashes (Path.Combine (parent, child));
            if (!AssetDatabase.IsValidFolder (tempPath)) {
                string newPath = AssetDatabase.CreateFolder (parent, child);
                return AssetDatabase.GUIDToAssetPath (newPath);
            }
            return FixSlashes (tempPath);
        }

        public static string FixSlashes (string txt) {
            txt = txt.Replace ("\\", "/");
            txt = txt.Replace (@"\\", "/");
            return txt;
        }

        /// <summary>
        /// Remove spaces and remove special characters.
        /// </summary>
        public static string FixPath (string orgPath) {
            string path = orgPath.Trim ();
            string[] pathFolders = path.Split ('/');

            for (int j = 0; j < pathFolders.Length - 1; j++) {
                pathFolders[j] = pathFolders[j].Trim ();
            }

            path = pathFolders[0];
            for (int i = 1; i < pathFolders.Length; i++) {
                path += "/" + pathFolders[i];
            }
            return path;
        }

        static bool isLegacyProject = false;
        static bool identifiedPipeline = false;

        /// <summary>
        /// This function attempts to auto-detect which template the project is using. Defaults to Legacy/Standard if all else fails.
        /// </summary>
        public static int GetShaderType()
        {
            int shaderType = EditorPrefs.GetInt("QuixelDefaultShader");
            return shaderType == 3 ? (int)getCurrentPipeline() : shaderType;
        }

        public static bool isLegacy () {
            if (!identifiedPipeline) {
                string[] versionParts = Application.unityVersion.Split ('.');
                int majorVersion = int.Parse (versionParts[0]);
                int minorVersion = int.Parse (versionParts[1]);
                isLegacyProject = (majorVersion < 2018 || (majorVersion == 2018 && minorVersion < 3));
            }
            return isLegacyProject;

        }

        //attempt to auto-detect a settings file for Lightweight or HD pipelines
        public static Pipeline getCurrentPipeline () {
            Pipeline currentPipeline = Pipeline.HDRP;

            //attempt to auto-detect a settings file for Lightweight or HD pipelines
            if (AssetDatabase.IsValidFolder ("Assets/Settings")) {
                if (AssetDatabase.LoadAssetAtPath ("Assets/Settings/HDRenderPipelineAsset.asset", typeof (ScriptableObject)) ||
                    AssetDatabase.LoadAssetAtPath("Assets/Settings/HDRP High Quality.asset", typeof(ScriptableObject)) ||
                    AssetDatabase.LoadAssetAtPath("Assets/Settings/HDRP Low Quality.asset", typeof(ScriptableObject)) ||
                    AssetDatabase.LoadAssetAtPath("Assets/Settings/HDRP Medium Quality.asset", typeof(ScriptableObject)) ||
                    AssetDatabase.LoadAssetAtPath("Assets/Settings/HDRPHighQuality.asset", typeof(ScriptableObject)) ||
                    AssetDatabase.LoadAssetAtPath("Assets/Settings/HDRPLowQuality.asset", typeof(ScriptableObject)) ||
                    AssetDatabase.LoadAssetAtPath("Assets/Settings/HDRPMediumQuality.asset", typeof(ScriptableObject))) {
                    currentPipeline = Pipeline.HDRP;
                } else if (AssetDatabase.LoadAssetAtPath ("Assets/Settings/Lightweight_RenderPipeline.asset", typeof (ScriptableObject)) ||
                    AssetDatabase.LoadAssetAtPath ("Assets/Settings/LWRP-HighQuality.asset", typeof (ScriptableObject)) ||
                    AssetDatabase.LoadAssetAtPath ("Assets/Settings/LWRP-LowQuality.asset", typeof (ScriptableObject)) ||
                    AssetDatabase.LoadAssetAtPath ("Assets/Settings/LWRP-MediumQuality.asset", typeof (ScriptableObject))) {
                    currentPipeline = Pipeline.LWRP;
                } else if (AssetDatabase.LoadAssetAtPath("Assets/Settings/UniversalRP-HighQuality.asset", typeof(ScriptableObject)) ||
                  AssetDatabase.LoadAssetAtPath("Assets/Settings/UniversalRP-LowQuality.asset", typeof(ScriptableObject)) ||
                  AssetDatabase.LoadAssetAtPath("Assets/Settings/UniversalRP-MediumQuality.asset", typeof(ScriptableObject)))
                {
                    currentPipeline = Pipeline.LWRP;
                }
                else {
                    currentPipeline = Pipeline.Standard;
                }
            } else {
                if (AssetDatabase.FindAssets ("HDRenderPipelineAsset").Length > 0 ||
                    AssetDatabase.FindAssets("HDRPHighQuality").Length > 0 ||
                    AssetDatabase.FindAssets("HDRP High Quality").Length > 0) {
                    currentPipeline = Pipeline.HDRP;
                } else if (AssetDatabase.FindAssets ("Lightweight_RenderPipeline").Length > 0 ||
                    AssetDatabase.FindAssets ("LWRP-HighQuality").Length > 0 ||
                    AssetDatabase.FindAssets ("LWRP-LowQuality").Length > 0 ||
                    AssetDatabase.FindAssets ("LWRP-MediumQuality").Length > 0) {
                    currentPipeline = Pipeline.LWRP;
                } else if (AssetDatabase.FindAssets("UniversalRP-HighQuality").Length > 0 ||
                   AssetDatabase.FindAssets("UniversalRP-LowQuality").Length > 0 ||
                   AssetDatabase.FindAssets("UniversalRP-MediumQuality").Length > 0)
                {
                    currentPipeline = Pipeline.LWRP;
                } else {
                    currentPipeline = Pipeline.Standard;
                }
            }

            return currentPipeline;
        }

        // Tells if an asset type is scatter or not. 
        public static bool isScatterAsset(JObject assetJson, List<string> importedMeshpaths)
        {
            try
            {
                string[] tags = assetJson["tags"].ToObject<string[]>();
                string[] categories = assetJson["categories"].ToObject<string[]>();
                int childCount = GetMeshChildrenCount(importedMeshpaths);

                foreach(string tag in tags)
                {
                    if (tag.ToLower() == "scatter")
                    {
                        return (childCount > 1); //Returns false if the is only one variation of asset.
                    } else if (tag.ToLower() == "cmb_asset")
                    {
                        return (childCount > 1); //Returns false if the is only one variation of asset.
                    }
                }

                foreach (string category in categories)
                {
                    if (category.ToLower() == "scatter")
                    {
                        return (childCount > 1); //Returns false if the is only one variation of asset.
                    }
                    else if (category.ToLower() == "cmb_asset")
                    {
                        return (childCount > 1); //Returns false if the is only one variation of asset.
                    }
                }

                return (childCount > 1);
            }
            catch (Exception ex)
            {
                Debug.Log("Exception::MegascansUtilities::IsScatterAsset:: " + ex.ToString());
                HideProgressBar();
            }

            return false;
        }

        public static int GetMeshChildrenCount(List<string> importedMeshpaths)
        {
            try
            {
                if(importedMeshpaths.Count > 0)
                {
                    UnityEngine.Object loadedGeometry = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(importedMeshpaths[0]);
                    GameObject testGO = (GameObject)Instantiate(loadedGeometry);
                    int count = testGO.transform.childCount;
                    DestroyImmediate(testGO);
                    return count;
                }
            } catch (Exception ex)
            {
                Debug.Log("Exception::MegascansUtilities::GetMeshChildrenCount:: " + ex.ToString());
                HideProgressBar();
            }

            return 1;
        }

        /// <summary>
        /// Determine which asset type we're creating. Surfaces, 3D_Assets, 3D_Scatter_Assets, 3D_Plants.
        /// </summary>
        public static string GetAssetType(string jsonPath)
        {
            string t = "Surfaces";
            if (jsonPath.ToLower().Contains("3d"))
            {
                t = "3D_Assets";
            }
            if (jsonPath.ToLower().Contains("debris") ||
                jsonPath.ToLower().Contains("dbrs") ||
                jsonPath.ToLower().Contains("scatter") ||
                jsonPath.ToLower().Contains("sctr"))
            {
                t = "3D_Scatter_Assets";
            }
            if (jsonPath.ToLower().Contains("3dplant"))
            {
                t = "3D_Plants";
            }
            if (jsonPath.ToLower().Contains("decals") || jsonPath.ToLower().Contains("decal"))
            {
                t = "Decals";
            }
            else if (jsonPath.ToLower().Contains("atlas"))
            {
                t = "Atlases";
            }
            return t;
        }

        public static void CopyFileToProject(string sourcePath, string destPath)
        {
            try
            {
                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, destPath, true);
                    AssetDatabase.ImportAsset(destPath);
                    AssetDatabase.Refresh();
                } else
                {
                    Debug.Log("Exception::MegascansImageUtils::ImportTexture:: Source file does not exist.");
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Exception::MegascansMeshUtils::Importing file to project:: " + ex.ToString());
                HideProgressBar();
            }
        }
        #endregion

        #region Selection Helpers
        /// <summary>
        /// Retrieves selected folders in Project view.
        /// </summary>
        public static List<string> GetSelectedFolders (List<UnityEngine.Object> selections) {
            List<string> folders = new List<string> ();

            foreach (UnityEngine.Object obj in selections) //Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                string path = AssetDatabase.GetAssetPath (obj);
                if (!string.IsNullOrEmpty (path)) {
                    folders.Add (path);
                }
            }
            return folders;
        }

        /// <summary>
        /// Retrieves selected material.
        /// </summary>
        public static Material GetSelectedMaterial(string fileType = ".mat")
        {
            foreach (Material obj in Selection.GetFiltered(typeof(Material), SelectionMode.Assets))
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (path.Contains(fileType))
                {
                    return (Material)AssetDatabase.LoadAssetAtPath(path, typeof(Material));
                }
            }
            return null;
        }

        /// <summary>
        /// Retrieves selected GameObjects with MeshRenderer component in Scene view.
        /// </summary>
        public static List<MeshRenderer> GetSelectedMeshRenderers()
        {
            List<MeshRenderer> meshRenderers = new List<MeshRenderer>();

            foreach (GameObject g in Selection.gameObjects)
            {
                if (g.GetComponent<MeshRenderer>() != null)
                {
                    meshRenderers.Add(g.GetComponent<MeshRenderer>());
                }
            }

            return meshRenderers;
        }

        /// <summary>
        /// Recursively gather all files under the given path including all its subfolders.
        /// </summary>
        public static List<string> GetFiles (string path, string fileType = null) {
            List<string> files = new List<string> ();
            Queue<string> queue = new Queue<string> ();
            queue.Enqueue (path);
            while (queue.Count > 0) {
                path = queue.Dequeue ();
                foreach (string subDir in Directory.GetDirectories (path)) {
                    queue.Enqueue (subDir);
                }
                foreach (string s in Directory.GetFiles (path)) {
                    if (fileType != null && s.Contains (fileType)) {
                        if (s.Contains (fileType)) {
                            files.Add (s);
                        }
                    } else {
                        files.Add (s);
                    }

                }
            }
            return files;
        }
        #endregion

        #region HDRP Features PreDefined Macro Setup
        /*
#if (UNITY_2018_2 || UNITY_2018_3 || UNITY_2018_4 || UNITY_2019 || UNITY_2020 || UNITY_2021)
        [MenuItem ("Window/Quixel/Enable HDRP Features")]
        private static void EnableHDRP () {
            Debug.Log ("HDRP enabled.");
            AddDefineIfNecessary ("HDRP", EditorUserBuildSettings.selectedBuildTargetGroup);
        }

        [MenuItem ("Window/Quixel/Disable HDRP Features")]
        private static void DisableHDRP () {
            Debug.Log ("HDRP disabled.");
            RemoveDefineIfNecessary ("HDRP", EditorUserBuildSettings.selectedBuildTargetGroup);
        }
#endif
        */
        public static void AddDefineIfNecessary (string _define, BuildTargetGroup _buildTargetGroup) {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup (_buildTargetGroup);

            if (defines == null) { defines = _define; } else if (defines.Length == 0) { defines = _define; } else { if (defines.IndexOf (_define, 0) < 0) { defines += ";" + _define; } }

            PlayerSettings.SetScriptingDefineSymbolsForGroup (_buildTargetGroup, defines);
        }

        public static void RemoveDefineIfNecessary (string _define, BuildTargetGroup _buildTargetGroup) {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup (_buildTargetGroup);

            if (defines.StartsWith (_define + ";")) {
                // First of multiple defines.
                defines = defines.Remove (0, _define.Length + 1);
            } else if (defines.StartsWith (_define)) {
                // The only define.
                defines = defines.Remove (0, _define.Length);
            } else if (defines.EndsWith (";" + _define)) {
                // Last of multiple defines.
                defines = defines.Remove (defines.Length - _define.Length - 1, _define.Length + 1);
            } else {
                // Somewhere in the middle or not defined.
                var index = defines.IndexOf (_define, 0, System.StringComparison.Ordinal);
                if (index >= 0) { defines = defines.Remove (index, _define.Length + 1); }
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup (_buildTargetGroup, defines);
        }
        #endregion

        #region Progress Bar Utils

        static float maxNumberOfOperations = 0;
        static float currentOperationCount = 0;

        public static void CalculateNumberOfOperations(JObject assetData, int dispType, int texPack, int shaderType, bool hasBillboardLODOnly)
        {
            JArray meshComps = (JArray)assetData["meshList"];
            int prefabCount = meshComps.Count;

            JArray lodList = (JArray)assetData["lodList"];
            int meshCount = meshComps.Count;

            JArray textureComps = (JArray)assetData["components"];

            List<string> texTypes = new List<string>();

            for (int i = 0; i < textureComps.Count; ++i)
            {
                texTypes.Add((string)textureComps[i]["type"]);
            }

            int texCount = 0;

            if (texTypes.Contains("albedo") || texTypes.Contains("diffuse"))
                texCount++;

            if (texTypes.Contains("normal"))
                texCount++;

            if (texTypes.Contains("displacement") && dispType != 0)
                texCount++;

            if (texTypes.Contains("translucency"))
                texCount++;

            if (texTypes.Contains("ao") && shaderType == 2)
                texCount++;

            if (texPack == 0)
            {
                if (texTypes.Contains("metalness") || texTypes.Contains("roughness") || texTypes.Contains("gloss") || texTypes.Contains("ao") || texTypes.Contains("displacement"))
                    texCount++;
            }
            else if (texPack == 1)
            {
                if ((texTypes.Contains("metalness") || texTypes.Contains("roughness") || texTypes.Contains("gloss") || texTypes.Contains("ao") || texTypes.Contains("displacement")) && shaderType == 0)
                    texCount++;

                if (texTypes.Contains("specular"))
                    texCount++;
            }

            string type = (string)assetData["type"];
            if (type.ToLower().Contains("3dplant") && !hasBillboardLODOnly)
            {
                texCount *= 2;
            }

            maxNumberOfOperations = (float)(prefabCount + meshCount + texCount);
            maxNumberOfOperations += 1.0f; //For the material
        }

        public static void UpdateProgressBar(float change = 0, string header = "Import Megascans Asset", string message = "Processing Asset")
        {
            currentOperationCount += change;
            if (currentOperationCount != maxNumberOfOperations)
                EditorUtility.DisplayProgressBar(header, message, (currentOperationCount / maxNumberOfOperations));
            else
                HideProgressBar();
        }

        public static void HideProgressBar()
        {
            currentOperationCount = 0;
            maxNumberOfOperations = 0;
            EditorUtility.ClearProgressBar();
        }

        public static void PrintProgressBarStats()
        {
            Debug.Log("Current: " + currentOperationCount);
            Debug.Log("Max: " + maxNumberOfOperations);
        }

        #endregion

        #region LOD Heights
        
        public static List<float> getLODHeightList(int numberOfFiles)
        {
            switch (numberOfFiles)
            {
                case 1:
                    return new List<float> { 0.01f };
                case 2:
                    return new List<float> { 0.4f, 0.01f };
                case 3:
                    return new List<float> { 0.5f, 0.2f, 0.01f };
                case 4:
                    return new List<float> { 0.5f, 0.3f, 0.18f, 0.01f };
                case 5:
                    return new List<float> { 0.5f, 0.3f, 0.2f, 0.1f, 0.01f };
                case 6:
                    return new List<float> { 0.55f, 0.35f, 0.24f, 0.15f, 0.07f, 0.01f };
                case 7:
                    return new List<float> { 0.6f, 0.4f, 0.3f, 0.21f, 0.13f, 0.06f, 0.01f };
                default:
                    return new List<float> { 0.65f, 0.45f, 0.35f, 0.26f, 0.18f, 0.11f, 0.06f, 0.01f };
            }
        }

        #endregion

        #region Channel packed textures data verification

        public static bool AlbedoHasOpacity(JObject channelData)
        {
            return channelData == null ? false : verifyMapInList((JArray)channelData["Alpha"], "opacity");
        }

        public static bool SpecularHasGloss(JObject channelData)
        {
            return channelData == null ? false : verifyMapInList((JArray)channelData["Alpha"], "gloss");
        }

        public static void MaskMapComponents(JObject channelData, out bool hasMetalness, out bool hasAO, out bool hasGloss)
        {
            hasMetalness = channelData == null ? false : verifyMapInList((JArray)channelData["Red"], "metalness");
            hasAO = channelData == null ? false : verifyMapInList((JArray)channelData["Green"], "ao");
            hasGloss = channelData == null ? false : verifyMapInList((JArray)channelData["Alpha"], "gloss");
        }

        public static bool verifyMapInList(JArray channelData, string texType)
        {
            for (int i = 0; i < channelData.Count; i++)
            {
                string packedTex = (string)channelData[i];
                if (packedTex.ToLower() == texType.ToLower())
                {
                    return true;
                }
            }
            return false;

        }
        #endregion
    }
}

#endif

public enum Pipeline {
    HDRP,
    LWRP,
    Standard
}