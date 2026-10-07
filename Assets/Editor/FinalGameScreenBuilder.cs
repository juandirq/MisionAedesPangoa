using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class FinalGameScreenBuilder
{
    private const string ScenePath = "Assets/Scenes/Zona1_Escuela.unity";
    private const string PanelPath = "Assets/UI/MenuPrincipal/PanelPixel.png";
    private const string DarkPath = "Assets/UI/MenuPrincipal/PanelOpcionesIntegrado.png";
    private const string ButtonPath = "Assets/UI/MenuPrincipal/BotonMaderaIntegrado.png";
    private const string TropicalPath = "Assets/Sprites/ZonaInicio/ChatGPT Image 21 sept 2026, 18_25_38 (4).png";
    private const string PlayerClipPath = "Assets/Animations/Player/WalkDown.anim";
    private const string DocenteClipPath = "Assets/Sprites/NPCs/Docente_Idle/DocenteIdle.anim";
    private const string VecinoClipPath = "Assets/Sprites/NPCs/Docente_Idle/VecinoIdle.anim";
    private const string PromotorClipPath = "Assets/Sprites/NPCs/Docente_Idle/PromotorIdle.anim";

    private static readonly Color Cream = new Color32(255, 247, 221, 255);
    private static readonly Color DeepGreen = new Color32(33, 72, 49, 255);
    private static readonly Color Ink = new Color32(69, 52, 38, 255);
    private static readonly Color Gold = new Color32(231, 177, 68, 255);
    private static readonly Color White = new Color32(255, 252, 238, 255);

    [MenuItem("Mision Aedes/Crear pantalla final del juego")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameManager game = UnityEngine.Object.FindAnyObjectByType<GameManager>(FindObjectsInactive.Include);
        if (game == null) throw new InvalidOperationException("No se encontró GameManager.");
        Canvas canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (canvas == null) throw new InvalidOperationException("No se encontró el Canvas de gameplay.");

        Transform previous = canvas.transform.Find("PanelFinalJuego");
        if (previous != null) Undo.DestroyObjectImmediate(previous.gameObject);

        GameObject root = NewUI("PanelFinalJuego", canvas.transform, typeof(Image), typeof(CanvasGroup), typeof(FinalGameUI));
        Stretch(root.GetComponent<RectTransform>());
        Image overlay = root.GetComponent<Image>();
        overlay.sprite = LoadSprite(DarkPath);
        overlay.type = Image.Type.Sliced;
        overlay.color = new Color32(8, 25, 18, 224);
        overlay.raycastTarget = true;

        RectTransform card = ImageRect("TarjetaFinal", root.transform, LoadSprite(PanelPath), Cream,
            new Vector2(1020f, 704f), Vector2.zero);
        AddShadow(card.gameObject, new Color(0f, 0f, 0f, 0.45f), new Vector2(10f, -11f));
        RectTransform header = ImageRect("CabeceraFinal", card, null, DeepGreen,
            new Vector2(982f, 136f), new Vector2(0f, 270f));
        ImageRect("LineaDorada", card, null, Gold, new Vector2(854f, 4f), new Vector2(0f, 198f));

        TMP_Text title = Text("TituloFinal", header, "¡MISIÓN CUMPLIDA!", 46f, White,
            FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(800f, 72f), new Vector2(0f, 16f));
        title.enableAutoSizing = true; title.fontSizeMin = 34f; title.fontSizeMax = 46f;
        AddShadow(title.gameObject, new Color(0f, 0f, 0f, 0.35f), new Vector2(2f, -2f));
        Text("SubtituloFinal", header, "Completaste las tres zonas de Pangoa", 23f, new Color32(235, 224, 178, 255),
            FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(780f, 38f), new Vector2(0f, -38f));

        Text("MensajeFinal", card,
            "Ayudaste a eliminar posibles criaderos del Aedes aegypti.", 24f, Ink,
            FontStyles.Normal, TextAlignmentOptions.Center, new Vector2(870f, 48f), new Vector2(0f, 158f));

        RectTransform characters = Rect("PersonajesFinal", card, new Vector2(860f, 244f), new Vector2(0f, 18f));
        Sprite[] player = Frames(PlayerClipPath);
        Sprite[] docente = Frames(DocenteClipPath);
        Sprite[] vecino = Frames(VecinoClipPath);
        Sprite[] promotor = Frames(PromotorClipPath);
        if (player.Length == 0 || docente.Length == 0 || vecino.Length == 0 || promotor.Length == 0)
            throw new InvalidOperationException("No se encontraron todos los frames reales de los personajes.");

        Image[] images = new Image[4];
        string[] names = { "PlayerFinal", "DocenteFinal", "VecinoFinal", "PromotorFinal" };
        string[] labels = { "TÚ", "DOCENTE", "VECINO", "PROMOTOR" };
        Sprite[][] sets = { player, docente, vecino, promotor };
        float[] xs = { -306f, -102f, 102f, 306f };
        for (int i = 0; i < 4; i++)
        {
            RectTransform slot = Rect(names[i] + "Contenedor", characters, new Vector2(182f, 232f), new Vector2(xs[i], 0f));
            images[i] = ImageRect(names[i], slot, sets[i][0], Color.white,
                new Vector2(150f, 174f), new Vector2(0f, 25f)).GetComponent<Image>();
            images[i].preserveAspect = true;
            Text("Etiqueta" + names[i], slot, labels[i], 18f, DeepGreen, FontStyles.Bold,
                TextAlignmentOptions.Center, new Vector2(164f, 28f), new Vector2(0f, -91f));
        }

        RectTransform summary = ImageRect("ResumenFinal", card, LoadSprite(PanelPath), new Color32(229, 239, 207, 255),
            new Vector2(500f, 58f), new Vector2(0f, -142f));
        Text("ZonasCompletadas", summary, "ZONAS COMPLETADAS: 3/3", 24f, DeepGreen,
            FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(458f, 42f), Vector2.zero);

        Text("FraseFinal", card, "Cada acción cuenta para proteger a nuestra comunidad.", 25f, DeepGreen,
            FontStyles.Bold | FontStyles.Italic, TextAlignmentOptions.Center, new Vector2(820f, 48f), new Vector2(0f, -207f));

        Button button = Button("BotonVolverMenu", card, "VOLVER AL MENÚ", new Vector2(410f, 66f), new Vector2(0f, -282f));

        Sprite tropical = LoadSprite(TropicalPath);
        ImageRect("Decor_HeliconiaIzquierda", header, tropical, Color.white, new Vector2(46f, 46f), new Vector2(-435f, 8f))
            .GetComponent<Image>().preserveAspect = true;
        RectTransform rightDecor = ImageRect("Decor_HeliconiaDerecha", header, tropical, Color.white,
            new Vector2(46f, 46f), new Vector2(435f, 8f));
        rightDecor.localScale = new Vector3(-1f, 1f, 1f);
        rightDecor.GetComponent<Image>().preserveAspect = true;
        ImageRect("Decor_HojaInferiorIzquierda", card, tropical, new Color(1f, 1f, 1f, 0.82f),
            new Vector2(58f, 58f), new Vector2(-447f, -278f)).GetComponent<Image>().preserveAspect = true;
        RectTransform lowerRight = ImageRect("Decor_HojaInferiorDerecha", card, tropical,
            new Color(1f, 1f, 1f, 0.82f), new Vector2(58f, 58f), new Vector2(447f, -278f));
        lowerRight.localScale = new Vector3(-1f, 1f, 1f);
        lowerRight.GetComponent<Image>().preserveAspect = true;

        FinalGameUI controller = root.GetComponent<FinalGameUI>();
        controller.canvasGroup = root.GetComponent<CanvasGroup>();
        controller.titulo = title.rectTransform;
        controller.botonVolverMenu = button;
        controller.personajes = images;
        controller.framesPlayer = player.Take(1).ToArray();
        controller.framesDocente = docente;
        controller.framesVecino = vecino;
        controller.framesPromotor = promotor;
        controller.canvasGroup.alpha = 1f;
        controller.canvasGroup.interactable = true;
        controller.canvasGroup.blocksRaycasts = true;
        game.panelFinalJuego = controller;
        root.transform.SetAsLastSibling();
        root.SetActive(false);

        Validate(scene, game, controller);
        EditorUtility.SetDirty(game);
        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("FINAL_GAME_SCREEN_CREATED sprites=Player+Docente+Vecino+Promotor");
    }

    public static void ApplyAllRequestedVisualPolish()
    {
        GameplayUIPolishTool.ApplyRequestedPolish();
        Build();
        Debug.Log("ALL_REQUESTED_VISUAL_POLISH_APPLIED");
    }

    public static void ValidateOnly()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameManager game = UnityEngine.Object.FindAnyObjectByType<GameManager>(FindObjectsInactive.Include);
        FinalGameUI final = UnityEngine.Object.FindAnyObjectByType<FinalGameUI>(FindObjectsInactive.Include);
        Validate(scene, game, final);
        Debug.Log("FINAL_GAME_SCREEN_VALIDATION errors=0");
    }

    public static void ValidateAndRender()
    {
        ValidateOnly();
        RenderPreview();
    }

    public static void ValidateAllAndRender()
    {
        GameplayUIPolishTool.Validate();
        ValidateOnly();
        GameplayUIPolishTool.RenderPreviews();
        RenderPreview();
        Debug.Log("FINAL_VISUAL_POLISH_VALIDATION errors=0");
    }

    public static void RenderPreview()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        FinalGameUI final = UnityEngine.Object.FindAnyObjectByType<FinalGameUI>(FindObjectsInactive.Include);
        if (final == null) throw new InvalidOperationException("No se encontró PanelFinalJuego.");
        Canvas canvas = final.GetComponentInParent<Canvas>(true);
        if (canvas == null) throw new InvalidOperationException("PanelFinalJuego no pertenece a un Canvas.");

        GameObject cameraObject = new GameObject("FinalPreviewCamera", typeof(Camera));
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(44, 83, 61, 255);
        camera.orthographic = true;
        camera.orthographicSize = 5.4f;
        camera.aspect = 1366f / 768f;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 100f;
        camera.cullingMask = 1 << 31;
        camera.transform.position = new Vector3(0f, 0f, -10f);

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = camera;
        RectTransform canvasRect = (RectTransform)canvas.transform;
        canvasRect.sizeDelta = new Vector2(1920f, 1080f);
        canvasRect.position = Vector3.zero;
        canvasRect.rotation = Quaternion.identity;
        canvasRect.localScale = Vector3.one * 0.01f;
        SetLayerRecursively(canvas.gameObject, 31);
        foreach (Transform child in canvas.transform) child.gameObject.SetActive(false);
        final.gameObject.SetActive(true);
        Canvas.ForceUpdateCanvases();

        RenderTexture target = new RenderTexture(1366, 768, 24, RenderTextureFormat.ARGB32);
        Texture2D image = new Texture2D(1366, 768, TextureFormat.RGB24, false);
        camera.targetTexture = target;
        camera.Render();
        RenderTexture.active = target;
        image.ReadPixels(new Rect(0f, 0f, 1366, 768), 0, 0);
        image.Apply();
        string output = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "FinalGameScreenPreview.png"));
        File.WriteAllBytes(output, image.EncodeToPNG());
        RenderTexture.active = null;
        camera.targetTexture = null;
        UnityEngine.Object.DestroyImmediate(image);
        UnityEngine.Object.DestroyImmediate(target);
        UnityEngine.Object.DestroyImmediate(cameraObject);
        Debug.Log("FINAL_GAME_SCREEN_PREVIEW " + output);
    }

    private static void Validate(Scene scene, GameManager game, FinalGameUI final)
    {
        if (!scene.IsValid() || game == null || final == null || game.panelFinalJuego != final)
            throw new InvalidOperationException("La pantalla final no quedó enlazada con GameManager.");
        if (final.gameObject.activeSelf) throw new InvalidOperationException("PanelFinalJuego debe comenzar inactivo.");
        if (final.botonVolverMenu == null || final.personajes == null || final.personajes.Length != 4 ||
            final.personajes.Any(i => i == null || !i.preserveAspect || i.raycastTarget))
            throw new InvalidOperationException("La pantalla final no conserva sus cuatro personajes UI seguros.");
        if (final.framesPlayer.Length == 0 || final.framesDocente.Length == 0 ||
            final.framesVecino.Length == 0 || final.framesPromotor.Length == 0)
            throw new InvalidOperationException("Faltan frames reales de personajes.");
        if (UnityEngine.Object.FindObjectsByType<AudioManager>(FindObjectsInactive.Include).Length != 1)
            throw new InvalidOperationException("Debe existir exactamente un AudioManager.");
        if (UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include).Length != 1)
            throw new InvalidOperationException("Debe existir exactamente un AudioListener.");
    }

    private static Sprite[] Frames(string clipPath)
    {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null) return Array.Empty<Sprite>();
        EditorCurveBinding binding = AnimationUtility.GetObjectReferenceCurveBindings(clip)
            .FirstOrDefault(b => b.propertyName == "m_Sprite");
        return AnimationUtility.GetObjectReferenceCurve(clip, binding)
            .Select(key => key.value as Sprite).Where(sprite => sprite != null).Distinct().ToArray();
    }

    private static void SetLayerRecursively(GameObject root, int layer)
    {
        root.layer = layer;
        foreach (Transform child in root.transform) SetLayerRecursively(child.gameObject, layer);
    }

    private static Sprite LoadSprite(string path) => AssetDatabase.LoadAllAssetsAtPath(path)
        .OfType<Sprite>().OrderByDescending(s => s.rect.width * s.rect.height).FirstOrDefault();

    private static GameObject NewUI(string name, Transform parent, params Type[] components)
    {
        GameObject item = new GameObject(name, new[] { typeof(RectTransform), typeof(CanvasRenderer) }.Concat(components).Distinct().ToArray());
        Undo.RegisterCreatedObjectUndo(item, "Crear pantalla final del juego");
        item.transform.SetParent(parent, false);
        item.layer = parent.gameObject.layer;
        return item;
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 position)
    {
        RectTransform rect = NewUI(name, parent).GetComponent<RectTransform>();
        SetRect(rect, size, position);
        return rect;
    }

    private static RectTransform ImageRect(string name, Transform parent, Sprite sprite, Color color, Vector2 size, Vector2 position)
    {
        RectTransform rect = NewUI(name, parent, typeof(Image)).GetComponent<RectTransform>();
        SetRect(rect, size, position);
        Image image = rect.GetComponent<Image>(); image.sprite = sprite; image.color = color;
        image.type = sprite != null ? Image.Type.Sliced : Image.Type.Simple; image.raycastTarget = false;
        return rect;
    }

    private static TMP_Text Text(string name, Transform parent, string value, float size, Color color,
        FontStyles style, TextAlignmentOptions alignment, Vector2 rectSize, Vector2 position)
    {
        TextMeshProUGUI text = NewUI(name, parent, typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        SetRect(text.rectTransform, rectSize, position); text.text = value; text.fontSize = size;
        text.color = color; text.fontStyle = style; text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal; text.raycastTarget = false;
        return text;
    }

    private static Button Button(string name, Transform parent, string label, Vector2 size, Vector2 position)
    {
        RectTransform rect = NewUI(name, parent, typeof(Image), typeof(Button)).GetComponent<RectTransform>();
        SetRect(rect, size, position);
        Image image = rect.GetComponent<Image>(); image.sprite = LoadSprite(ButtonPath); image.type = Image.Type.Sliced;
        Button button = rect.GetComponent<Button>(); button.targetGraphic = image;
        button.colors = new ColorBlock { normalColor = Color.white, highlightedColor = new Color32(255, 232, 171, 255),
            pressedColor = new Color32(210, 154, 78, 255), selectedColor = Color.white,
            disabledColor = new Color32(125, 111, 91, 180), colorMultiplier = 1f, fadeDuration = 0.08f };
        TMP_Text text = Text("Texto", rect, label, 22f, White, FontStyles.Bold,
            TextAlignmentOptions.Center, size - new Vector2(28f, 10f), Vector2.zero);
        AddShadow(text.gameObject, new Color(0f, 0f, 0f, 0.4f), new Vector2(2f, -2f));
        AddShadow(rect.gameObject, new Color(0f, 0f, 0f, 0.3f), new Vector2(4f, -4f));
        return button;
    }

    private static void SetRect(RectTransform rect, Vector2 size, Vector2 position)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size; rect.anchoredPosition = position; rect.localScale = Vector3.one;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero; rect.localScale = Vector3.one;
    }

    private static void AddShadow(GameObject item, Color color, Vector2 distance)
    {
        Shadow shadow = item.AddComponent<Shadow>(); shadow.effectColor = color;
        shadow.effectDistance = distance; shadow.useGraphicAlpha = true;
    }
}
