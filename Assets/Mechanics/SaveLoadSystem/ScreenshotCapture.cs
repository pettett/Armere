using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ScreenshotCapture
{
    static Camera camera;

    public static Texture2D CaptureScreenshot(int resWidth, int resHeight)
    {
        camera = Camera.main;
        if (camera == null) throw new System.Exception("Attempting to capture screenshot with no camera");

        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        camera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        MonoBehaviour.Destroy(rt);
        return screenShot;
    }
}
