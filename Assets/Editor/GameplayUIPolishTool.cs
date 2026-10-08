using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
        PolishPause(Find(scene, "PanelPausa"));
        PolishVictory(Find(scene, "PanelVictoria"));
        PolishDefeat(Find(scene, "PanelDerrota"));
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
        foreach (string name in new[] { "PanelInicioZona1", "PanelInicioZona2", "PanelInicioZona3", "VentanaDecision", "PanelDialogo", "PanelPausa", "PanelVictoria", "PanelDerrota" })
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

        if (decision != null)
        {
            foreach (TMP_Text label in new TMP_Text[] { decision.textoBoton1, decision.textoBoton2, decision.textoBoton3 })
            {
                Button answer = label != null ? label.GetComponentInParent<Button>(true) : null;
                if (answer != null && answer.colors.selectedColor == answer.colors.normalColor) continue;
                Debug.LogError("Una respuesta conserva un color Selected diferente de Normal.", answer);
                errors++;
            }
        }

        MenuPausaUI pause = UnityEngine.Object.FindAnyObjectByType<MenuPausaUI>(FindObjectsInactive.Include);
        GameManager game = UnityEngine.Object.FindAnyObjectByType<GameManager>(FindObjectsInactive.Include);
        if (pause == null || pause.panelPausa == null || pause.gameManager != game || pause.playerMovement == null ||
            pause.panelPausa.GetComponentsInChildren<Button>(true).Length != 3)
        {
            Debug.LogError("MenuPausaUI perdió referencias o botones.", pause);
            errors++;
        }
        if (game == null || game.panelVictoria == null || game.panelDerrota == null || game.tituloDerrota == null ||
            game.textoMotivoDerrota == null || game.panelVictoria.GetComponentsInChildren<Button>(true).Length != 1 ||
            game.panelDerrota.GetComponentsInChildren<Button>(true).Length != 1)
        {
            Debug.LogError("Victoria/derrota perdió referencias o botones.", game);
            errors++;
        }
        if (UnityEngine.Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsInactive.Include).Length != 1)
        {
            Debug.LogError("Debe existir exactamente un EventSystem.");
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

    public static void ValidateRequestedRuntimeFlows()
    {
        ValidatePauseFlow();
        ValidateTimeoutDefeat();
        ValidateErrorsDefeat();
        ValidateNormalVictory();
        Time.timeScale = 1f;
        Debug.Log("REQUESTED_RUNTIME_FLOWS pause=ok timeout=ok errors=ok victory=ok");
    }

    private static void ValidatePauseFlow()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameManager game = UnityEngine.Object.FindAnyObjectByType<GameManager>(FindObjectsInactive.Include);
        MenuPausaUI pause = UnityEngine.Object.FindAnyObjectByType<MenuPausaUI>(FindObjectsInactive.Include);
        PlayerMovement player = UnityEngine.Object.FindAnyObjectByType<PlayerMovement>(FindObjectsInactive.Include);
        SetPrivate(game, "juegoTerminado", false);
        SetPrivate(game, "zonaActiva", true);
        SetPrivate(game, "cronometroActivo", true);
        SetPrivate(game, "cronometroPausado", false);
        player.enabled = true;
        pause.panelPausa.SetActive(false);
        Time.timeScale = 1f;
        InvokePrivate(pause, "Start");
        InvokePrivate(pause, "AbrirPausa");
        if (!pause.panelPausa.activeSelf || player.enabled || game.CronometroEnMarcha || Time.timeScale != 0f)
            throw new InvalidOperationException("La apertura real de pausa no bloqueó correctamente el gameplay.");
        pause.Reanudar();
        if (pause.panelPausa.activeSelf || !player.enabled || !game.CronometroEnMarcha || Time.timeScale != 1f)
            throw new InvalidOperationException("Reanudar no restauró correctamente gameplay y cronómetro.");
    }

    private static void ValidateTimeoutDefeat()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameManager game = PrepareActiveGame();
        game.tiempoRestante = 0f;
        InvokePrivate(game, "Update");
        AssertDefeat(game, "TIEMPO AGOTADO", "tiempo de inspección");
    }

    private static void ValidateErrorsDefeat()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameManager game = PrepareActiveGame();
        game.RegistrarError();
        game.RegistrarError();
        game.RegistrarError();
        AssertDefeat(game, "INSPECCIÓN INTERRUMPIDA", "máximo de errores");
    }

    private static void ValidateNormalVictory()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameManager game = PrepareActiveGame();
        game.indiceZonaActual = 1;
        game.objetivosTotales = 1;
        SetPrivate(game, "riesgo", 100);
        SetPrivate(game, "objetivosCompletados", 0);
        game.RegistrarAcierto(80);
        if (!game.JuegoTerminado || !game.panelVictoria.activeSelf || game.panelDerrota.activeSelf ||
            (game.panelFinalJuego != null && game.panelFinalJuego.gameObject.activeSelf))
            throw new InvalidOperationException("La victoria normal no mostró exclusivamente PanelVictoria.");
        game.ContinuarDespuesDeVictoria();
        if (game.JuegoTerminado || game.panelVictoria.activeSelf || !game.playerMovement.enabled)
            throw new InvalidOperationException("Continuar tras victoria no restauró la exploración.");
    }

    private static GameManager PrepareActiveGame()
    {
        GameManager game = UnityEngine.Object.FindAnyObjectByType<GameManager>(FindObjectsInactive.Include);
        if (game == null || game.playerMovement == null) throw new InvalidOperationException("Falta GameManager/Player.");
        Time.timeScale = 1f;
        SetPrivate(game, "juegoTerminado", false);
        SetPrivate(game, "zonaActiva", true);
        SetPrivate(game, "cronometroActivo", true);
        SetPrivate(game, "cronometroPausado", false);
        SetPrivate(game, "victoriaPendiente", false);
        game.playerMovement.enabled = true;
        if (game.panelDerrota != null) game.panelDerrota.SetActive(false);
        if (game.panelVictoria != null) game.panelVictoria.SetActive(false);
        if (game.panelFinalJuego != null) game.panelFinalJuego.gameObject.SetActive(false);
        return game;
    }

    private static void AssertDefeat(GameManager game, string titlePart, string messagePart)
    {
        if (!game.JuegoTerminado || !game.panelDerrota.activeSelf || game.playerMovement.enabled ||
            game.CronometroEnMarcha || game.panelVictoria.activeSelf ||
            !game.tituloDerrota.text.Contains(titlePart) || !game.textoMotivoDerrota.text.Contains(messagePart))
            throw new InvalidOperationException("La derrota no reflejó su causa real o no bloqueó el gameplay.");
    }

    private static void SetPrivate(object target, string name, object value)
    {
        FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null) throw new MissingFieldException(target.GetType().Name, name);
        field.SetValue(target, value);
    }

    private static void InvokePrivate(object target, string name)
    {
        MethodInfo method = target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
        if (method == null) throw new MissingMethodException(target.GetType().Name, name);
        method.Invoke(target, null);
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
        RenderPanel(camera, canvas, Find(scene, "PanelPausa"), "PausePreview_1366.png");
        RenderPanel(camera, canvas, Find(scene, "PanelVictoria"), "VictoryPreview_1366.png");
        RenderDefeatPreview(camera, canvas, Find(scene, "PanelDerrota"), true, "DefeatTimePreview_1366.png");
        RenderDefeatPreview(camera, canvas, Find(scene, "PanelDerrota"), false, "DefeatErrorsPreview_1366.png");
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
        ui.AjustarLayoutOpciones();
        RenderPanel(camera, canvas, panel, fileName);
        if (title != null) title.text = previousTitle;
        ui.textoBoton1.text = previousAnswer;
        ui.AjustarLayoutOpciones();
    }

    private static void RenderDefeatPreview(Camera camera, Canvas canvas, GameObject panel,
        bool timeout, string fileName)
    {
        GameManager game = UnityEngine.Object.FindAnyObjectByType<GameManager>(FindObjectsInactive.Include);
        if (game == null || game.tituloDerrota == null || game.textoMotivoDerrota == null)
            throw new InvalidOperationException("PanelDerrota no conserva sus textos enlazados.");
        string oldTitle = game.tituloDerrota.text;
        string oldMessage = game.textoMotivoDerrota.text;
        game.tituloDerrota.text = timeout ? "¡TIEMPO AGOTADO!" : "¡INSPECCIÓN INTERRUMPIDA!";
        game.textoMotivoDerrota.text = timeout
            ? "Se terminó el tiempo de inspección. Inténtalo de nuevo y revisa los riesgos con mayor rapidez."
            : "Alcanzaste el máximo de errores permitidos. Inténtalo nuevamente y observa cada situación con atención.";
        RenderPanel(camera, canvas, panel, fileName);
        game.tituloDerrota.text = oldTitle;
        game.textoMotivoDerrota.text = oldMessage;
    }

    private static void RenderPanel(Camera camera, Canvas canvas, GameObject panel, string fileName,
        int width = 1366, int height = 768)
    {
        if (panel == null) throw new InvalidOperationException("No se encontró el panel para la previsualización.");
        foreach (Transform child in canvas.transform)
            child.gameObject.SetActive(false);
        Transform ancestor = panel.transform.parent;
        while (ancestor != null && ancestor != canvas.transform)
        {
            ancestor.gameObject.SetActive(true);
            ancestor = ancestor.parent;
        }
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
        SetRect((RectTransform)panel.transform, new Vector2(800f, 720f), new Vector2(0f, -8f));
        Image rootImage = panel.GetComponent<Image>();
        if (rootImage != null) rootImage.color = Color.clear;
        RectTransform border = ChildRect(panel.transform, "BordeMarron");
        RectTransform background = ChildRect(panel.transform, "FondoCrema");
        RectTransform titlePanel = ChildRect(panel.transform, "PanelTitulo");
        RectTransform line = ChildRect(panel.transform, "LineaTitulo");
        if (border != null)
        {
            SetRect(border, new Vector2(780f, 700f), Vector2.zero);
            SetImage(border.GetComponent<Image>(), DarkPanelSprite(), Color.white);
            SetShadow(border.gameObject, new Color(0f, 0f, 0f, 0.38f), new Vector2(9f, -10f));
            border.SetAsFirstSibling();
        }
        if (background != null)
        {
            SetRect(background, new Vector2(750f, 670f), Vector2.zero);
            SetImage(background.GetComponent<Image>(), PanelSprite(), Cream);
            background.SetSiblingIndex(1);
        }
        RectTransform titleBackground = Layer((RectTransform)panel.transform, "Decor_CabeceraDecision",
            null, DeepGreen, new Vector2(730f, 92f), new Vector2(0f, 287f));
        titleBackground.SetSiblingIndex(2);
        if (titlePanel != null)
        {
            SetRect(titlePanel, new Vector2(730f, 92f), new Vector2(0f, 287f));
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
            SetRect(line, new Vector2(630f, 4f), new Vector2(0f, 122f));
            SetImage(line.GetComponent<Image>(), null, WarmGold);
        }

        VentanaDecisionUI ui = UnityEngine.Object.FindObjectsByType<VentanaDecisionUI>(FindObjectsInactive.Include)
            .FirstOrDefault(v => v.ventanaDecision == panel);
        if (ui == null) throw new InvalidOperationException("VentanaDecision no conserva su controlador.");
        SetRect(ui.descripcionObjeto.rectTransform, new Vector2(670f, 112f), new Vector2(0f, 184f));
        SetText(ui.descripcionObjeto, Ink, 21f, FontStyles.Normal, TextAlignmentOptions.Center);
        ConfigureResponsiveText(ui.descripcionObjeto, 17f, 21f, TextWrappingModes.Normal, 3f,
            new Vector4(18f, 7f, 18f, 7f));

        TMP_Text[] labels = { ui.textoBoton1, ui.textoBoton2, ui.textoBoton3 };
        float[] ys = { 51f, -11f, -73f };
        for (int index = 0; index < labels.Length; index++)
        {
            Button button = labels[index].GetComponentInParent<Button>(true);
            if (button == null) continue;
            SetRect((RectTransform)button.transform, new Vector2(600f, 54f), new Vector2(0f, ys[index]));
            StyleAnswerButton(button);
            SetText(labels[index], DeepGreen, 20f, FontStyles.Bold, TextAlignmentOptions.Center);
            ConfigureResponsiveText(labels[index], 16f, 20f, TextWrappingModes.Normal, 1f,
                new Vector4(26f, 10f, 26f, 10f));
        }

        RectTransform feedback = ui.textoFeedback.transform.parent as RectTransform;
        if (feedback != null)
        {
            SetRect(feedback, new Vector2(660f, 82f), new Vector2(0f, -260f));
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

    private static void PolishPause(GameObject panel)
    {
        if (panel == null) throw new InvalidOperationException("No se encontró PanelPausa.");
        Image overlay = panel.GetComponent<Image>();
        if (overlay != null) overlay.color = new Color32(8, 25, 18, 205);
        RectTransform root = (RectTransform)panel.transform;
        RectTransform card = ChildRect(root, "TarjetaPausa");
        if (card == null) throw new InvalidOperationException("PanelPausa no conserva TarjetaPausa.");
        SetRect(card, new Vector2(600f, 520f), Vector2.zero);
        SetImage(card.GetComponent<Image>(), PanelSprite(), Cream);
        SetShadow(card.gameObject, new Color(0f, 0f, 0f, 0.45f), new Vector2(9f, -10f));
        RectTransform header = Layer(card, "Decor_CabeceraPausa", null, DeepGreen,
            new Vector2(560f, 104f), new Vector2(0f, 184f));
        header.SetAsFirstSibling();
        Layer(card, "Decor_LineaPausa", null, WarmGold, new Vector2(470f, 4f), new Vector2(0f, 127f));
        TMP_Text title = card.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(t => t.name == "TituloPausa");
        if (title != null)
        {
            SetRect(title.rectTransform, new Vector2(440f, 70f), new Vector2(0f, 184f));
            SetText(title, White, 48f, FontStyles.Bold, TextAlignmentOptions.Center);
            title.transform.SetAsLastSibling();
        }
        string[] names = { "BotonReanudar", "BotonReiniciar", "BotonSalirPausa" };
        float[] ys = { 63f, -31f, -125f };
        for (int i = 0; i < names.Length; i++)
        {
            Button button = card.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name == names[i]);
            if (button == null) continue;
            SetRect((RectTransform)button.transform, new Vector2(360f, 68f), new Vector2(0f, ys[i]));
            StyleWoodButton(button, 23f);
            button.transform.SetAsLastSibling();
        }
        SetTropicalSprite(card, "Decor_PausaIzquierda", new Vector2(-246f, 184f), 34f, false);
        SetTropicalSprite(card, "Decor_PausaDerecha", new Vector2(246f, 184f), 34f, true);
    }

    private static void PolishVictory(GameObject panel)
    {
        if (panel == null) throw new InvalidOperationException("No se encontró PanelVictoria.");
        Image overlay = panel.GetComponent<Image>();
        if (overlay != null) overlay.color = new Color32(8, 25, 18, 215);
        RectTransform root = (RectTransform)panel.transform;
        RectTransform card = ChildRect(root, "TarjetaVictoria");
        if (card == null) throw new InvalidOperationException("PanelVictoria no conserva TarjetaVictoria.");
        SetRect(card, new Vector2(820f, 600f), Vector2.zero);
        SetImage(card.GetComponent<Image>(), PanelSprite(), Cream);
        SetShadow(card.gameObject, new Color(0f, 0f, 0f, 0.46f), new Vector2(10f, -11f));
        card.SetAsFirstSibling();
        RectTransform header = Layer(root, "Decor_CabeceraVictoria", null, DeepGreen,
            new Vector2(780f, 116f), new Vector2(0f, 222f));
        header.SetSiblingIndex(1);
        Layer(root, "Decor_LineaVictoria", null, WarmGold, new Vector2(660f, 4f), new Vector2(0f, 158f));

        TMP_Text title = panel.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(t => t.name == "TituloResultados");
        if (title != null)
        {
            SetRect(title.rectTransform, new Vector2(660f, 82f), new Vector2(0f, 222f));
            SetText(title, White, 40f, FontStyles.Bold, TextAlignmentOptions.Center);
            title.transform.SetAsLastSibling();
        }
        string[] statNames = { "ResultadoRiesgo", "ResultadoObjetivos", "ResultadoAciertos", "ResultadoErrores" };
        Vector2[] positions = { new Vector2(-185f, 68f), new Vector2(185f, 68f), new Vector2(-185f, -8f), new Vector2(185f, -8f) };
        for (int i = 0; i < statNames.Length; i++)
        {
            TMP_Text stat = panel.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(t => t.name == statNames[i]);
            if (stat == null) continue;
            SetRect(stat.rectTransform, new Vector2(330f, 54f), positions[i]);
            SetText(stat, Ink, 23f, FontStyles.Bold, TextAlignmentOptions.Center);
            ConfigureResponsiveText(stat, 18f, 23f, TextWrappingModes.Normal, 0f, new Vector4(8f, 5f, 8f, 5f));
            stat.transform.SetAsLastSibling();
        }
        Button button = panel.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name == "BotonContinuar");
        if (button != null)
        {
            SetRect((RectTransform)button.transform, new Vector2(380f, 70f), new Vector2(0f, -216f));
            StyleWoodButton(button, 23f);
            button.transform.SetAsLastSibling();
        }
        SetTropicalSprite(root, "Decor_VictoriaIzquierda", new Vector2(-350f, 222f), 36f, false);
        SetTropicalSprite(root, "Decor_VictoriaDerecha", new Vector2(350f, 222f), 36f, true);
    }

    private static void PolishDefeat(GameObject panel)
    {
        if (panel == null) throw new InvalidOperationException("No se encontró PanelDerrota.");
        Image overlay = panel.GetComponent<Image>();
        if (overlay != null) overlay.color = new Color32(20, 18, 14, 218);
        RectTransform root = (RectTransform)panel.transform;
        RectTransform card = Layer(root, "Decor_TarjetaDerrota", PanelSprite(), Cream,
            new Vector2(780f, 500f), Vector2.zero);
        card.SetAsFirstSibling();
        SetShadow(card.gameObject, new Color(0f, 0f, 0f, 0.48f), new Vector2(10f, -11f));
        RectTransform header = Layer(root, "Decor_CabeceraDerrota", null, new Color32(83, 55, 39, 255),
            new Vector2(740f, 116f), new Vector2(0f, 172f));
        header.SetSiblingIndex(1);
        Layer(root, "Decor_LineaDerrota", null, WarmGold, new Vector2(620f, 4f), new Vector2(0f, 108f));

        GameManager game = UnityEngine.Object.FindAnyObjectByType<GameManager>(FindObjectsInactive.Include);
        if (game == null || game.textoMotivoDerrota == null)
            throw new InvalidOperationException("PanelDerrota no conserva GameManager/textoMotivoDerrota.");
        TMP_Text title = CreateOrGetText(root, "TituloDerrota", game.textoMotivoDerrota);
        title.text = "¡TIEMPO AGOTADO!";
        SetRect(title.rectTransform, new Vector2(620f, 78f), new Vector2(0f, 172f));
        SetText(title, White, 38f, FontStyles.Bold, TextAlignmentOptions.Center);
        ConfigureResponsiveText(title, 27f, 38f, TextWrappingModes.NoWrap, 0f,
            new Vector4(8f, 5f, 8f, 5f));
        title.transform.SetAsLastSibling();
        game.tituloDerrota = title as TextMeshProUGUI;

        SetRect(game.textoMotivoDerrota.rectTransform, new Vector2(640f, 122f), new Vector2(0f, 30f));
        SetText(game.textoMotivoDerrota, Ink, 22f, FontStyles.Normal, TextAlignmentOptions.Center);
        ConfigureResponsiveText(game.textoMotivoDerrota, 18f, 22f, TextWrappingModes.Normal, 5f,
            new Vector4(18f, 10f, 18f, 10f));
        game.textoMotivoDerrota.transform.SetAsLastSibling();
        Button button = panel.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name == "BotonReintentar");
        if (button != null)
        {
            SetRect((RectTransform)button.transform, new Vector2(380f, 70f), new Vector2(0f, -150f));
            StyleWoodButton(button, 23f);
            button.transform.SetAsLastSibling();
        }
        SetTropicalSprite(root, "Decor_DerrotaIzquierda", new Vector2(-330f, 172f), 34f, false);
        SetTropicalSprite(root, "Decor_DerrotaDerecha", new Vector2(330f, 172f), 34f, true);
        EditorUtility.SetDirty(game);
    }

    private static TMP_Text CreateOrGetText(RectTransform parent, string name, TMP_Text template)
    {
        Transform existing = parent.Find(name);
        TextMeshProUGUI text;
        if (existing != null)
        {
            text = existing.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            GameObject item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            item.transform.SetParent(parent, false);
            item.layer = parent.gameObject.layer;
            text = item.GetComponent<TextMeshProUGUI>();
        }
        text.font = template.font;
        text.raycastTarget = false;
        return text;
    }

    private static void ReplaceDecisionDecor(GameObject panel)
    {
        if (panel == null) throw new InvalidOperationException("No se encontró VentanaDecision.");
        RectTransform root = (RectTransform)panel.transform;
        RemoveDecor(root, "Decor_AcentoIzquierdo");
        RemoveDecor(root, "Decor_AcentoDerecho");
        SetTropicalSprite(root, "Decor_HojasDecisionIzq", new Vector2(-350f, 287f), 26f, false);
        SetTropicalSprite(root, "Decor_HojasDecisionDer", new Vector2(350f, 287f), 26f, true);
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
            Color.white, new Color32(157, 166, 145, 180));
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
