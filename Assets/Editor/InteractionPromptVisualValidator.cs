using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public static class InteractionPromptVisualValidator
{
    private const string ScenePath = "Assets/Scenes/Zona1_Escuela.unity";
    private const string OutputPath = "Logs/InteractionPrompt/Prompt_1366x768.png";

    public static void Run()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));

        GameObject cameraGo = new GameObject("PromptValidationCamera");
        Camera camera = cameraGo.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(44, 73, 66, 255);
        camera.cullingMask = 1 << 30;
        camera.transform.position = new Vector3(0f, 0f, -10f);

        GameObject canvasGo = new GameObject("PromptValidationCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasGo.layer = 30;
        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1f;
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1366f, 768f);

        ClonePrompt("TextoInteraccion", canvasGo.transform, new Vector2(0f, 42f));
        ClonePrompt("TextoHablar", canvasGo.transform, new Vector2(0f, -42f));

        RenderTexture target = new RenderTexture(1366, 768, 0, RenderTextureFormat.ARGB32);
        target.filterMode = FilterMode.Point;
        camera.targetTexture = target;
        Canvas.ForceUpdateCanvases();
        camera.Render();

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = target;
        Texture2D image = new Texture2D(1366, 768, TextureFormat.RGBA32, false);
        image.ReadPixels(new Rect(0, 0, 1366, 768), 0, 0);
        image.Apply(false, false);
        File.WriteAllBytes(OutputPath, image.EncodeToPNG());
        RenderTexture.active = previous;

        UnityEngine.Object.DestroyImmediate(image);
        UnityEngine.Object.DestroyImmediate(target);
        UnityEngine.Object.DestroyImmediate(canvasGo);
        UnityEngine.Object.DestroyImmediate(cameraGo);
        Debug.Log("INTERACTION_PROMPT_VISUAL_OK resolution=1366x768 prompts=2 capture=" + OutputPath);
    }

    private static void ClonePrompt(string name, Transform parent, Vector2 position)
    {
        GameObject[] matches = Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(go => go.scene.IsValid() && go.scene.isLoaded && go.name == name).ToArray();
        if (matches.Length != 1) throw new InvalidOperationException($"Se esperaba un {name} y se encontraron {matches.Length}.");
        GameObject source = matches[0];
        GameObject clone = UnityEngine.Object.Instantiate(source, parent);
        clone.name = "Preview_" + name;
        SetLayerRecursively(clone, 30);
        clone.SetActive(true);
        RectTransform rect = clone.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.localScale = Vector3.one;
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform) SetLayerRecursively(child.gameObject, layer);
    }
}
