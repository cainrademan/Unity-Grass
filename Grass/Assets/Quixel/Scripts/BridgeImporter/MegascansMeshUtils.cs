#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;
using System.IO;
using System;

namespace Quixel
{
    //This class imports the geometry and create the prefabs.
    public class MegascansMeshUtils : MonoBehaviour
    {
        /// <summary>
        /// Import meshes, start from highest LOD and import the chain.
        /// </summary>
        public static void ProcessMeshes(JObject assetJson, string assetFolderPath, bool highpoly, bool hasVariations, PrefabData prefabData)
        {
            try
            {
                bool createPrefabs = EditorPrefs.GetBool("QuixelDefaultSetupPrefabs", true);
                bool importLODs = EditorPrefs.GetBool("QuixelDefaultImportLODs", true);
                bool setupLODs = EditorPrefs.GetBool("QuixelDefaultSetupLOD", true);
                prefabData.setupLODs = (importLODs && setupLODs); //Only do LOD setup if lower lods were imported and LOD grouping is enabled.

                //get mesh components from the current object. Also, meshComps.Count can give us the number of variations ;)
                JArray meshComps = (JArray)assetJson["meshList"];

                JArray lodList = (JArray)assetJson["lodList"];
                string activeLOD = (string)assetJson["activeLOD"];
                string minLOD = (string)assetJson["minLOD"];

                string modelsFolderPath = MegascansUtilities.ValidateFolderCreate(assetFolderPath, "Models");

                if (hasVariations)
                {
                    List<List<string>> importedGeometryPaths3DPlant = new List<List<string>>();

                    for (int i = 1; i <= meshComps.Count; i++)
                    {
                        List<string> importedGeometryPaths = new List<string>();
                        bool lodMatched = false; // This flag helps to import the lower lods once the active lod is found.
                        foreach (JObject mesh in lodList)
                        {
                            if ((int)mesh["variation"] == i)
                            {
                                string currentLOD = (string)mesh["lod"];
                                if (lodMatched || currentLOD == activeLOD || highpoly)
                                {
                                    lodMatched = true;
                                    if ((currentLOD == "high") && !highpoly)
                                    {
                                        continue;
                                    }
                                    //get the path of the highest LOD to be imported.
                                    string sourcePath = (string)mesh["path"];
                                    string destPath = Path.Combine(modelsFolderPath, (string)mesh["nameOverride"]);
                                    ImportMesh(sourcePath, destPath);
                                    importedGeometryPaths.Add(destPath);
                                    if(!importLODs)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        importedGeometryPaths3DPlant.Add(importedGeometryPaths);
                    }
                    prefabData.importedGeometryPaths3DPlant = importedGeometryPaths3DPlant;
                    if(createPrefabs)
                        CreatePrefab3DPlant(prefabData);
                }
                else
                {
                    List<string> importedGeometryPaths3D = new List<string>();
                    bool lodMatched = false; // This flag helps to import the lower lods once the active lod is found.
                    foreach (JObject mesh in lodList)
                    {
                        string currentLOD = (string)mesh["lod"];
                        if (lodMatched || (currentLOD == activeLOD) || highpoly)
                        {
                            lodMatched = true;
                            if ((currentLOD == "high") && !highpoly)
                            {
                                continue;
                            }
                            //get the path of the highest LOD to be imported.
                            string sourcePath = (string)mesh["path"];
                            string destPath = Path.Combine(modelsFolderPath, (string)mesh["nameOverride"]);
                            ImportMesh(sourcePath, destPath);
                            importedGeometryPaths3D.Add(destPath);
                            if (!importLODs)
                            {
                                break;
                            }
                        }

                    }
                    prefabData.importedGeometryPaths3D = importedGeometryPaths3D;
                    if (createPrefabs)
                    {
                        if (MegascansUtilities.isScatterAsset(assetJson, importedGeometryPaths3D))
                        {
                            CreatePrefabsScatter(prefabData);
                        }
                        else
                        {
                            CreatePrefab3D(prefabData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Exception::MegascansMeshUtils::Processing Meshes:: " + ex.ToString());
                MegascansUtilities.HideProgressBar();
            }
        }

        static void ImportMesh(string sourcePath, string destPath)
        {
            MegascansUtilities.UpdateProgressBar(1.0f, "Importing Megascans Asset", "Importing Mesh...");
            MegascansUtilities.CopyFileToProject(sourcePath, destPath);
        }

        /// <summary>
        /// Generates prefabs from imported meshes.
        /// Used for normal 3D assets
        /// </summary>
        public static void CreatePrefab3D(PrefabData prefabData)
        {
            try {
                string prefabPath = MegascansUtilities.ValidateFolderCreate(prefabData.assetPath, "Prefabs");
                string prefabName = prefabData.modelNamingConvention;

                //Setting up prefab gameobject
                GameObject prefabGameObject = new GameObject();
                prefabGameObject.name = prefabName;
                prefabGameObject.isStatic = true;
                if (prefabData.setupLODs)
                {
                    prefabGameObject.AddComponent<LODGroup>();
                    prefabGameObject.GetComponent<LODGroup>().fadeMode = (LODFadeMode)prefabData.lodFadeMode; //Casting lod fade mode to enum.
                    prefabGameObject.GetComponent<LODGroup>().animateCrossFading = true;
                }

                List<LOD> lodsForPrefab = new List<LOD>();
                int numberOfFiles = prefabData.importedGeometryPaths3D.Count;

                List<float> lodHeights = MegascansUtilities.getLODHeightList(numberOfFiles);
                //Instantiate all the meshes in the scene, add them to the material/collider to them.
                for (int x = 0; (x < numberOfFiles && x < 8); x++)
                {
                    UnityEngine.Object loadedGeometry = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(prefabData.importedGeometryPaths3D[x]);
                    //Highpoly mesh check.
                    if (loadedGeometry.name.ToLower().Contains("highpoly") && !prefabData.highpoly)
                    {
                        continue;
                    }
                    GameObject geometryObject = Instantiate(loadedGeometry) as GameObject;
                    Renderer[] r;
                    //Parent all the objects to the prefab game object.
                    if (geometryObject.transform.childCount > 0 && !prefabData.isAlembic)
                    {
                        r = new Renderer[geometryObject.transform.childCount];
                        for (int j = 0; j < geometryObject.transform.childCount; ++j)
                        {
                            //Parent the child gameobject (geometry) to the prefab game object.
                            GameObject geometryChildObject = geometryObject.transform.GetChild(j).gameObject; //Cache a reference to the child gameobject of the geometry.
                            geometryChildObject.transform.parent = prefabGameObject.transform;
                            geometryChildObject.transform.localPosition = Vector3.zero;
                            geometryChildObject.name = geometryChildObject.name.Replace("(Clone)", "");
                            r[j] = geometryChildObject.GetComponentInChildren<Renderer>();
                        }
                        //Destroy the empty parent container which was holding the meshes.
                        DestroyImmediate(geometryObject);
                    }
                    else if (prefabData.isAlembic) //if the instantiated mesh is an alembic asset.
                    {
                        //Parent the child gameobject (geometry) to the prefab game object.
                        geometryObject.transform.parent = prefabGameObject.transform;
                        geometryObject.transform.localPosition = Vector3.zero;
                        geometryObject.name = geometryObject.name.Replace("(Clone)", "");
                        r = geometryObject.GetComponentsInChildren<Renderer>();
                    }
                    else //if the instantiated mesh does not have any children
                    {
                        //Parent the child gameobject (geometry) to the prefab game object.
                        geometryObject.transform.parent = prefabGameObject.transform;
                        geometryObject.transform.localPosition = Vector3.zero;
                        geometryObject.name = geometryObject.name.Replace("(Clone)", "");
                        r = geometryObject.GetComponentsInChildren<Renderer>();
                    }

                    foreach (Renderer ren in r)
                    {
                        ren.material = prefabData.finalMat;
                        //Apply collision
                        if (prefabData.setupCollision)
                            ren.gameObject.AddComponent<MeshCollider>().sharedMesh = ren.gameObject.GetComponent<MeshFilter>().sharedMesh;
                    }

                    if (prefabData.setupLODs)
                    {
                        lodsForPrefab.Add(new LOD(lodHeights[0], r));
                        lodHeights.RemoveAt(0);
                    } else //We only set the prefab with 1 LOD if setup LODs is unchecked.
                        break;
                }
                //Set LODs in the LOD group
                if (prefabData.setupLODs)
                {
                    prefabGameObject.GetComponent<LODGroup>().SetLODs(lodsForPrefab.ToArray());
                    prefabGameObject.GetComponent<LODGroup>().RecalculateBounds();
                }
                //Prefab saving
                string prefLocation = prefabPath + "/" + prefabName + ".prefab";
                prefLocation = prefLocation.Replace("(Clone)", "");
                SavePrefab(prefabGameObject, prefLocation, prefabData.addAssetToScene);
            }
            catch (Exception ex)
            {
                Debug.Log("Exception::MegascansMeshUtils::3D Asset Prefab:: " + ex.ToString());
                MegascansUtilities.HideProgressBar();
            }
        }

        /// <summary>
        /// Creates prefabs from the newer assets on bridge, has an option for billboard materials on plants.
        /// </summary>
        /// <param name="hasBillboard"></param>
        /// <returns></returns>
        public static void CreatePrefab3DPlant(PrefabData prefabData)
        {
            try
            {
                string prefabPath = MegascansUtilities.ValidateFolderCreate(prefabData.assetPath, "Prefabs");
                List<GameObject> prefabObjects = new List<GameObject>();

                for (int i = 0; i < prefabData.importedGeometryPaths3DPlant.Count; i++)
                {
                    string prefabName = prefabData.modelNamingConvention + "_Var" + (i + 1).ToString();
                    //Setting up prefab gameobject
                    GameObject prefabGameObject = new GameObject();
                    prefabGameObject.name = prefabName;
                    prefabGameObject.isStatic = true;

                    if (prefabData.setupLODs)
                    {
                        prefabGameObject.AddComponent<LODGroup>();
                        prefabGameObject.GetComponent<LODGroup>().fadeMode = (LODFadeMode)prefabData.lodFadeMode; //Casting lod fade mode to enum.
                        prefabGameObject.GetComponent<LODGroup>().animateCrossFading = true;
                    }

                    List<LOD> lodsForPrefab = new List<LOD>();
                    int numberOfFiles = prefabData.importedGeometryPaths3DPlant[i].Count;

                    List<float> lodHeights = MegascansUtilities.getLODHeightList(numberOfFiles);
                    //Instantiate all the meshes in the scene, add them to the material/collider to them.
                    for (int x = 0; (x < numberOfFiles && x < 8); x++)
                    {
                        UnityEngine.Object loadedGeometry = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(prefabData.importedGeometryPaths3DPlant[i][x]);
                        //Highpoly mesh check.
                        if (loadedGeometry.name.ToLower().Contains("highpoly") && !prefabData.highpoly)
                        {
                            continue;
                        }
                        GameObject geometryObject = Instantiate(loadedGeometry) as GameObject;
                        Renderer[] r;
                        //Parent all the objects to the prefab game object.
                        if (geometryObject.transform.childCount > 0 && !prefabData.isAlembic)
                        {
                            r = new Renderer[geometryObject.transform.childCount];
                            for (int j = 0; j < geometryObject.transform.childCount; ++j)
                            {
                                //Parent the child gameobject (geometry) to the prefab game object.
                                GameObject geometryChildObject = geometryObject.transform.GetChild(j).gameObject; //Cache a reference to the child gameobject of the geometry.
                                geometryChildObject.transform.parent = prefabGameObject.transform;
                                geometryChildObject.transform.localPosition = Vector3.zero;
                                geometryChildObject.name = geometryChildObject.name.Replace("(Clone)", "");
                                r[j] = geometryChildObject.GetComponent<Renderer>();
                            }
                            //Destroy the empty parent container which was holding the meshes.
                            DestroyImmediate(geometryObject);
                        }
                        else if (prefabData.isAlembic) //if the instantiated mesh is an alembic asset.
                        {
                            //Parent the child gameobject (geometry) to the prefab game object.
                            geometryObject.transform.parent = prefabGameObject.transform;
                            geometryObject.transform.localPosition = Vector3.zero;
                            geometryObject.name = geometryObject.name.Replace("(Clone)", "");
                            r = geometryObject.GetComponentsInChildren<Renderer>();
                        }
                        else //if the instantiated mesh does not have any children
                        {
                            r = new Renderer[1];
                            //Parent the child gameobject (geometry) to the prefab game object.
                            geometryObject.transform.parent = prefabGameObject.transform;
                            geometryObject.transform.localPosition = Vector3.zero;
                            geometryObject.name = geometryObject.name.Replace("(Clone)", "");
                            r[0] = geometryObject.GetComponent<Renderer>();
                        }

                        foreach (Renderer ren in r)
                        {
                            ren.material = prefabData.finalMat;
                            //Billboard material application
                            if (prefabData.hasBillboardLOD && x == (numberOfFiles - 1))
                            {
                                ren.material = prefabData.billboardMat;
                            }

                            //Apply collision
                            if (prefabData.setupCollision)
                                ren.gameObject.AddComponent<MeshCollider>().sharedMesh = ren.gameObject.GetComponent<MeshFilter>().sharedMesh;
                        }

                        if (prefabData.setupLODs)
                        {
                            lodsForPrefab.Add(new LOD(lodHeights[0], r));
                            lodHeights.RemoveAt(0);
                        }
                        else
                            break;
                    }
                    //Set LODs in the LOD group
                    if (prefabData.setupLODs)
                    {
                        prefabGameObject.GetComponent<LODGroup>().SetLODs(lodsForPrefab.ToArray());
                        prefabGameObject.GetComponent<LODGroup>().RecalculateBounds();
                    }
                    //Prefab saving
                    string prefLocation = prefabPath + "/" + prefabName + ".prefab";
                    prefLocation = prefLocation.Replace("(Clone)", "");
                    GameObject prefabObject = SavePrefab(prefabGameObject, prefLocation, prefabData.addAssetToScene);
                    if (prefabObject)
                        prefabObjects.Add(prefabObject);
                }

                //Setting up variation holder gameobject
                if (prefabData.addAssetToScene)
                {
                    GameObject plantsParent = new GameObject(prefabData.assetName);
                    plantsParent.isStatic = true;
                    foreach (GameObject variation in prefabObjects)
                    {
                        variation.transform.parent = plantsParent.transform;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Exception::MegascansMeshUtils::3D Plant Prefab:: " + ex.ToString());
                MegascansUtilities.HideProgressBar();
            }
        }

        /// <summary>
        /// Creates prefabs for the 3D Scatter assets.
        /// </summary>
        /// <param name="hasBillboard"></param>
        /// <returns></returns>
        public static void CreatePrefabsScatter(PrefabData prefabData)
        {
            try
            {
                string prefabPath = MegascansUtilities.ValidateFolderCreate(prefabData.assetPath, "Prefabs");
                int numberOfVariations = MegascansUtilities.GetMeshChildrenCount(prefabData.importedGeometryPaths3D);
                List<GameObject> prefabObjects = new List<GameObject>();

                for (int i = 0; i < numberOfVariations; i++)
                {
                    string prefabName = prefabData.modelNamingConvention + "_Var" + (i + 1).ToString();

                    //Setting up prefab gameobject
                    GameObject prefabGameObject = new GameObject();
                    prefabGameObject.name = prefabName;
                    prefabGameObject.isStatic = true;
                    if (prefabData.setupLODs)
                    {
                        prefabGameObject.AddComponent<LODGroup>();
                        prefabGameObject.GetComponent<LODGroup>().fadeMode = (LODFadeMode)prefabData.lodFadeMode; //Casting lod fade mode to enum.
                        prefabGameObject.GetComponent<LODGroup>().animateCrossFading = true;
                    }

                    List<LOD> lodsForPrefab = new List<LOD>();
                    int numberOfFiles = prefabData.importedGeometryPaths3D.Count;

                    List<float> lodHeights = MegascansUtilities.getLODHeightList(numberOfFiles);
                    //Instantiate all the meshes in the scene, add them to the material/collider to them.
                    for (int x = 0; (x < numberOfFiles && x < 8); x++)
                    {
                        UnityEngine.Object loadedGeometry = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(prefabData.importedGeometryPaths3D[x]);
                        //Highpoly mesh check.
                        if (loadedGeometry.name.ToLower().Contains("highpoly") && !prefabData.highpoly)
                        {
                            continue;
                        }
                        GameObject geometryObject = Instantiate(loadedGeometry) as GameObject;
                        Renderer[] r;
                        if (prefabData.isAlembic) //if the instantiated mesh is an alembic asset.
                        {
                            //Get all variations in a LOD
                            List<Transform> varsInLOD = new List<Transform>();
                            foreach (Transform var in geometryObject.transform)
                            {
                                varsInLOD.Add(var);
                            }
                            //Delete all the other variations in the LOD object
                            for (int y = 0; y < varsInLOD.Count; y++)
                            {
                                //If variation does not match one currently being processed.
                                if (y != i)
                                {
                                    DestroyImmediate(varsInLOD[y].gameObject);
                                }
                            }
                            //Parent the child gameobject (geometry) to the prefab game object.
                            geometryObject.transform.parent = prefabGameObject.transform;
                            geometryObject.transform.localPosition = Vector3.zero;
                            geometryObject.name = geometryObject.name.Replace("(Clone)", "");
                            r = geometryObject.GetComponentsInChildren<Renderer>();
                        }
                        else//if the instantiated mesh is a scatter type asset.
                        {
                            //Get all variations in a LOD
                            List<Transform> varsInLOD = new List<Transform>();
                            foreach (Transform var in geometryObject.transform)
                            {
                                varsInLOD.Add(var);
                            }
                            //Delete all the other variations in the LOD object
                            for(int y = 0; y < varsInLOD.Count; y++)
                            {
                                //If variation does not match one currently being processed.
                                if(y!=i)
                                {
                                    DestroyImmediate(varsInLOD[y].gameObject);
                                }
                            }
                            //Parent the child gameobject (geometry) to the prefab game object.
                            geometryObject.transform.parent = prefabGameObject.transform;
                            geometryObject.transform.localPosition = Vector3.zero;
                            geometryObject.name = geometryObject.name.Replace("(Clone)", "");
                            r = geometryObject.GetComponentsInChildren<Renderer>();
                        }

                        foreach (Renderer ren in r)
                        {
                            ren.material = prefabData.finalMat;
                            //Apply collision
                            if (prefabData.setupCollision)
                                ren.gameObject.AddComponent<MeshCollider>().sharedMesh = ren.gameObject.GetComponent<MeshFilter>().sharedMesh;
                        }

                        if (prefabData.setupLODs)
                        {
                            lodsForPrefab.Add(new LOD(lodHeights[0], r));
                            lodHeights.RemoveAt(0);
                        }
                        else
                            break;
                    }
                    //Set LODs in the LOD group
                    if (prefabData.setupLODs)
                    {
                        prefabGameObject.GetComponent<LODGroup>().SetLODs(lodsForPrefab.ToArray());
                        prefabGameObject.GetComponent<LODGroup>().RecalculateBounds();
                    }
                    //Prefab saving
                    string prefLocation = prefabPath + "/" + prefabName + ".prefab";
                    prefLocation = prefLocation.Replace("(Clone)", "");
                    GameObject prefabObject = SavePrefab(prefabGameObject, prefLocation, prefabData.addAssetToScene);
                    if (prefabObject)
                        prefabObjects.Add(prefabObject);
                }

                //Setting up variation holder gameobject
                if (prefabData.addAssetToScene)
                {
                    GameObject scatterParent = new GameObject(prefabData.assetName);
                    scatterParent.isStatic = true;
                    foreach (GameObject variation in prefabObjects)
                    {
                        variation.transform.parent = scatterParent.transform;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Exception::MegascansMeshUtils::3D Asset Prefab:: " + ex.ToString());
                MegascansUtilities.HideProgressBar();
            }
        }

        static GameObject SavePrefab(GameObject prefabGo, string savePath, bool addAssetToScene = false)
        {
            try
            {
                //Set all children objects of the prefab to static
                Transform[] allChildren = prefabGo.GetComponentsInChildren<Transform>();
                foreach (Transform child in allChildren)
                {
                    child.gameObject.isStatic = true;
                }

                GameObject newPrefabObject = prefabGo;
                MegascansUtilities.UpdateProgressBar(1.0f, "Importing Megascans Asset", "Creating Prefab...");
#if UNITY_5 || UNITY_2017 || UNITY_2018
                UnityEngine.Object existingPrefab = AssetDatabase.LoadAssetAtPath(savePath, typeof(UnityEngine.Object));
                if (!existingPrefab)
                    PrefabUtility.CreatePrefab(savePath, prefabGo);
                else
                    PrefabUtility.ReplacePrefab(prefabGo, existingPrefab, ReplacePrefabOptions.ReplaceNameBased);
#else
                PrefabUtility.SaveAsPrefabAsset(prefabGo, savePath);
#endif
                DestroyImmediate(prefabGo);
                if (addAssetToScene)
                {
                    UnityEngine.Object prefabObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(savePath);
                    newPrefabObject = (GameObject)PrefabUtility.InstantiatePrefab(prefabObject);
                    newPrefabObject.isStatic = true;
                }
                return newPrefabObject;
            }
            catch (Exception ex)
            {
                Debug.Log("Exception::MegascansMeshUtils::Saving Prefab:: " + ex.ToString());
                MegascansUtilities.HideProgressBar();
                return null;
            }
        }


        public static bool ContainsLowestLOD(JArray lodList, string minLOD, string activeLOD)
        {
            bool importLODs = EditorPrefs.GetBool("QuixelDefaultImportLODs", true);
            if (importLODs)
            {
                for (int i = 0; i < lodList.Count; i++)
                {
                    JObject meshData = (JObject)lodList[i];
                    if ((string)meshData["lod"] == minLOD)
                        return true;
                }

                return false;
            } else
            {
                return (activeLOD.ToLower() == minLOD.ToLower());
            }
        }
    }

    public struct PrefabData
    {
        public string assetPath;
        public string assetName;
        public string modelNamingConvention;
        public int lodFadeMode;
        public bool highpoly;
        public bool addAssetToScene;
        public bool setupCollision;
        public bool hasBillboardLOD;
        public bool isAlembic;
        public bool isScatterAsset;
        public bool setupLODs;
        public Material finalMat;
        public Material billboardMat;
        public List<string> importedGeometryPaths3D;
        public List<List<string>> importedGeometryPaths3DPlant;

        public PrefabData(string assetPath, string assetName, string modelNamingConvention, int lodFadeMode, bool highpoly, bool addAssetToScene, bool setupCollision, bool hasBillboardLOD, bool isAlembic, bool isScatterAsset, bool setupLODs, Material finalMat, Material billboardMat, List<string> importedGeometryPaths3D, List<List<string>> importedGeometryPaths3DPlant)
        {
            this.assetPath = assetPath;
            this.assetName = assetName;
            this.modelNamingConvention = modelNamingConvention;
            this.lodFadeMode = lodFadeMode;
            this.highpoly = highpoly;
            this.addAssetToScene = addAssetToScene;
            this.setupCollision = setupCollision;
            this.hasBillboardLOD = hasBillboardLOD;
            this.isAlembic = isAlembic;
            this.isScatterAsset = isScatterAsset;
            this.setupLODs = setupLODs;
            this.finalMat = finalMat;
            this.billboardMat = billboardMat;
            this.importedGeometryPaths3D = importedGeometryPaths3D;
            this.importedGeometryPaths3DPlant = importedGeometryPaths3DPlant;
        }

    }

}
#endif