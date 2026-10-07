#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Integra de forma idempotente el menú principal dentro de Zona1_Escuela.
/// No modifica mapa, colliders, sorting ni progresión.
/// </summary>
public static class MainMenuIntegrationTool
{
    private const string ScenePath = "Assets/Scenes/Zona1_Escuela.unity";
    private const string VisualFolder = "Assets/Sprites/FONDO";
    private const string UiFolder = "Assets/UI/MenuPrincipal";
    private const string ButtonArtPath = UiFolder + "/BotonMaderaIntegrado.png";
    private const string PanelArtPath = UiFolder + "/PanelOpcionesIntegrado.png";

    private static TMP_FontAsset font;
    private static Sprite buttonSprite;
    private static Sprite panelSprite;
    private static Sprite trackSprite;
    private static Sprite knobSprite;

    [MenuItem("Misión Aedes/Integrar menú principal en Zona1_Escuela")]
    public static void Integrate()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string logoPath = FindVisualAsset(true);
        string backgroundPath = FindVisualAsset(false);
        if (string.IsNullOrEmpty(logoPath) || string.IsNullOrEmpty(backgroundPath))
            throw new InvalidOperationException("No se encontraron LOGO y fondo tropical en " + VisualFolder);

        ConfigureSprite(logoPath, Vector4.zero, false);
        ConfigureSprite(backgroundPath, Vector4.zero, false);
        CreateUiArt();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        LoadUiAssets();

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        AudioManager audio = UnityEngine.Object.FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include);
        GameManager game = UnityEngine.Object.FindAnyObjectByType<GameManager>(FindObjectsInactive.Include);
        PlayerMovement player = UnityEngine.Object.FindAnyObjectByType<PlayerMovement>(FindObjectsInactive.Include);
        Camera mainCamera = Camera.main;

        Require(audio, "AudioManager");
        Require(game, "GameManager");
        Require(player, "PlayerMovement");
        Require(mainCamera, "Main Camera");
        if (mainCamera.GetComponent<AudioListener>() == null)
            throw new InvalidOperationException("Main Camera no tiene AudioListener.");

        GameObject oldCanvas = GameObject.Find("CanvasMenuPrincipal");
        if (oldCanvas != null) UnityEngine.Object.DestroyImmediate(oldCanvas);

        Sprite logo = AssetDatabase.LoadAssetAtPath<Sprite>(logoPath);
        Sprite background = AssetDatabase.LoadAssetAtPath<Sprite>(backgroundPath);
        MainMenuController controller = BuildMenuCanvas(logo, background, audio, game, player);
        MenuPausaUI pause = BuildPauseMenu(game, player);

        // El bloqueo existe ya en el archivo de escena, antes incluso del primer Update.
        player.enabled = false;
        if (game.panelHUDIzquierdo != null) game.panelHUDIzquierdo.SetActive(false);
        if (game.panelHUDDerecho != null) game.panelHUDDerecho.SetActive(false);
        if (game.textoInteraccion != null) game.textoInteraccion.SetActive(false);

        ConfigureBuildSettings();
        Validate(scene, controller, pause, audio, game, player, mainCamera);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("Menú principal integrado y validado en Zona1_Escuela. Build index 0 restaurado.");
    }

    private static MainMenuController BuildMenuCanvas(Sprite logo, Sprite background,
        AudioManager audio, GameManager game, PlayerMovement player)
    {
        GameObject canvasGo = new GameObject("CanvasMenuPrincipal", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject root = Container("MenuPrincipal", canvasGo.transform);
        Stretch(root.GetComponent<RectTransform>());
        CanvasGroup group = root.AddComponent<CanvasGroup>();

        Image backgroundImage = ImageObject("FondoMenu", root.transform, background, Color.white);
        RectTransform backgroundRect = backgroundImage.rectTransform;
        Center(backgroundRect, Vector2.zero, new Vector2(1920f, 1440f));
        AspectRatioFitter fitter = backgroundImage.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = background != null && background.rect.height > 0f
            ? background.rect.width / background.rect.height
            : 4f / 3f;

        GameObject mainPanel = Container("PanelPrincipal", root.transform);
        Stretch(mainPanel.GetComponent<RectTransform>());

        Image logoImage = ImageObject("LogoMenu", mainPanel.transform, logo, Color.white);
        logoImage.preserveAspect = true;
        logoImage.raycastTarget = false;
        Center(logoImage.rectTransform, new Vector2(0f, 270f), new Vector2(760f, 570f));

        Image readability = ImageObject("OverlayLegibilidad", mainPanel.transform, panelSprite,
            new Color(0.025f, 0.12f, 0.09f, 0.74f), Image.Type.Sliced);
        Center(readability.rectTransform, new Vector2(0f, -208f), new Vector2(600f, 445f));
        Shadow readabilityShadow = readability.gameObject.AddComponent<Shadow>();
        readabilityShadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
        readabilityShadow.effectDistance = new Vector2(10f, -10f);

        GameObject buttons = Container("ContenedorBotones", mainPanel.transform);
        Center(buttons.GetComponent<RectTransform>(), new Vector2(0f, -205f), new Vector2(520f, 410f));

        MainMenuController controller = root.AddComponent<MainMenuController>();
        Button play = MenuButton("BotonJugar", buttons.transform, "JUGAR", new Vector2(0f, 125f), controller);
        Button options = MenuButton("BotonOpciones", buttons.transform, "OPCIONES", Vector2.zero, controller);
        Button quit = MenuButton("BotonSalir", buttons.transform, "SALIR", new Vector2(0f, -125f), controller);

        GameObject optionsPanel = BuildOptionsPanel(root.transform, controller, out Button back,
            out Slider musicSlider, out Slider sfxSlider, out TMP_Text musicValue, out TMP_Text sfxValue);

        controller.raizMenu = root;
        controller.panelPrincipal = mainPanel;
        controller.panelOpciones = optionsPanel;
        controller.grupoMenu = group;
        controller.botonJugar = play;
        controller.botonOpciones = options;
        controller.botonSalir = quit;
        controller.botonVolver = back;
        controller.sliderMusica = musicSlider;
        controller.sliderSfx = sfxSlider;
        controller.valorMusica = musicValue;
        controller.valorSfx = sfxValue;
        controller.audioManager = audio;
        controller.gameManager = game;
        controller.playerMovement = player;
        optionsPanel.SetActive(false);
        return controller;
    }

    private static GameObject BuildOptionsPanel(Transform parent, MainMenuController controller,
        out Button back, out Slider musicSlider, out Slider sfxSlider,
        out TMP_Text musicValue, out TMP_Text sfxValue)
    {
        Image card = ImageObject("PanelOpciones", parent, panelSprite, Color.white, Image.Type.Sliced);
        Center(card.rectTransform, Vector2.zero, new Vector2(700f, 590f));
        Shadow shadow = card.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
        shadow.effectDistance = new Vector2(12f, -12f);

        Text("TituloOpciones", card.transform, "OPCIONES", 52f, new Color32(255, 216, 85, 255),
            FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0f, 218f), new Vector2(560f, 70f));

        Text("TextoMusica", card.transform, "MÚSICA", 29f, Color.white,
            FontStyles.Bold, TextAlignmentOptions.Left, new Vector2(-15f, 115f), new Vector2(520f, 42f));
        musicSlider = SliderObject("SliderMusica", card.transform, new Vector2(-35f, 55f));
        musicValue = ValueBadge("ValorMusica", card.transform, "75%", new Vector2(280f, 55f));

        Text("TextoSFX", card.transform, "EFECTOS", 29f, Color.white,
            FontStyles.Bold, TextAlignmentOptions.Left, new Vector2(-15f, -48f), new Vector2(520f, 42f));
        sfxSlider = SliderObject("SliderSFX", card.transform, new Vector2(-35f, -108f));
        sfxValue = ValueBadge("ValorSFX", card.transform, "85%", new Vector2(280f, -108f));

        back = MenuButton("BotonVolver", card.transform, "VOLVER", new Vector2(0f, -225f), controller);
        Center(back.GetComponent<RectTransform>(), new Vector2(0f, -225f), new Vector2(390f, 88f));
        return card.gameObject;
    }

    private static MenuPausaUI BuildPauseMenu(GameManager game, PlayerMovement player)
    {
        Canvas gameplayCanvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        Require(gameplayCanvas, "Canvas de gameplay");

        Transform old = gameplayCanvas.transform.Find("SistemaPausa");
        if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);

        GameObject system = Container("SistemaPausa", gameplayCanvas.transform);
        Stretch(system.GetComponent<RectTransform>());
        MenuPausaUI pause = system.AddComponent<MenuPausaUI>();
        pause.gameManager = game;
        pause.playerMovement = player;
        pause.otrosPanelesModales = new[] { game.panelDerrota, game.panelVictoria, game.ventanaDecision };

        Image backdrop = ImageObject("PanelPausa", system.transform, null, new Color(0f, 0f, 0f, 0.62f));
        Stretch(backdrop.rectTransform);
        Image card = ImageObject("TarjetaPausa", backdrop.transform, panelSprite, Color.white, Image.Type.Sliced);
        Center(card.rectTransform, Vector2.zero, new Vector2(610f, 560f));
        Shadow shadow = card.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
        shadow.effectDistance = new Vector2(12f, -12f);

        Text("TituloPausa", card.transform, "PAUSA", 58f, new Color32(255, 216, 85, 255),
            FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0f, 195f), new Vector2(500f, 76f));
        Button resume = PauseButton("BotonReanudar", card.transform, "REANUDAR", new Vector2(0f, 75f));
        Button restart = PauseButton("BotonReiniciar", card.transform, "REINICIAR", new Vector2(0f, -55f));
        Button exit = PauseButton("BotonSalirPausa", card.transform, "SALIR", new Vector2(0f, -185f));
        UnityEventTools.AddPersistentListener(resume.onClick, pause.Reanudar);
        UnityEventTools.AddPersistentListener(restart.onClick, pause.ReiniciarZona);
        UnityEventTools.AddPersistentListener(exit.onClick, pause.SalirDelJuego);

        pause.panelPausa = backdrop.gameObject;
        backdrop.gameObject.SetActive(false);
        return pause;
    }

    private static Button PauseButton(string name, Transform parent, string label, Vector2 position)
    {
        Image image = ImageObject(name, parent, buttonSprite, Color.white, Image.Type.Sliced);
        Center(image.rectTransform, position, new Vector2(410f, 94f));
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.90f, 0.62f, 1f);
        colors.pressedColor = new Color(0.76f, 0.58f, 0.38f, 1f);
        colors.selectedColor = new Color(1f, 0.90f, 0.62f, 1f);
        colors.fadeDuration = 0.07f;
        button.colors = colors;
        TMP_Text text = Text("Texto", button.transform, label, 34f, new Color32(255, 248, 218, 255),
            FontStyles.Bold, TextAlignmentOptions.Center, Vector2.zero, new Vector2(380f, 74f));
        text.outlineColor = new Color32(49, 22, 8, 255);
        text.outlineWidth = 0.16f;
        text.raycastTarget = false;
        button.gameObject.AddComponent<MenuButtonFeedback>();
        button.gameObject.AddComponent<AudioBotonSFX>();
        return button;
    }

    private static Button MenuButton(string name, Transform parent, string label,
        Vector2 position, MainMenuController controller)
    {
        Image image = ImageObject(name, parent, buttonSprite, Color.white, Image.Type.Sliced);
        Center(image.rectTransform, position, new Vector2(455f, 96f));
        Shadow shadow = image.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0.05f, 0.02f, 0f, 0.72f);
        shadow.effectDistance = new Vector2(7f, -8f);

        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.90f, 0.62f, 1f);
        colors.pressedColor = new Color(0.76f, 0.58f, 0.38f, 1f);
        colors.selectedColor = new Color(1f, 0.90f, 0.62f, 1f);
        colors.disabledColor = new Color(0.45f, 0.4f, 0.33f, 0.75f);
        colors.fadeDuration = 0.07f;
        button.colors = colors;

        TMP_Text text = Text("Texto", button.transform, label, 37f, new Color32(255, 248, 218, 255),
            FontStyles.Bold, TextAlignmentOptions.Center, Vector2.zero, new Vector2(420f, 76f));
        text.characterSpacing = 3f;
        text.outlineColor = new Color32(49, 22, 8, 255);
        text.outlineWidth = 0.16f;
        text.raycastTarget = false;

        MenuButtonFeedback feedback = button.gameObject.AddComponent<MenuButtonFeedback>();
        feedback.menu = controller;
        feedback.escalaHover = 1.04f;
        feedback.escalaPresionada = 0.97f;
        return button;
    }

    private static Slider SliderObject(string name, Transform parent, Vector2 position)
    {
        GameObject root = Container(name, parent);
        Center(root.GetComponent<RectTransform>(), position, new Vector2(500f, 56f));

        Image background = ImageObject("Fondo", root.transform, trackSprite,
            new Color32(35, 31, 22, 255), Image.Type.Sliced);
        Stretch(background.rectTransform, new Vector2(0f, 18f), new Vector2(0f, -18f));
        background.raycastTarget = false;

        GameObject fillArea = Container("AreaRelleno", root.transform);
        Stretch(fillArea.GetComponent<RectTransform>(), new Vector2(14f, 20f), new Vector2(-14f, -20f));
        Image fill = ImageObject("Relleno", fillArea.transform, trackSprite,
            new Color32(255, 193, 54, 255), Image.Type.Sliced);
        Stretch(fill.rectTransform);
        fill.raycastTarget = false;

        GameObject handleArea = Container("AreaControl", root.transform);
        Stretch(handleArea.GetComponent<RectTransform>(), new Vector2(20f, 0f), new Vector2(-20f, 0f));
        Image handle = ImageObject("Control", handleArea.transform, knobSprite,
            new Color32(255, 224, 92, 255));
        Center(handle.rectTransform, Vector2.zero, new Vector2(50f, 50f));

        Slider slider = root.AddComponent<Slider>();
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.direction = Slider.Direction.LeftToRight;
        return slider;
    }

    private static TMP_Text ValueBadge(string name, Transform parent, string value, Vector2 position)
    {
        Image badge = ImageObject(name, parent, panelSprite, Color.white, Image.Type.Sliced);
        Center(badge.rectTransform, position, new Vector2(105f, 58f));
        return Text("Texto", badge.transform, value, 24f, Color.white, FontStyles.Bold,
            TextAlignmentOptions.Center, Vector2.zero, new Vector2(95f, 48f));
    }

    private static void Validate(Scene scene, MainMenuController controller, MenuPausaUI pause, AudioManager audio,
        GameManager game, PlayerMovement player, Camera camera)
    {
        if (!scene.IsValid() || scene.path != ScenePath) throw new InvalidOperationException("Escena incorrecta.");
        if (UnityEngine.Object.FindObjectsByType<AudioManager>(FindObjectsInactive.Include).Length != 1)
            throw new InvalidOperationException("Debe existir exactamente un AudioManager.");
        if (UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include).Length != 1)
            throw new InvalidOperationException("Debe existir exactamente un AudioListener.");
        if (UnityEngine.Object.FindObjectsByType<MainMenuController>(FindObjectsInactive.Include).Length != 1)
            throw new InvalidOperationException("Debe existir exactamente un MainMenuController.");
        if (UnityEngine.Object.FindObjectsByType<MenuPausaUI>(FindObjectsInactive.Include).Length != 1 ||
            pause == null || pause.panelPausa == null)
            throw new InvalidOperationException("MenuPausaUI no quedó configurado correctamente.");
        if (GameObject.Find("CanvasMenuPrincipal") == null || GameObject.Find("MenuPrincipal") == null)
            throw new InvalidOperationException("Falta la jerarquía del menú.");
        if (controller.botonJugar == null || controller.botonOpciones == null || controller.botonSalir == null ||
            controller.sliderMusica == null || controller.sliderSfx == null || controller.botonVolver == null)
            throw new InvalidOperationException("Faltan referencias UI del menú.");
        if (audio.musicaInicio == null || audio.efectoBoton == null)
            throw new InvalidOperationException("AudioManager no tiene Música Inicio o Botones.");
        if (game.panelHUDIzquierdo == null || game.panelHUDDerecho == null || player == null || camera == null)
            throw new InvalidOperationException("Faltan referencias del gameplay.");
        if (EditorBuildSettings.scenes.Length == 0 || EditorBuildSettings.scenes[0].path != ScenePath)
            throw new InvalidOperationException("Zona1_Escuela no quedó en build index 0.");
    }

    private static void ConfigureBuildSettings()
    {
        EditorBuildSettingsScene gameplay = new EditorBuildSettingsScene(ScenePath, true);
        EditorBuildSettings.scenes = new[] { gameplay };
    }

    private static string FindVisualAsset(bool logo)
    {
        string[] paths = AssetDatabase.FindAssets("t:Texture2D", new[] { VisualFolder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => p.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                        p.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                        p.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        return logo
            ? paths.FirstOrDefault(p => Path.GetFileNameWithoutExtension(p)
                .Equals("LOGO", StringComparison.OrdinalIgnoreCase))
            : paths.FirstOrDefault(p => !Path.GetFileNameWithoutExtension(p)
                .Equals("LOGO", StringComparison.OrdinalIgnoreCase));
    }

    private static void LoadUiAssets()
    {
        font = TMP_Settings.defaultFontAsset;
        if (font == null)
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonArtPath);
        panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PanelArtPath);
        trackSprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiFolder + "/Barra.png");
        knobSprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiFolder + "/Control.png");
        if (font == null || buttonSprite == null || panelSprite == null || trackSprite == null || knobSprite == null)
            throw new InvalidOperationException("No se pudieron cargar los recursos UI del menú.");
    }

    private static void CreateUiArt()
    {
        EnsureFolder("Assets/UI");
        EnsureFolder(UiFolder);
        WriteButton(ButtonArtPath);
        WritePanel(PanelArtPath);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        ConfigureSprite(ButtonArtPath, new Vector4(14f, 14f, 14f, 14f), true);
        ConfigureSprite(PanelArtPath, new Vector4(14f, 14f, 14f, 14f), true);
    }

    private static void WriteButton(string assetPath)
    {
        const int width = 96;
        const int height = 48;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            Color32 pixel = new Color32(0, 0, 0, 0);
            if (InsideRounded(x, y, width, height, 10))
            {
                if (!InsideRounded(x - 4, y - 4, width - 8, height - 8, 7))
                    pixel = new Color32(54, 25, 10, 255);
                else if (!InsideRounded(x - 7, y - 7, width - 14, height - 14, 5))
                    pixel = new Color32(247, 177, 47, 255);
                else if (y >= height - 11)
                    pixel = new Color32(166, 91, 40, 255);
                else if (y <= 10)
                    pixel = new Color32(91, 40, 18, 255);
                else
                    pixel = new Color32(126, 61, 26, 255);
            }
            texture.SetPixel(x, y, pixel);
        }
        SaveTexture(texture, assetPath);
    }

    private static void WritePanel(string assetPath)
    {
        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            Color32 pixel = new Color32(0, 0, 0, 0);
            if (InsideRounded(x, y, size, size, 10))
            {
                if (!InsideRounded(x - 4, y - 4, size - 8, size - 8, 7))
                    pixel = new Color32(48, 24, 10, 255);
                else if (!InsideRounded(x - 7, y - 7, size - 14, size - 14, 5))
                    pixel = new Color32(230, 166, 47, 255);
                else
                    pixel = new Color32(18, 57, 43, 246);
            }
            texture.SetPixel(x, y, pixel);
        }
        SaveTexture(texture, assetPath);
    }

    private static bool InsideRounded(int x, int y, int width, int height, int radius)
    {
        if (width <= 0 || height <= 0 || x < 0 || y < 0 || x >= width || y >= height) return false;
        if ((x >= radius && x < width - radius) || (y >= radius && y < height - radius)) return true;
        int cx = x < radius ? radius : width - radius - 1;
        int cy = y < radius ? radius : height - radius - 1;
        int dx = x - cx;
        int dy = y - cy;
        return dx * dx + dy * dy <= radius * radius;
    }

    private static void SaveTexture(Texture2D texture, string assetPath)
    {
        texture.Apply();
        File.WriteAllBytes(Absolute(assetPath), texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
    }

    private static void ConfigureSprite(string path, Vector4 border, bool point)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = point ? FilterMode.Point : FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
        importer.spriteBorder = border;
        importer.SaveAndReimport();
    }

    private static Image ImageObject(string name, Transform parent, Sprite sprite, Color color,
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

    private static TMP_Text Text(string name, Transform parent, string value, float size, Color color,
        FontStyles style, TextAlignmentOptions alignment, Vector2 position, Vector2 dimensions)
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
        Center(text.rectTransform, position, dimensions);
        return text;
    }

    private static GameObject Container(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void Center(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect, Vector2? min = null, Vector2? max = null)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = min ?? Vector2.zero;
        rect.offsetMax = max ?? Vector2.zero;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }

    private static string Absolute(string assetPath) => Path.Combine(
        Directory.GetParent(Application.dataPath).FullName,
        assetPath.Replace('/', Path.DirectorySeparatorChar));

    private static void Require(UnityEngine.Object value, string name)
    {
        if (value == null) throw new InvalidOperationException("No se encontró " + name + " en Zona1_Escuela.");
    }
}
#endif
