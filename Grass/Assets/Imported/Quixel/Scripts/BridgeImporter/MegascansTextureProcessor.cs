#if UNITY_EDITOR

using UnityEngine;
using System.IO;
using UnityEditor;
using System;

namespace Quixel
{
    public class MegascansTextureProcessor : MonoBehaviour
    {
        string sourcePath;
        string destPath;
        bool normalMap;
        bool sRGB;

        public MegascansTextureProcessor(string sourcePath, string destPath, bool normalMap = false, bool sRGB = true)
        {
            this.sourcePath = sourcePath;
            this.destPath = destPath;
            this.normalMap = normalMap;
            this.sRGB = sRGB;
        }

        public Texture2D ImportTexture()
        {
            MegascansUtilities.CopyFileToProject(sourcePath, destPath);
            TextureImporter tImp = AssetImporter.GetAtPath(destPath) as TextureImporter;
            int importResolution = Convert.ToInt32(Math.Pow(2, 9 + EditorPrefs.GetInt("QuixelDefaultImportResolution", 4)));
            tImp.maxTextureSize = importResolution;
            tImp.sRGBTexture = sRGB;
            tImp.textureType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
            AssetDatabase.ImportAsset(destPath);
            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(destPath);
        }

        public void AdjustAlphaCutoff(float alphaCutoff = 0.33f, bool alphaIsTransparency = true, bool mipMapsPreserveCoverage = true)
        {
            TextureImporter tImp = AssetImporter.GetAtPath(destPath) as TextureImporter;
            tImp.mipMapsPreserveCoverage = mipMapsPreserveCoverage;
            tImp.alphaIsTransparency = alphaIsTransparency;
            tImp.alphaTestReferenceValue = alphaCutoff;
            AssetDatabase.ImportAsset(destPath);
            AssetDatabase.Refresh();
            AssetDatabase.LoadAssetAtPath<Texture2D>(destPath);
        }
    }
}
#endif