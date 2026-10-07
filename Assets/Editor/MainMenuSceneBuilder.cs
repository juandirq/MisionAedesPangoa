#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Genera de forma reproducible la escena de menú sin modificar la escena de gameplay.
/// </summary>
public static class MainMenuSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/MenuPrincipal.unity";
    private const string GameplayPath = "Assets/Scenes/Zona1_Escuela.unity";
    private const string TreePath = "Assets/Imagen de ChatGPT 1 oct 2026, 02_58_53.png";
    private const string ArtFolder = "Assets/UI/MenuPrincipal";

    private static Sprite panelSprite;
    private static Sprite buttonSprite;
    private static Sprite circleSprite;
    private static Sprite trackSprite;
    private static Sprite knobSprite;
    private static TMP_FontAsset font;

    [MenuItem("Misión Aedes/Crear o actualizar menú principal")]
    public static void Build()
    {
        EnsureFolder("Assets/UI");
        EnsureFolder(ArtFolder);
        CreateInterfaceSprites();
        ConfigureTree();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        LoadAssets();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        MainMenuController controller = CreateController();
        Canvas canvas = CreateCanvas();
        CreateEventSystem();
        BuildBackdrop(canvas.transform);
        BuildMenu(canvas.transform, controller);
        BuildFade(canvas.transform, controller);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("Escena de respaldo del menú actualizada (sin cambiar Build Settings): " + ScenePath);
    }

    private static MainMenuController CreateController()
    {
        GameObject go = new GameObject("SistemaMenuPrincipal");
        return go.AddComponent<MainMenuController>();
    }

    private static Canvas CreateCanvas()
    {
        GameObject go = new GameObject("Canvas_MenuPrincipal", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;
        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static void CreateEventSystem()
    {
        GameObject go = new GameObject("EventSystem", typeof(EventSystem));
        InputSystemUIInputModule input = go.AddComponent<InputSystemUIInputModule>();
        input.AssignDefaultActions();
    }

    private static void BuildBackdrop(Transform canvas)
    {
        Image sky = CreateImage("Fondo_Turquesa", canvas, null, Hex("#0B514D"));
        Stretch(sky.rectTransform);

        Image glow = CreateImage("Sol_Tropical", canvas, circleSprite, Hex("#F2C84B66"));
        Place(glow.rectTransform, new Vector2(0.72f, 0.66f), new Vector2(0f, 0f), new Vector2(760f, 760f));

        Image hillBack = CreateImage("Colina_Lejana", canvas, circleSprite, Hex("#23725DBA"));
        Place(hillBack.rectTransform, new Vector2(0.78f, 0.08f), Vector2.zero, new Vector2(1540f, 720f));

        Image hillFront = CreateImage("Colina_Cercana", canvas, circleSprite, Hex("#105641E8"));
        Place(hillFront.rectTransform, new Vector2(0.61f, -0.02f), Vector2.zero, new Vector2(1780f, 600f));

        Image ground = CreateImage("Suelo", canvas, null, Hex("#073C32"));
        RectTransform groundRect = ground.rectTransform;
        groundRect.anchorMin = new Vector2(0f, 0f);
        groundRect.anchorMax = new Vector2(1f, 0f);
        groundRect.pivot = new Vector2(0.5f, 0f);
        groundRect.sizeDelta = new Vector2(0f, 155f);

        Sprite tree = AssetDatabase.LoadAssetAtPath<Sprite>(TreePath);
        Image treeImage = CreateImage("Arte_Arbol_Pangoa", canvas, tree, Color.white);
        treeImage.preserveAspect = true;
        treeImage.raycastTarget = false;
        Place(treeImage.rectTransform, new Vector2(0.73f, 0.48f), new Vector2(50f, -5f), new Vector2(1050f, 1050f));

        Image tag = CreateImage("Etiqueta_Ambiental", canvas, panelSprite, Hex("#083D36E8"), Image.Type.Sliced);
        Place(tag.rectTransform, new Vector2(0.75f, 0f), new Vector2(0f, 70f), new Vector2(575f, 66f));
        CreateText("TextoEtiqueta", tag.transform, "EXPLORA  •  APRENDE  •  PROTEGE",
            27f, Hex("#F8EBC7"), FontStyles.Bold, TextAlignmentOptions.Center, Vector2.zero,
            new Vector2(540f, 56f));
    }

    private static void BuildMenu(Transform canvas, MainMenuController controller)
    {
        Image card = CreateImage("Tarjeta_Principal", canvas, panelSprite, Hex("#062F2BEF"), Image.Type.Sliced);
        Place(card.rectTransform, new Vector2(0.255f, 0.5f), new Vector2(0f, 0f), new Vector2(820f, 970f));
        Shadow shadow = card.gameObject.AddComponent<Shadow>();
        shadow.effectColor = Hex("#001B18A8");
        shadow.effectDistance = new Vector2(14f, -14f);

        Image accent = CreateImage("Linea_Dorada", card.transform, null, Hex("#F2B84B"));
        Place(accent.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(650f, 9f));

        GameObject mainPanel = CreateContainer("PanelPrincipal", card.transform);
        Stretch(mainPanel.GetComponent<RectTransform>());
        controller.panelPrincipal = mainPanel;

        CreateText("Antetitulo", mainPanel.transform, "UNA AVENTURA DE PREVENCIÓN",
            27f, Hex("#88E0B6"), FontStyles.Bold, TextAlignmentOptions.Center,
            new Vector2(0f, 386f), new Vector2(690f, 52f));

        TMP_Text title = CreateText("Titulo", mainPanel.transform, "MISIÓN AEDES",
            78f, Hex("#FFF3CE"), FontStyles.Bold, TextAlignmentOptions.Center,
            new Vector2(0f, 288f), new Vector2(720f, 120f));
        title.outlineColor = Hex("#092D27");
        title.outlineWidth = 0.18f;

        TMP_Text pangoa = CreateText("Subtitulo", mainPanel.transform, "PANGOA",
            67f, Hex("#F4B846"), FontStyles.Bold, TextAlignmentOptions.Center,
            new Vector2(0f, 206f), new Vector2(650f, 90f));
        pangoa.characterSpacing = 7f;

        CreateText("Descripcion", mainPanel.transform,
            "Recorre la comunidad y elimina los criaderos\ndel mosquito antes de que sea tarde.",
            26f, Hex("#D7EEE4"), FontStyles.Normal, TextAlignmentOptions.Center,
            new Vector2(0f, 120f), new Vector2(650f, 78f));

        Button play = CreateButton("Boton_Jugar", mainPanel.transform, "JUGAR", new Vector2(0f, -8f), controller);
        Button options = CreateButton("Boton_Opciones", mainPanel.transform, "OPCIONES", new Vector2(0f, -145f), controller);
        Button quit = CreateButton("Boton_Salir", mainPanel.transform, "SALIR", new Vector2(0f, -282f), controller);
        controller.botonJugar = play;
        controller.botonOpciones = options;
        controller.botonSalir = quit;

        CreateText("Consejo", mainPanel.transform, "Usa el mouse o las flechas para elegir",
            21f, Hex("#91BAAA"), FontStyles.Italic, TextAlignmentOptions.Center,
            new Vector2(0f, -405f), new Vector2(650f, 42f));

        GameObject optionsPanel = CreateContainer("PanelOpciones", card.transform);
        Stretch(optionsPanel.GetComponent<RectTransform>());
        controller.panelOpciones = optionsPanel;

        CreateText("OpcionesTitulo", optionsPanel.transform, "OPCIONES",
            62f, Hex("#FFF3CE"), FontStyles.Bold, TextAlignmentOptions.Center,
            new Vector2(0f, 350f), new Vector2(680f, 90f));
        CreateText("OpcionesBajada", optionsPanel.transform,
            "Ajusta el sonido a tu manera. Tus cambios se guardan automáticamente.",
            24f, Hex("#BFE0D2"), FontStyles.Normal, TextAlignmentOptions.Center,
            new Vector2(0f, 280f), new Vector2(650f, 70f));

        Slider musicSlider = CreateSlider("Slider_Musica", optionsPanel.transform, new Vector2(-30f, 100f));
        TMP_Text musicValue = CreateOptionLabel(optionsPanel.transform, "MÚSICA", new Vector2(0f, 178f));
        TMP_Text musicNumber = CreateValueLabel(optionsPanel.transform, "75%", new Vector2(276f, 100f));

        Slider sfxSlider = CreateSlider("Slider_SFX", optionsPanel.transform, new Vector2(-30f, -105f));
        TMP_Text sfxValue = CreateOptionLabel(optionsPanel.transform, "EFECTOS / SFX", new Vector2(0f, -27f));
        TMP_Text sfxNumber = CreateValueLabel(optionsPanel.transform, "85%", new Vector2(276f, -105f));
        musicValue.raycastTarget = false;
        sfxValue.raycastTarget = false;

        Button back = CreateButton("Boton_Volver", optionsPanel.transform, "VOLVER", new Vector2(0f, -310f), controller);
        controller.botonVolver = back;
        controller.sliderMusica = musicSlider;
        controller.sliderSfx = sfxSlider;
        controller.valorMusica = musicNumber;
        controller.valorSfx = sfxNumber;

        optionsPanel.SetActive(false);
    }

    private static void BuildFade(Transform canvas, MainMenuController controller)
    {
        Image image = CreateImage("Fundido", canvas, null, Color.black);
        Stretch(image.rectTransform);
        CanvasGroup group = image.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        controller.grupoMenu = group;
    }

    private static Button CreateButton(string name, Transform parent, string label,
        Vector2 position, MainMenuController controller)
    {
        Image image = CreateImage(name, parent, buttonSprite, Color.white, Image.Type.Sliced);
        Place(image.rectTransform, new Vector2(0.5f, 0.5f), position, new Vector2(555f, 108f));
        Shadow shadow = image.gameObject.AddComponent<Shadow>();
        shadow.effectColor = Hex("#001A16B5");
        shadow.effectDistance = new Vector2(7f, -8f);

        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = Hex("#FFF0C9");
        colors.highlightedColor = Hex("#FFD469");
        colors.pressedColor = Hex("#C87A35");
        colors.selectedColor = Hex("#FFE090");
        colors.disabledColor = Hex("#8F9A83A0");
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        TMP_Text text = CreateText("Texto", button.transform, label, 38f, Hex("#3A281B"),
            FontStyles.Bold, TextAlignmentOptions.Center, Vector2.zero, new Vector2(515f, 90f));
        text.characterSpacing = 3f;
        text.raycastTarget = false;

        MenuButtonFeedback feedback = button.gameObject.AddComponent<MenuButtonFeedback>();
        feedback.menu = controller;
        return button;
    }

    private static Slider CreateSlider(string name, Transform parent, Vector2 position)
    {
        GameObject root = CreateContainer(name, parent);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        Place(rootRect, new Vector2(0.5f, 0.5f), position, new Vector2(500f, 58f));

        Image background = CreateImage("Fondo", root.transform, trackSprite, Hex("#123F38"), Image.Type.Sliced);
        Stretch(background.rectTransform, new Vector2(0f, 17f), new Vector2(0f, -17f));
        background.raycastTarget = false;

        GameObject fillArea = CreateContainer("AreaRelleno", root.transform);
        Stretch(fillArea.GetComponent<RectTransform>(), new Vector2(15f, 19f), new Vector2(-15f, -19f));
        Image fill = CreateImage("Relleno", fillArea.transform, trackSprite, Hex("#67D39A"), Image.Type.Sliced);
        Stretch(fill.rectTransform);
        fill.raycastTarget = false;

        GameObject handleArea = CreateContainer("AreaControl", root.transform);
        Stretch(handleArea.GetComponent<RectTransform>(), new Vector2(20f, 0f), new Vector2(-20f, 0f));
        Image handle = CreateImage("Control", handleArea.transform, knobSprite, Hex("#FFD15A"));
        Place(handle.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(52f, 52f));

        Slider slider = root.AddComponent<Slider>();
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.75f;
        slider.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = slider.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Hex("#FFF2A8");
        colors.pressedColor = Hex("#E79B38");
        colors.selectedColor = Hex("#FFF2A8");
        colors.fadeDuration = 0.08f;
        slider.colors = colors;
        return slider;
    }

    private static TMP_Text CreateOptionLabel(Transform parent, string text, Vector2 position)
    {
        return CreateText("Etiqueta_" + text, parent, text, 28f, Hex("#FFF0C9"), FontStyles.Bold,
            TextAlignmentOptions.Left, position + new Vector2(-30f, 0f), new Vector2(500f, 48f));
    }

    private static TMP_Text CreateValueLabel(Transform parent, string text, Vector2 position)
    {
        Image badge = CreateImage("Valor_" + text, parent, panelSprite, Hex("#0D4D43"), Image.Type.Sliced);
        Place(badge.rectTransform, new Vector2(0.5f, 0.5f), position, new Vector2(115f, 58f));
        return CreateText("Texto", badge.transform, text, 25f, Hex("#FFF0C9"), FontStyles.Bold,
            TextAlignmentOptions.Center, Vector2.zero, new Vector2(105f, 50f));
    }

    private static TMP_Text CreateText(string name, Transform parent, string value, float size,
        Color color, FontStyles style, TextAlignmentOptions alignment, Vector2 position, Vector2 dimensions)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.fontStyle = style;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Overflow;
        Place(text.rectTransform, new Vector2(0.5f, 0.5f), position, dimensions);
        return text;
    }

    private static Image CreateImage(string name, Transform parent, Sprite sprite, Color color,
        Image.Type type = Image.Type.Simple)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.type = sprite != null ? type : Image.Type.Simple;
        return image;
    }

    private static GameObject CreateContainer(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect, Vector2? minOffset = null, Vector2? maxOffset = null)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = minOffset ?? Vector2.zero;
        rect.offsetMax = maxOffset ?? Vector2.zero;
    }

    private static void ConfigureBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes
            .Where(s => s.path != ScenePath)
            .ToList();
        scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));

        if (scenes.All(s => s.path != GameplayPath))
            scenes.Add(new EditorBuildSettingsScene(GameplayPath, true));

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void ConfigureTree()
    {
        TextureImporter importer = AssetImporter.GetAtPath(TreePath) as TextureImporter;
        if (importer == null) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
        importer.SaveAndReimport();
    }

    private static void LoadAssets()
    {
        panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + "/PanelPixel.png");
        buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + "/BotonMadera.png");
        circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + "/Circulo.png");
        trackSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + "/Barra.png");
        knobSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + "/Control.png");
        font = TMP_Settings.defaultFontAsset;
        if (font == null)
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
    }

    private static void CreateInterfaceSprites()
    {
        WriteRoundedSprite(ArtFolder + "/PanelPixel.png", 64, 64, 10, 3,
            new Color32(255, 255, 255, 255), new Color32(109, 183, 139, 255), false);
        WriteRoundedSprite(ArtFolder + "/BotonMadera.png", 96, 48, 9, 5,
            new Color32(255, 242, 204, 255), new Color32(101, 57, 30, 255), true);
        WriteRoundedSprite(ArtFolder + "/Barra.png", 64, 20, 8, 2,
            new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 255), false);
        WriteCircleSprite(ArtFolder + "/Circulo.png", 64, false);
        WriteCircleSprite(ArtFolder + "/Control.png", 64, true);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        ConfigureSprite(ArtFolder + "/PanelPixel.png", new Vector4(12f, 12f, 12f, 12f));
        ConfigureSprite(ArtFolder + "/BotonMadera.png", new Vector4(13f, 13f, 13f, 13f));
        ConfigureSprite(ArtFolder + "/Barra.png", new Vector4(9f, 9f, 9f, 9f));
        ConfigureSprite(ArtFolder + "/Circulo.png", Vector4.zero);
        ConfigureSprite(ArtFolder + "/Control.png", Vector4.zero);
    }

    private static void WriteRoundedSprite(string assetPath, int width, int height, int radius,
        int border, Color32 fill, Color32 edge, bool bevel)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color32 clear = new Color32(0, 0, 0, 0);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool inside = IsInsideRounded(x, y, width, height, radius);
                bool inner = IsInsideRounded(x - border, y - border,
                    width - border * 2, height - border * 2, Mathf.Max(1, radius - border));
                Color32 pixel = !inside ? clear : (!inner ? edge : fill);
                if (bevel && inner)
                {
                    if (y >= height - border * 2) pixel = new Color32(255, 251, 224, 255);
                    else if (y < border * 2) pixel = new Color32(202, 150, 79, 255);
                }
                texture.SetPixel(x, y, pixel);
            }
        }
        texture.Apply();
        File.WriteAllBytes(Absolute(assetPath), texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
    }

    private static bool IsInsideRounded(int x, int y, int width, int height, int radius)
    {
        if (width <= 0 || height <= 0 || x < 0 || y < 0 || x >= width || y >= height) return false;
        if ((x >= radius && x < width - radius) || (y >= radius && y < height - radius)) return true;
        int cx = x < radius ? radius : width - radius - 1;
        int cy = y < radius ? radius : height - radius - 1;
        int dx = x - cx;
        int dy = y - cy;
        return dx * dx + dy * dy <= radius * radius;
    }

    private static void WriteCircleSprite(string assetPath, int size, bool ring)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = (size - 1) * 0.5f;
        float radius = center - 1f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (distance > radius)
                    texture.SetPixel(x, y, Color.clear);
                else if (ring && distance > radius - 5f)
                    texture.SetPixel(x, y, new Color32(87, 52, 24, 255));
                else
                    texture.SetPixel(x, y, Color.white);
            }
        }
        texture.Apply();
        File.WriteAllBytes(Absolute(assetPath), texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
    }

    private static void ConfigureSprite(string path, Vector4 border)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spriteBorder = border;
        importer.SaveAndReimport();
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string name = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    private static string Absolute(string assetPath)
    {
        return Path.Combine(Directory.GetParent(Application.dataPath).FullName,
            assetPath.Replace('/', Path.DirectorySeparatorChar));
    }

    private static Color Hex(string value)
    {
        ColorUtility.TryParseHtmlString(value, out Color color);
        return color;
    }
}
#endif
