using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class InteractionPromptSetup
{
    private const string ScenePath = "Assets/Scenes/Zona1_Escuela.unity";
    private const string SpriteFolder = "Assets/UI/InteractionPrompt";
    private const string SpritePath = SpriteFolder + "/RoundedPrompt.png";

    [MenuItem("Mision Aedes/UI/Aplicar indicadores de interaccion")]
    public static void Apply()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Sprite rounded = EnsureRoundedSprite();
        TMP_FontAsset font = StylePrompt(FindUnique("TextoInteraccion"), rounded, "Inspeccionar", 310f);
        StylePrompt(FindUnique("TextoHablar"), rounded, "Hablar", 240f, font);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("INTERACTION_PROMPT_APPLY_OK prompts=2 interactables=13 npcs=3");
    }

    public static void Validate()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject inspect = FindUnique("TextoInteraccion");
        GameObject talk = FindUnique("TextoHablar");
        ValidatePrompt(inspect, "Inspeccionar", new Vector2(310f, 62f));
        ValidatePrompt(talk, "Hablar", new Vector2(240f, 62f));

        ObjetoInteractuable[] interactables = Resources.FindObjectsOfTypeAll<ObjetoInteractuable>()
            .Where(item => item.gameObject.scene.IsValid() && item.gameObject.scene.isLoaded &&
                item.indiceZona >= 1 && item.indiceZona <= 3).ToArray();
        NPCDialogo[] npcs = Resources.FindObjectsOfTypeAll<NPCDialogo>()
            .Where(item => item.gameObject.scene.IsValid() && item.gameObject.scene.isLoaded).ToArray();
        if (interactables.Length != 13 || interactables.Any(item => item.textoInteraccion != inspect))
            throw new InvalidOperationException("Los 13 interactuables no comparten el indicador de inspeccion.");
        if (npcs.Length != 3 || npcs.Any(item => item.textoHablar != talk))
            throw new InvalidOperationException("Los 3 NPC no comparten el indicador de hablar.");

        Collider2D[] colliders = Resources.FindObjectsOfTypeAll<Collider2D>()
            .Where(item => item.gameObject.scene.IsValid() && item.gameObject.scene.isLoaded).ToArray();
        int triggers = colliders.Count(item => item.isTrigger);
        if (colliders.Length != 215 || triggers != 25)
            throw new InvalidOperationException($"Conteo fisico inesperado: {colliders.Length}/{triggers}.");

        Debug.Log("INTERACTION_PROMPT_VALIDATE_OK prompts=2 interactables=13 npcs=3 colliders=215 triggers=25");
    }

    private static TMP_FontAsset StylePrompt(GameObject root, Sprite rounded, string action, float width,
        TMP_FontAsset fallbackFont = null)
    {
        RectTransform rect = root.GetComponent<RectTransform>();
        if (rect == null) throw new InvalidOperationException(root.name + " no tiene RectTransform.");

        TMP_Text oldText = root.GetComponent<TMP_Text>();
        TMP_FontAsset font = oldText != null && oldText.font != null ? oldText.font : fallbackFont;
        if (font == null) throw new InvalidOperationException("No se encontro una fuente TMP para " + root.name);

        foreach (Transform child in rect.Cast<Transform>().ToArray())
            if (child.name.StartsWith("Prompt_", StringComparison.Ordinal))
                UnityEngine.Object.DestroyImmediate(child.gameObject);
        if (oldText != null) UnityEngine.Object.DestroyImmediate(oldText);

        rect.sizeDelta = new Vector2(width, 62f);
        Image border = root.GetComponent<Image>();
        if (border == null) border = root.AddComponent<Image>();
        border.sprite = rounded;
        border.type = Image.Type.Sliced;
        border.color = new Color32(181, 207, 201, 255);
        border.raycastTarget = false;

        Shadow shadow = root.GetComponent<Shadow>();
        if (shadow == null) shadow = root.AddComponent<Shadow>();
        shadow.effectColor = new Color32(5, 12, 15, 150);
        shadow.effectDistance = new Vector2(3f, -3f);
        shadow.useGraphicAlpha = true;

        Image fill = CreateImage("Prompt_Fondo", rect, rounded, new Color32(25, 51, 59, 255));
        SetStretch(fill.rectTransform, 2f);

        RectTransform key = CreateImage("Prompt_Tecla", rect, rounded, new Color32(240, 237, 221, 255)).rectTransform;
        key.anchorMin = key.anchorMax = new Vector2(0.5f, 0.5f);
        key.pivot = new Vector2(0.5f, 0.5f);
        key.sizeDelta = new Vector2(44f, 44f);
        key.anchoredPosition = new Vector2(-width * 0.5f + 33f, 0f);

        TextMeshProUGUI keyLabel = CreateText("Prompt_Letra", key, font, "E", 23f,
            new Color32(24, 49, 56, 255), TextAlignmentOptions.Center);
        SetStretch(keyLabel.rectTransform, 0f);

        float labelLeft = 64f;
        TextMeshProUGUI actionLabel = CreateText("Prompt_Accion", rect, font, action, 23f,
            new Color32(255, 246, 224, 255), TextAlignmentOptions.MidlineLeft);
        actionLabel.fontStyle = FontStyles.Bold;
        actionLabel.textWrappingMode = TextWrappingModes.NoWrap;
        actionLabel.overflowMode = TextOverflowModes.Overflow;
        RectTransform labelRect = actionLabel.rectTransform;
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = new Vector2(labelLeft, 7f);
        labelRect.offsetMax = new Vector2(-14f, -7f);

        root.SetActive(false);
        return font;
    }

    private static void ValidatePrompt(GameObject root, string expectedAction, Vector2 expectedSize)
    {
        RectTransform rect = root.GetComponent<RectTransform>();
        Image border = root.GetComponent<Image>();
        TMP_Text key = root.transform.Find("Prompt_Tecla/Prompt_Letra")?.GetComponent<TMP_Text>();
        TMP_Text action = root.transform.Find("Prompt_Accion")?.GetComponent<TMP_Text>();
        if (rect == null || border == null || border.sprite == null || key == null || action == null ||
            key.text != "E" || action.text != expectedAction || rect.sizeDelta != expectedSize ||
            border.raycastTarget || key.raycastTarget || action.raycastTarget)
            throw new InvalidOperationException("Indicador incompleto o incorrecto: " + root.name);
        action.ForceMeshUpdate();
        if (action.preferredWidth > action.rectTransform.rect.width)
            throw new InvalidOperationException("Texto cortado en " + root.name);
    }

    private static Sprite EnsureRoundedSprite()
    {
        if (!AssetDatabase.IsValidFolder("Assets/UI")) AssetDatabase.CreateFolder("Assets", "UI");
        if (!AssetDatabase.IsValidFolder(SpriteFolder)) AssetDatabase.CreateFolder("Assets/UI", "InteractionPrompt");
        if (!File.Exists(SpritePath))
        {
            const int size = 32;
            const float radius = 7.5f;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = Math.Max(radius - x - 0.5f, Math.Max(0f, x + 0.5f - (size - radius)));
                float dy = Math.Max(radius - y - 0.5f, Math.Max(0f, y + 0.5f - (size - radius)));
                pixels[y * size + x] = dx * dx + dy * dy <= radius * radius
                    ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            File.WriteAllBytes(SpritePath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(SpritePath, ImportAssetOptions.ForceSynchronousImport);
        }

        TextureImporter importer = AssetImporter.GetAtPath(SpritePath) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("No se pudo importar el sprite redondeado.");
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.spriteBorder = new Vector4(8f, 8f, 8f, 8f);
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
    }

    private static Image CreateImage(string name, Transform parent, Sprite sprite, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, TMP_FontAsset font, string value,
        float size, Color color, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = FontStyles.Bold;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }

    private static void SetStretch(RectTransform rect, float inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
    }

    private static GameObject FindUnique(string name)
    {
        GameObject[] matches = Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(go => go.scene.IsValid() && go.scene.isLoaded && go.name == name).ToArray();
        if (matches.Length != 1)
            throw new InvalidOperationException($"Se esperaba un objeto {name} y se encontraron {matches.Length}.");
        return matches[0];
    }
}
