using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RiverWaterVisualValidator
{
    private const string ScenePath = "Assets/Scenes/Zona1_Escuela.unity";
    private const string OutputFolder = "Logs/RiverWaterVisual";
    private static readonly string[] RiverNames = { "RIO_1", "RIO_2", "RIO_3", "RIO_4", "RIO_5" };

    public static void Run()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Directory.CreateDirectory(OutputFolder);

        Material waterMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/RiverWaterSubtle.mat");
        Shader baselineShader = Shader.Find("Sprites/Default");
        if (waterMaterial == null || baselineShader == null)
            throw new InvalidOperationException("Faltan materiales para la validacion visual.");

        Material baselineMaterial = new Material(baselineShader);
        GameObject cameraObject = new GameObject("RiverVisualValidationCamera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.clear;
        camera.cullingMask = 1 << 31;
        camera.transform.position = new Vector3(0f, 0f, -10f);

        RenderTexture target = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32)
        {
            filterMode = FilterMode.Point,
            antiAliasing = 1
        };
        camera.targetTexture = target;

        int totalChanged = 0;
        int totalSolidChanged = 0;
        foreach (string riverName in RiverNames)
        {
            SpriteRenderer source = GameObject.Find(riverName)?.GetComponent<SpriteRenderer>();
            if (source == null || source.sprite == null) throw new InvalidOperationException("No se encontro " + riverName);

            GameObject sampleObject = new GameObject("Sample_" + riverName);
            sampleObject.layer = 31;
            SpriteRenderer sample = sampleObject.AddComponent<SpriteRenderer>();
            sample.sprite = source.sprite;
            sample.color = Color.white;
            sample.sharedMaterial = waterMaterial;
            RiverWaterAnimator profile = source.GetComponent<RiverWaterAnimator>();
            if (profile == null) throw new InvalidOperationException("Falta perfil de cascada en " + riverName);

            Bounds bounds = sample.bounds;
            camera.orthographicSize = Math.Max(bounds.extents.y, bounds.extents.x) * 1.08f;

            sample.sharedMaterial = baselineMaterial;
            Color32[] baseline = Capture(camera, target, sample, profile, 0f, riverName + "_base.png");
            sample.sharedMaterial = waterMaterial;
            Color32[] atZero = Capture(camera, target, sample, profile, 0f, riverName + "_t0.png");
            Color32[] atOne = Capture(camera, target, sample, profile, 1f, riverName + "_t1.png");
            Color32[] atThree = Capture(camera, target, sample, profile, 3f, riverName + "_t3.png");

            int changed = CountChanged(atZero, atOne, 3) + CountChanged(atOne, atThree, 3);
            int solidChanged = CountChangedOutsideWater(baseline, atZero, atOne, atThree, 3);
            if (changed < 500)
                throw new InvalidOperationException(riverName + " no presenta suficientes pixeles animados: " + changed);
            if (solidChanged != 0)
                throw new InvalidOperationException(riverName + " altero pixeles no acuaticos: " + solidChanged);

            totalChanged += changed;
            totalSolidChanged += solidChanged;
            UnityEngine.Object.DestroyImmediate(sampleObject);
        }

        UnityEngine.Object.DestroyImmediate(baselineMaterial);
        UnityEngine.Object.DestroyImmediate(target);
        UnityEngine.Object.DestroyImmediate(cameraObject);
        Debug.Log($"RIVER_WATER_VISUAL_OK rivers=5 changedPixels={totalChanged} solidChanged={totalSolidChanged} captures=20");
    }

    private static Color32[] Capture(Camera camera, RenderTexture target, SpriteRenderer renderer,
        RiverWaterAnimator profile, float time, string fileName)
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(block);
        block.SetFloat("_WaterTime", time);
        block.SetFloat("_WaterPhase", 0f);
        block.SetFloat("_FallCount", profile.CantidadCascadas);
        block.SetVector("_FallRect1", profile.Caida1);
        block.SetVector("_FallRect2", profile.Caida2);
        block.SetVector("_FallRect3", profile.Caida3);
        block.SetVector("_BaseRect1", profile.Base1);
        block.SetVector("_BaseRect2", profile.Base2);
        block.SetVector("_BaseRect3", profile.Base3);
        renderer.SetPropertyBlock(block);

        RenderTexture previous = RenderTexture.active;
        camera.Render();
        RenderTexture.active = target;
        Texture2D image = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
        image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
        image.Apply(false, false);
        Color32[] pixels = image.GetPixels32();
        File.WriteAllBytes(Path.Combine(OutputFolder, fileName), image.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(image);
        RenderTexture.active = previous;
        return pixels;
    }

    private static int CountChanged(Color32[] first, Color32[] second, int tolerance)
    {
        int count = 0;
        for (int i = 0; i < first.Length; i++)
            if (Difference(first[i], second[i]) > tolerance) count++;
        return count;
    }

    private static int CountChangedOutsideWater(Color32[] baseline, Color32[] zero, Color32[] one,
        Color32[] three, int tolerance)
    {
        int count = 0;
        for (int i = 0; i < baseline.Length; i++)
        {
            Color32 color = baseline[i];
            if (color.a < 8) continue;
            bool water = color.b > color.r + 9 && color.b + 41 > color.g && Math.Max(color.g, color.b) > color.r + 7;
            if (!water && (Difference(zero[i], one[i]) > tolerance || Difference(one[i], three[i]) > tolerance)) count++;
        }
        return count;
    }

    private static int Difference(Color32 a, Color32 b)
    {
        return Math.Max(Math.Abs(a.r - b.r), Math.Max(Math.Abs(a.g - b.g), Math.Abs(a.b - b.b)));
    }
}
