using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthTest : MonoBehaviour
{
    // Start is called before the first frame update

    RenderTexture outputTex;

    public ComputeShader shader;

    public Camera camera;
    void Start()
    {
        if (outputTex == null)
        {
            outputTex = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            outputTex.enableRandomWrite = true;
            outputTex.Create();
        }

        int kernelHandle = shader.FindKernel("CSMain");
        shader.SetTextureFromGlobal(kernelHandle, "_DepthTexture", "_CameraDepthTexture");
        shader.SetTexture(kernelHandle, "_OutputTexture", outputTex);
        shader.Dispatch(kernelHandle, camera.pixelWidth / 32, camera.pixelHeight / 32, 1);

    }

    public int width;
    public int height;

    public Material mat;

    //private void OnRenderImage(RenderTexture source, RenderTexture destination)
    //{
    //    RenderTexture src = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.R8);
    //    //RenderTexture dst = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.R8);
    //    Graphics.Blit(src, destination, mat, 0);


    //    //Graphics.Blit(source,destination, clumpingVoronoiMat);

    //    //RenderTexture rt = Selection.activeObject as RenderTexture;

    //    //RenderTexture.active = dst;
    //    //Texture2D tex = new Texture2D(dst.width, dst.height, TextureFormat.RGB24, false);
    //    //tex.ReadPixels(new Rect(0, 0, dst.width, dst.height), 0, 0);
    //    //RenderTexture.active = null;
    //    RenderTexture.ReleaseTemporary(src);
    //    //byte[] bytes;
    //    //bytes = tex.EncodeToPNG();

    //    //string path = AssetDatabase.GetAssetPath(dst) + ".png";
    //    //System.IO.File.WriteAllBytes(path, bytes);
    //    //AssetDatabase.ImportAsset(path);

    //}

    // Update is called once per frame
    void Update()
    {

    }
}
