using System.Collections;
using System.IO;
using UnityEngine;

public static class ScreenShooter
{
    public static IEnumerator CaptureScreen(string Path, int width = 0, int height = 0)
    {
        yield return new WaitForEndOfFrame();

        int w = (width > 0) ? width : Screen.width;
        int h = (height > 0) ? height : Screen.height;

        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();

        WritePNG(tex, Path);
        Object.Destroy(tex);
    }

    // remove the UI layer
    // use chosen camera to Texture2D to ignore UIs.
    // Destory should excecute by the function that call this method
    public static Texture2D CaptureCameraToTexture(Camera cam, int width, int height)
    {
        var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        var prevRT = RenderTexture.active;
        var prevCamRT = cam.targetTexture;

        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        cam.targetTexture = prevCamRT;
        RenderTexture.active = prevRT;
        rt.Release();
        Object.Destroy(rt);

        return tex;
    }

    public static void WritePNG(Texture2D tex, string fullPath)
    {
        byte[] png = tex.EncodeToPNG();
        EnsureDirectory(fullPath);
        File.WriteAllBytes(fullPath, png);
    }

    private static void EnsureDirectory(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath)) return;
        string dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
}
