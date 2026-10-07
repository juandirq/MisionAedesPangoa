using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GameplayUIPolishTool
{
    private const string ScenePath = "Assets/Scenes/Zona1_Escuela.unity";
    private const string PanelSpritePath = "Assets/UI/MenuPrincipal/PanelPixel.png";
    private const string DarkPanelSpritePath = "Assets/UI/MenuPrincipal/PanelOpcionesIntegrado.png";
    private const string WoodButtonSpritePath = "Assets/UI/MenuPrincipal/BotonMaderaIntegrado.png";
    private const string TropicalDecorSpritePath = "Assets/Sprites/ZonaInicio/ChatGPT Image 21 sept 2026, 18_25_38 (4).png";

    private static readonly Color Cream = new Color32(255, 244, 213, 255);
    private static readonly Color CreamSoft = new Color32(255, 250, 232, 255);
    private static readonly Color DeepGreen = new Color32(37, 67, 49, 255);
    private static readonly Color LeafGreen = new Color32(91, 139, 72, 255);
    private static readonly Color PaleGreen = new Color32(220, 235, 199, 255);
    private static readonly Color WarmGold = new Color32(228, 169, 59, 255);
    private static readonly Color Wood = new Color32(112, 62, 31, 255);
    private static readonly Color Ink = new Color32(69, 52, 38, 255);
    private static readonly Color White = new Color32(255, 251, 235, 255);

    [MenuItem("Mision Aedes/UI/Aplicar pulido fino de decisiones y dialogo")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        PolishDecision(Find(scene, "VentanaDecision"));
        PolishDialogue(Find(scene, "PanelDialogo"));
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("GAMEPLAY_UI_POLISH_APPLIED");
    }

    [MenuItem("Mision Aedes/UI/Aplicar pulido visual final solicitado")]
    public static void ApplyRequestedPolish()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        PolishZonePanel(Find(scene, "PanelInicioZona1"));
        PolishZonePanel(Find(scene, "PanelInicioZona2"));
        PolishZonePanel(Find(scene, "PanelInicioZona3"));
        PolishDecision(Find(scene, "VentanaDecision"));
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("REQUESTED_FINAL_UI_POLISH_APPLIED");
    }

    [MenuItem("Mision Aedes/UI/Reemplazar solo adornos tropicales")]
    public static void ReplaceTropicalDecorOnly()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        ReplaceDecisionDecor(Find(scene, "VentanaDecision"));
        ReplaceDialogueDecor(Find(scene, "PanelDialogo"));
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("TROPICAL_DECOR_REPLACED_ONLY");
    }

    public static void Validate()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        int errors = 0;
        foreach (string name in new[] { "PanelInicioZona1", "PanelInicioZona2", "PanelInicioZona3", "VentanaDecision", "PanelDialogo" })
        {
            if (Find(scene, name) != null) continue;
            Debug.LogError("Falta el objeto UI requerido: " + name);
            errors++;
        }

        VentanaDecisionUI decision = UnityEngine.Object.FindAnyObjectByType<VentanaDecisionUI>(FindObjectsInactive.Include);
        if (decision == null || decision.ventanaDecision == null || decision.tituloObjeto == null ||
            decision.descripcionObjeto == null || decision.textoFeedback == null ||
            decision.textoBoton1 == null || decision.textoBoton2 == null || decision.textoBoton3 == null)
        {
            Debug.LogError("La ventana de decisiones perdió una referencia serializada.");
            errors++;
        }

        NPCDialogo[] dialogues = UnityEngine.Object.FindObjectsByType<NPCDialogo>(FindObjectsInactive.Include);
        if (dialogues.Length != 3 || dialogues.Any(d => d.panelDialogo == null || d.nombreNPC == null || d.textoDialogo == null))
        {
            Debug.LogError("Las referencias compartidas de diálogo NPC no están completas.");
            errors++;
        }

        foreach (GameObject panel in new[] { Find(scene, "PanelInicioZona1"), Find(scene, "PanelInicioZona2"), Find(scene, "PanelInicioZona3") })
        {
            if (panel != null && panel.GetComponentsInChildren<Button>(true).Length == 1) continue;
            Debug.LogError("Un panel de inicio no conserva exactamente su botón.", panel);
            errors++;
        }

        if (decision != null && decision.ventanaDecision != null &&
            decision.ventanaDecision.GetComponentsInChildren<Button>(true).Length != 3)
        {
            Debug.LogError("La ventana de decisiones no conserva sus tres botones.", decision);
            errors++;
        }

        if (UnityEngine.Object.FindObjectsByType<Collider2D>(FindObjectsInactive.Include)
            .Any(c => c.gameObject.name.StartsWith("Decor_", StringComparison.Ordinal)))
        {
            Debug.LogError("Una capa decorativa recibió un collider por error.");
            errors++;
        }

        Debug.Log("GAMEPLAY_UI_VALIDATION errors=" + errors + " npc=" + dialogues.Length);
        if (errors > 0) throw new InvalidOperationException("Falló la validación segura de la UI.");
    }

    public static void RenderPreviews()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject decisionPanel = Find(scene, "VentanaDecision");
        Canvas canvas = decisionPanel != null ? decisionPanel.GetComponentInParent<Canvas>(true) : null;
        if (canvas == null) throw new InvalidOperationException("No se encontró el Canvas principal.");

        GameObject cameraObject = new GameObject("PreviewCamera", typeof(Camera));
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(55, 91, 70, 255);
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

        foreach (Transform child in canvas.transform)
            child.gameObject.SetActive(false);

        RenderPanel(camera, canvas, Find(scene, "PanelInicioZona2"), "FinePolishZone2.png");
        RenderPanel(camera, canvas, Find(scene, "PanelInicioZona3"), "FinePolishZone3.png");
        RenderPanel(camera, canvas, decisionPanel, "FinePolishDecision.png");
        RenderLongAnswerPreview(camera, canvas, decisionPanel,
            "Lona con agua retenida",
            "Estirarla o guardarla de manera que el\nagua pueda drenar y no forme\ncharcos.",
            "LongAnswerLona_1366.png");
        RenderLongAnswerPreview(camera, canvas, decisionPanel,
            "Carretilla expuesta a la lluvia",
            "Vaciarla y guardarla invertida o bajo\ntecho para evitar nuevo\nestancamiento.",
            "LongAnswerCarretilla_1366.png");
        RenderPanel(camera, canvas, Find(scene, "PanelDialogo"), "FinePolishDialogue.png");
        RenderPanel(camera, canvas, Find(scene, "PanelInicioZona2"), "FinePolishZone2_1920.png", 1920, 1080);
        RenderPanel(camera, canvas, decisionPanel, "FinePolishDecision_1920.png", 1920, 1080);
        UnityEngine.Object.DestroyImmediate(cameraObject);
        Debug.Log("GAMEPLAY_UI_PREVIEWS_RENDERED");
    }

    private static void RenderLongAnswerPreview(Camera camera, Canvas canvas, GameObject panel,
        string titleText, string answerText, string fileName)
    {
        VentanaDecisionUI ui = UnityEngine.Object.FindObjectsByType<VentanaDecisionUI>(FindObjectsInactive.Include)
            .FirstOrDefault(v => v.ventanaDecision == panel);
        if (ui == null) throw new InvalidOperationException("No se encontró el controlador para validar respuestas largas.");

        TMP_Text title = panel.GetComponentsInChildren<TMP_Text>(true)
            .FirstOrDefault(t => t.transform.parent != null && t.transform.parent.name == "PanelTitulo");
        string previousTitle = title != null ? title.text : string.Empty;
        string previousAnswer = ui.textoBoton1.text;
        if (title != null) title.text = titleText;
        ui.textoBoton1.text = answerText;
        RenderPanel(camera, canvas, panel, fileName);
        if (title != null) title.text = previousTitle;
        ui.textoBoton1.text = previousAnswer;
    }

    private static void RenderPanel(Camera camera, Canvas canvas, GameObject panel, string fileName,
        int width = 1366, int height = 768)
    {
        if (panel == null) throw new InvalidOperationException("No se encontró el panel para la previsualización.");
        foreach (Transform child in canvas.transform)
            child.gameObject.SetActive(false);
        panel.SetActive(true);
        Canvas.ForceUpdateCanvases();

        RenderTexture target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        Texture2D image = new Texture2D(width, height, TextureFormat.RGB24, false);
        camera.targetTexture = target;
        camera.Render();
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = target;
        image.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
        image.Apply();
        string output = Path.GetFullPath(Path.Combine(Application.dataPath, "..", fileName));
        File.WriteAllBytes(output, image.EncodeToPNG());
        RenderTexture.active = previous;
        camera.targetTexture = null;
        UnityEngine.Object.DestroyImmediate(image);
        UnityEngine.Object.DestroyImmediate(target);
        panel.SetActive(false);
        Debug.Log("GAMEPLAY_UI_PREVIEW " + output);
    }

    private static void SetLayerRecursively(GameObject root, int layer)
    {
        root.layer = layer;
        foreach (Transform child in root.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    private static void PolishZonePanel(GameObject overlay)
    {
        if (overlay == null) throw new InvalidOperationException("No se encontró un panel de inicio de zona.");
        Image overlayImage = overlay.GetComponent<Image>();
        if (overlayImage != null) overlayImage.color = new Color32(13, 28, 21, 185);

        RectTransform card = overlay.transform.Cast<Transform>()
            .Select(t => t as RectTransform).FirstOrDefault(t => t != null && t.GetComponent<Image>() != null);
        if (card == null) throw new InvalidOperationException(overlay.name + " no tiene tarjeta visual.");
        SetRect(card, new Vector2(840f, 540f), Vector2.zero);
        SetImage(card.GetComponent<Image>(), DarkPanelSprite(), Color.white);
        SetShadow(card.gameObject, new Color(0f, 0f, 0f, 0.42f), new Vector2(9f, -10f));

        RectTransform inner = Layer(card, "Decor_Interior", PanelSprite(), Cream, new Vector2(808f, 508f), Vector2.zero);
        inner.SetAsFirstSibling();
        RectTransform header = Layer(card, "Decor_Cabecera", null, DeepGreen, new Vector2(780f, 112f), new Vector2(0f, 187f));
        header.SetSiblingIndex(1);
        RectTransform line = Layer(card, "Decor_LineaDorada", null, WarmGold, new Vector2(690f, 4f), new Vector2(0f, 123f));
        line.SetSiblingIndex(2);
        AddCorner(card, "Decor_EsquinaIzquierda", new Vector2(-362f, 231f));
        AddCorner(card, "Decor_EsquinaDerecha", new Vector2(362f, 231f));

        TMP_Text title = card.GetComponentsInChildren<TMP_Text>(true)
            .FirstOrDefault(t => t.name.StartsWith("TituloZona", StringComparison.Ordinal));
        TMP_Text description = card.GetComponentsInChildren<TMP_Text>(true)
            .FirstOrDefault(t => t.name.StartsWith("DescripcionZona", StringComparison.Ordinal));
        Image info = card.GetComponentsInChildren<Image>(true)
            .FirstOrDefault(i => i.name.StartsWith("PanelInfoZona", StringComparison.Ordinal));
        Button button = card.GetComponentInChildren<Button>(true);

        if (title != null)
        {
            SetText(title, White, title.text.Contains("\n") ? 37f : 46f, FontStyles.Bold, TextAlignmentOptions.Center);
            SetRect(title.rectTransform, new Vector2(720f, 98f), new Vector2(0f, 187f));
            ConfigureResponsiveText(title, 31f, title.text.Contains("\n") ? 39f : 46f,
                TextWrappingModes.Normal, 3f, new Vector4(14f, 5f, 14f, 5f));
            SetShadow(title.gameObject, new Color(0f, 0f, 0f, 0.35f), new Vector2(2f, -2f));
            title.transform.SetAsLastSibling();
        }
        if (description != null)
        {
            SetText(description, Ink, 25f, FontStyles.Normal, TextAlignmentOptions.Center);
            SetRect(description.rectTransform, new Vector2(700f, 112f), new Vector2(0f, 70f));
            ConfigureResponsiveText(description, 20f, 25f, TextWrappingModes.Normal, 7f,
                new Vector4(18f, 7f, 18f, 7f));
            description.transform.SetAsLastSibling();
        }
        if (info != null)
        {
            SetRect(info.rectTransform, new Vector2(660f, 68f), new Vector2(0f, -57f));
            SetImage(info, PanelSprite(), PaleGreen);
            SetOutline(info.gameObject, new Color32(119, 151, 82, 210), new Vector2(2f, -2f));
            TMP_Text infoText = info.GetComponentInChildren<TMP_Text>(true);
            if (infoText != null)
            {
                SetRect(infoText.rectTransform, new Vector2(620f, 52f), Vector2.zero);
                SetText(infoText, DeepGreen, 22f, FontStyles.Bold, TextAlignmentOptions.Center);
                ConfigureResponsiveText(infoText, 18f, 22f, TextWrappingModes.Normal, 2f,
                    new Vector4(12f, 4f, 12f, 4f));
            }
            info.transform.SetAsLastSibling();
        }
        if (button != null)
        {
            SetRect((RectTransform)button.transform, new Vector2(430f, 74f), new Vector2(0f, -188f));
            StyleWoodButton(button, 22f);
            button.transform.SetAsLastSibling();
        }
    }

    private static void PolishDecision(GameObject panel)
    {
        if (panel == null) throw new InvalidOperationException("No se encontró VentanaDecision.");
        SetRect((RectTransform)panel.transform, new Vector2(800f, 760f), new Vector2(0f, -8f));
        Image rootImage = panel.GetComponent<Image>();
        if (rootImage != null) rootImage.color = Color.clear;
        RectTransform border = ChildRect(panel.transform, "BordeMarron");
        RectTransform background = ChildRect(panel.transform, "FondoCrema");
        RectTransform titlePanel = ChildRect(panel.transform, "PanelTitulo");
        RectTransform line = ChildRect(panel.transform, "LineaTitulo");
        if (border != null)
        {
            SetRect(border, new Vector2(780f, 740f), Vector2.zero);
            SetImage(border.GetComponent<Image>(), DarkPanelSprite(), Color.white);
            SetShadow(border.gameObject, new Color(0f, 0f, 0f, 0.38f), new Vector2(9f, -10f));
            border.SetAsFirstSibling();
        }
        if (background != null)
        {
            SetRect(background, new Vector2(750f, 710f), Vector2.zero);
            SetImage(background.GetComponent<Image>(), PanelSprite(), Cream);
            background.SetSiblingIndex(1);
        }
        RectTransform titleBackground = Layer((RectTransform)panel.transform, "Decor_CabeceraDecision",
            null, DeepGreen, new Vector2(730f, 92f), new Vector2(0f, 307f));
        titleBackground.SetSiblingIndex(2);
        if (titlePanel != null)
        {
            SetRect(titlePanel, new Vector2(730f, 92f), new Vector2(0f, 307f));
            SetImage(titlePanel.GetComponent<Image>(), null, Color.clear);
            TMP_Text title = titlePanel.GetComponentInChildren<TMP_Text>(true);
            if (title != null)
            {
                SetRect(title.rectTransform, new Vector2(626f, 76f), Vector2.zero);
                SetText(title, White, 30f, FontStyles.Bold, TextAlignmentOptions.Center);
                ConfigureResponsiveText(title, 22f, 30f, TextWrappingModes.Normal, 0f,
                    new Vector4(8f, 4f, 8f, 4f));
                SetShadow(title.gameObject, new Color(0f, 0f, 0f, 0.3f), new Vector2(2f, -2f));
            }
        }
        if (line != null)
        {
            SetRect(line, new Vector2(630f, 4f), new Vector2(0f, 132f));
            SetImage(line.GetComponent<Image>(), null, WarmGold);
        }

        VentanaDecisionUI ui = UnityEngine.Object.FindObjectsByType<VentanaDecisionUI>(FindObjectsInactive.Include)
            .FirstOrDefault(v => v.ventanaDecision == panel);
        if (ui == null) throw new InvalidOperationException("VentanaDecision no conserva su controlador.");
        SetRect(ui.descripcionObjeto.rectTransform, new Vector2(670f, 120f), new Vector2(0f, 204f));
        SetText(ui.descripcionObjeto, Ink, 21f, FontStyles.Normal, TextAlignmentOptions.Center);
        ConfigureResponsiveText(ui.descripcionObjeto, 17f, 21f, TextWrappingModes.Normal, 3f,
            new Vector4(18f, 7f, 18f, 7f));

        TMP_Text[] labels = { ui.textoBoton1, ui.textoBoton2, ui.textoBoton3 };
        float[] ys = { 55f, -75f, -205f };
        for (int index = 0; index < labels.Length; index++)
        {
            Button button = labels[index].GetComponentInParent<Button>(true);
            if (button == null) continue;
            SetRect((RectTransform)button.transform, new Vector2(600f, 120f), new Vector2(0f, ys[index]));
            StyleAnswerButton(button);
            SetText(labels[index], DeepGreen, 20f, FontStyles.Bold, TextAlignmentOptions.Center);
            ConfigureResponsiveText(labels[index], 16f, 20f, TextWrappingModes.Normal, 1f,
                new Vector4(26f, 10f, 26f, 10f));
        }

        RectTransform feedback = ui.textoFeedback.transform.parent as RectTransform;
        if (feedback != null)
        {
            SetRect(feedback, new Vector2(660f, 78f), new Vector2(0f, -311f));
            SetImage(feedback.GetComponent<Image>(), PanelSprite(), CreamSoft);
            SetOutline(feedback.gameObject, new Color32(204, 164, 81, 180), new Vector2(2f, -2f));
        }
        SetRect(ui.textoFeedback.rectTransform, new Vector2(610f, 58f), Vector2.zero);
        ui.textoFeedback.fontSize = 21f;
        ui.textoFeedback.fontStyle = FontStyles.Bold;
        ui.textoFeedback.alignment = TextAlignmentOptions.Center;
        ConfigureResponsiveText(ui.textoFeedback, 17f, 21f, TextWrappingModes.Normal, 0f,
            new Vector4(10f, 5f, 10f, 5f));
        if (titlePanel != null) titlePanel.SetAsLastSibling();
        ReplaceDecisionDecor(panel);
    }

    private static void PolishDialogue(GameObject panel)
    {
        if (panel == null) throw new InvalidOperationException("No se encontró PanelDialogo.");
        RectTransform root = (RectTransform)panel.transform;
        SetRect(root, new Vector2(1000f, 250f), new Vector2(0f, 150f));
        SetImage(panel.GetComponent<Image>(), DarkPanelSprite(), Color.white);
        SetShadow(panel, new Color(0f, 0f, 0f, 0.4f), new Vector2(8f, -9f));
        RectTransform inner = Layer(root, "Decor_DialogoInterior", PanelSprite(), CreamSoft, new Vector2(970f, 220f), Vector2.zero);
        inner.SetAsFirstSibling();
        RectTransform nameBackground = Layer(root, "Decor_NombreFondo", WoodButtonSprite(), Color.white,
            new Vector2(260f, 58f), new Vector2(-337f, 67f));
        nameBackground.SetSiblingIndex(1);
        RectTransform divider = Layer(root, "Decor_DivisorDialogo", null, WarmGold,
            new Vector2(875f, 3f), new Vector2(0f, 31f));
        divider.SetSiblingIndex(2);

        NPCDialogo sample = UnityEngine.Object.FindObjectsByType<NPCDialogo>(FindObjectsInactive.Include)
            .FirstOrDefault(d => d.panelDialogo == panel);
        if (sample == null) throw new InvalidOperationException("PanelDialogo no conserva sus NPC enlazados.");
        SetRect(sample.nombreNPC.rectTransform, new Vector2(230f, 46f), new Vector2(-337f, 67f));
        SetText(sample.nombreNPC, White, 25f, FontStyles.Bold, TextAlignmentOptions.Center);
        ConfigureResponsiveText(sample.nombreNPC, 20f, 25f, TextWrappingModes.NoWrap, 0f,
            new Vector4(8f, 4f, 8f, 4f));
        SetShadow(sample.nombreNPC.gameObject, new Color(0f, 0f, 0f, 0.35f), new Vector2(2f, -2f));
        sample.nombreNPC.transform.SetAsLastSibling();
        SetRect(sample.textoDialogo.rectTransform, new Vector2(875f, 100f), new Vector2(0f, -25f));
        SetText(sample.textoDialogo, Ink, 24f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        ConfigureResponsiveText(sample.textoDialogo, 19f, 24f, TextWrappingModes.Normal, 4f,
            new Vector4(10f, 7f, 10f, 5f));
        sample.textoDialogo.transform.SetAsLastSibling();

        Button continueButton = panel.GetComponentsInChildren<Button>(true).FirstOrDefault();
        if (continueButton != null)
        {
            SetRect((RectTransform)continueButton.transform, new Vector2(190f, 50f), new Vector2(370f, -91f));
            StyleWoodButton(continueButton, 16f);
            continueButton.transform.SetAsLastSibling();
        }

        ReplaceDialogueDecor(panel);
    }

    private static void ReplaceDecisionDecor(GameObject panel)
    {
        if (panel == null) throw new InvalidOperationException("No se encontró VentanaDecision.");
        RectTransform root = (RectTransform)panel.transform;
        RemoveDecor(root, "Decor_AcentoIzquierdo");
        RemoveDecor(root, "Decor_AcentoDerecho");
        SetTropicalSprite(root, "Decor_HojasDecisionIzq", new Vector2(-350f, 307f), 26f, false);
        SetTropicalSprite(root, "Decor_HojasDecisionDer", new Vector2(350f, 307f), 26f, true);
    }

    private static void ReplaceDialogueDecor(GameObject panel)
    {
        if (panel == null) throw new InvalidOperationException("No se encontró PanelDialogo.");
        RectTransform root = (RectTransform)panel.transform;
        SetTropicalSprite(root, "Decor_TropicalIzquierdo", new Vector2(-445f, -88f), 22f, false);
        SetTropicalSprite(root, "Decor_TropicalDerecho", new Vector2(445f, 60f), 22f, true);
    }

    private static void SetTropicalSprite(RectTransform parent, string name, Vector2 position, float size, bool mirror)
    {
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(TropicalDecorSpritePath)
            .OfType<Sprite>()
            .OrderByDescending(item => item.rect.width * item.rect.height)
            .FirstOrDefault();
        if (sprite == null) throw new InvalidOperationException("No se pudo cargar el sprite tropical: " + TropicalDecorSpritePath);

        Transform existing = parent.Find(name);
        GameObject item = existing != null ? existing.gameObject :
            new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        if (existing == null) item.transform.SetParent(parent, false);
        while (item.transform.childCount > 0)
            UnityEngine.Object.DestroyImmediate(item.transform.GetChild(0).gameObject);

        item.layer = parent.gameObject.layer;
        RectTransform rect = item.GetComponent<RectTransform>();
        SetRect(rect, new Vector2(size, size), position);
        rect.localScale = new Vector3(mirror ? -1f : 1f, 1f, 1f);
        Image image = item.GetComponent<Image>();
        if (image == null) image = item.AddComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = true;
        image.color = Color.white;
        image.raycastTarget = false;
        rect.SetAsLastSibling();
    }

    private static void RemoveDecor(RectTransform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);
    }

    private static void StyleWoodButton(Button button, float fontSize)
    {
        SetImage(button.image, WoodButtonSprite(), Color.white);
        button.transition = Selectable.Transition.ColorTint;
        button.colors = Colors(Color.white, new Color32(255, 233, 173, 255), new Color32(211, 154, 76, 255),
            new Color32(255, 233, 173, 255), new Color32(125, 111, 91, 180));
        SetShadow(button.gameObject, new Color(0f, 0f, 0f, 0.3f), new Vector2(4f, -4f));
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label == null) return;
        SetText(label, White, fontSize, FontStyles.Bold, TextAlignmentOptions.Center);
        SetRect(label.rectTransform, Vector2.zero, Vector2.zero, true);
        label.margin = new Vector4(18f, 3f, 18f, 3f);
        SetShadow(label.gameObject, new Color(0f, 0f, 0f, 0.4f), new Vector2(2f, -2f));
    }

    private static void StyleAnswerButton(Button button)
    {
        SetImage(button.image, PanelSprite(), PaleGreen);
        button.transition = Selectable.Transition.ColorTint;
        button.colors = Colors(Color.white, new Color32(255, 242, 195, 255), new Color32(187, 211, 160, 255),
            new Color32(246, 213, 143, 255), new Color32(157, 166, 145, 180));
        SetShadow(button.gameObject, new Color(0.16f, 0.24f, 0.15f, 0.28f), new Vector2(3f, -3f));
    }

    private static ColorBlock Colors(Color normal, Color hover, Color pressed, Color selected, Color disabled)
    {
        return new ColorBlock { normalColor = normal, highlightedColor = hover, pressedColor = pressed,
            selectedColor = selected, disabledColor = disabled, colorMultiplier = 1f, fadeDuration = 0.08f };
    }

    private static void ConfigureResponsiveText(TMP_Text text, float minimum, float maximum,
        TextWrappingModes wrapping, float lineSpacing, Vector4 margin)
    {
        text.enableAutoSizing = true;
        text.fontSizeMin = minimum;
        text.fontSizeMax = maximum;
        text.textWrappingMode = wrapping;
        text.lineSpacing = lineSpacing;
        text.margin = margin;
        text.overflowMode = TextOverflowModes.Overflow;
    }

    private static void AddLeafPair(RectTransform parent, string name, Vector2 position, bool mirror)
    {
        RectTransform group = Layer(parent, name, null, Color.clear, new Vector2(62f, 42f), position);
        group.SetAsLastSibling();
        float direction = mirror ? -1f : 1f;
        RectTransform stem = Layer(group, "Tallo", null, WarmGold, new Vector2(4f, 28f), new Vector2(0f, -1f));
        stem.localRotation = Quaternion.Euler(0f, 0f, direction * -34f);
        RectTransform upper = Layer(group, "HojaSuperior", null, PaleGreen, new Vector2(19f, 10f),
            new Vector2(direction * 9f, 9f));
        upper.localRotation = Quaternion.Euler(0f, 0f, direction * 28f);
        RectTransform lower = Layer(group, "HojaInferior", null, LeafGreen, new Vector2(21f, 11f),
            new Vector2(direction * -7f, -7f));
        lower.localRotation = Quaternion.Euler(0f, 0f, direction * -28f);
    }

    private static void AddTropicalCorner(RectTransform parent, string name, Vector2 position, bool mirror)
    {
        RectTransform group = Layer(parent, name, null, Color.clear, new Vector2(74f, 70f), position);
        group.SetAsLastSibling();
        float direction = mirror ? -1f : 1f;
        RectTransform stem = Layer(group, "Tallo", null, Wood, new Vector2(5f, 45f), new Vector2(0f, -4f));
        stem.localRotation = Quaternion.Euler(0f, 0f, direction * -18f);
        RectTransform leafTop = Layer(group, "HojaAlta", null, LeafGreen, new Vector2(24f, 12f),
            new Vector2(direction * 9f, 13f));
        leafTop.localRotation = Quaternion.Euler(0f, 0f, direction * 30f);
        RectTransform leafMiddle = Layer(group, "HojaMedia", null, PaleGreen, new Vector2(27f, 13f),
            new Vector2(direction * -10f, -2f));
        leafMiddle.localRotation = Quaternion.Euler(0f, 0f, direction * -27f);
        RectTransform flower = Layer(group, "Flor", null, WarmGold, new Vector2(11f, 11f),
            new Vector2(direction * 4f, 25f));
        flower.localRotation = Quaternion.Euler(0f, 0f, 45f);
    }

    private static void AddCorner(RectTransform parent, string name, Vector2 position)
    {
        RectTransform outer = Layer(parent, name, null, WarmGold, new Vector2(34f, 34f), position);
        outer.SetSiblingIndex(Mathf.Min(3, parent.childCount - 1));
        Layer(outer, "Interior", null, DeepGreen, new Vector2(20f, 20f), Vector2.zero);
    }

    private static void AddAccent(RectTransform parent, string name, Vector2 position)
    {
        RectTransform accent = Layer(parent, name, null, WarmGold, new Vector2(20f, 20f), position);
        accent.SetAsLastSibling();
        accent.localRotation = Quaternion.Euler(0f, 0f, 45f);
    }

    private static RectTransform Layer(RectTransform parent, string name, Sprite sprite, Color color, Vector2 size, Vector2 position)
    {
        Transform existing = parent.Find(name);
        GameObject item = existing != null ? existing.gameObject :
            new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        if (existing == null) item.transform.SetParent(parent, false);
        item.layer = parent.gameObject.layer;
        RectTransform rect = item.GetComponent<RectTransform>();
        SetRect(rect, size, position);
        Image image = item.GetComponent<Image>();
        SetImage(image, sprite, color);
        image.raycastTarget = false;
        return rect;
    }

    private static void SetImage(Image image, Sprite sprite, Color color)
    {
        if (image == null) return;
        image.sprite = sprite;
        image.type = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
        image.color = color;
    }

    private static void SetText(TMP_Text text, Color color, float size, FontStyles style, TextAlignmentOptions alignment)
    {
        text.color = color;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.overflowMode = TextOverflowModes.Ellipsis;
    }

    private static void SetRect(RectTransform rect, Vector2 size, Vector2 position, bool stretch = false)
    {
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.pivot = new Vector2(0.5f, 0.5f);
        if (stretch)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return;
        }
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    private static void SetShadow(GameObject item, Color color, Vector2 distance)
    {
        Shadow shadow = item.GetComponent<Shadow>();
        if (shadow == null) shadow = item.AddComponent<Shadow>();
        shadow.effectColor = color;
        shadow.effectDistance = distance;
        shadow.useGraphicAlpha = true;
    }

    private static void SetOutline(GameObject item, Color color, Vector2 distance)
    {
        Outline outline = item.GetComponent<Outline>();
        if (outline == null) outline = item.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = distance;
        outline.useGraphicAlpha = true;
    }

    private static RectTransform ChildRect(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        return child as RectTransform;
    }

    private static GameObject Find(Scene scene, string name)
    {
        return scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .Where(item => item.name == name).Select(item => item.gameObject).FirstOrDefault();
    }

    private static Sprite PanelSprite() => AssetDatabase.LoadAssetAtPath<Sprite>(PanelSpritePath);
    private static Sprite DarkPanelSprite() => AssetDatabase.LoadAssetAtPath<Sprite>(DarkPanelSpritePath);
    private static Sprite WoodButtonSprite() => AssetDatabase.LoadAssetAtPath<Sprite>(WoodButtonSpritePath);
}
