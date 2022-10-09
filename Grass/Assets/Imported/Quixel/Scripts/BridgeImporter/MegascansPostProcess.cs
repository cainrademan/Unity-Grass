#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Quixel
{
    public class MegascansPostProcess : AssetPostprocessor
    {

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // if(!MegascansUtilities.isLegacy())
            // {
            //     Debug.Log("Automatically changing API Compatibility Level.");
            //     PlayerSettings.SetApiCompatibilityLevel(EditorUserBuildSettings.selectedBuildTargetGroup, ApiCompatibilityLevel.NET_4_6);
            // }

            // loop through imported files, see if it's a .qxl file.
            for (int i = 0; i < importedAssets.Length; ++i)
            {
                if (importedAssets[i].Contains("MegascansImporterWindow.cs"))
                {
                    MegascansImporterWindow.Init();
                }
            }
        }
    }
}
#endif