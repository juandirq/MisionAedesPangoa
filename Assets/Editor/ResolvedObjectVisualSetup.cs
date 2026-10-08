using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public static class ResolvedObjectVisualSetup
{
    private const string ScenePath = "Assets/Scenes/Zona1_Escuela.unity";

    private sealed class Mapping
    {
        public readonly string Interactable;
        public readonly string Renderer;
        public readonly string FixedSprite;

        public Mapping(string interactable, string renderer, string fixedSprite)
        {
            Interactable = interactable;
            Renderer = renderer;
            FixedSprite = "Assets/Sprites/Objetos ya arreglados/" + fixedSprite;
        }
    }

    private static readonly Mapping[] Mappings =
    {
        new Mapping("Zona1_TachoBasura", "Tacho de basura", "Tacho de basura arreglado.png"),
        new Mapping("Zona1_CuboPintura", "Balde de pintura", "Balde de pintura arreglada.png"),
        new Mapping("Zona1_Lavadero", "Lavadero", "LAVADERO ARREGLADA FINAL.png"),
        new Mapping("Zona2_Tanque", "Tanque", "Tanque arreglado.png"),
        new Mapping("Zona2_Canaleta", "Canaleta", "Canaleta arreglado.png"),
        new Mapping("Zona2_Maceta", "Florero", "Florero arreglado.png"),
        new Mapping("Zona2_Balde", "Balde", "Balde arreglado.png"),
        new Mapping("Zona2_Botella", "Botella", "Botella arreglado.png"),
        new Mapping("Zona2_Llanta", "Llanta", "Llanta arreglada.png"),
        new Mapping("Zona3_Emgranaje", "Alcantarilla", "ALCANTARILLA ARREGLADA FINAL.png"),
        new Mapping("Zona3_Carretilla", "Carretilla", "CARRETILLA ARREGLADA FINAL.png"),
        new Mapping("Zona3_ValdeComunitario", "Balde comunitario", "Balde comunitario arreglado.png"),
        new Mapping("Zona3_Bolsas", "Lona plastica", "LONA ARREGLADA FINAL.png")
    };

    [MenuItem("Mision Aedes/Visual resuelto/Aplicar piloto canaleta")]
    public static void ApplyPilotCanaleta()
    {
        ApplyMappings(Mappings.Where(m => m.Interactable == "Zona2_Canaleta").ToArray());
    }

    [MenuItem("Mision Aedes/Visual resuelto/Aplicar a los 13 objetos")]
    public static void ApplyAll()
    {
        ApplyMappings(Mappings);
    }

    [MenuItem("Mision Aedes/UI/Configurar volumen en pausa")]
    public static void ApplyPauseVolumeControls()
    {
        OpenScene();
        MenuPausaUI pause = UnityEngine.Object.FindAnyObjectByType<MenuPausaUI>();
        MainMenuController main = UnityEngine.Object.FindAnyObjectByType<MainMenuController>();
        AudioManager audio = UnityEngine.Object.FindAnyObjectByType<AudioManager>();
        GameObject card = FindUnique("TarjetaPausa");
        if (pause == null || main == null || audio == null || main.sliderMusica == null || main.sliderSfx == null)
            throw new InvalidOperationException("Faltan referencias para construir los controles de volumen.");

        GameObject old = FindOptional("ControlesVolumenPausa");
        if (old != null) UnityEngine.Object.DestroyImmediate(old);

        GameObject container = new GameObject("ControlesVolumenPausa", typeof(RectTransform));
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.SetParent(card.transform, false);
        SetRect(containerRect, Vector2.zero, new Vector2(540f, 150f));

        Slider music = UnityEngine.Object.Instantiate(main.sliderMusica, containerRect);
        music.name = "SliderMusicaPausa";
        SetRect((RectTransform)music.transform, new Vector2(45f, 38f), new Vector2(330f, 42f));
        music.minValue = 0f; music.maxValue = 1f; music.wholeNumbers = false;
        music.onValueChanged.RemoveAllListeners();

        Slider effects = UnityEngine.Object.Instantiate(main.sliderSfx, containerRect);
        effects.name = "SliderEfectosPausa";
        SetRect((RectTransform)effects.transform, new Vector2(45f, -38f), new Vector2(330f, 42f));
        effects.minValue = 0f; effects.maxValue = 1f; effects.wholeNumbers = false;
        effects.onValueChanged.RemoveAllListeners();

        TMP_Text template = FindUnique("TituloPausa").GetComponent<TMP_Text>();
        TMP_Text musicLabel = CreatePauseText("EtiquetaMusicaPausa", "MÚSICA", containerRect, template,
            new Vector2(-215f, 38f), new Vector2(120f, 42f), TextAlignmentOptions.MidlineLeft);
        TMP_Text effectsLabel = CreatePauseText("EtiquetaEfectosPausa", "EFECTOS", containerRect, template,
            new Vector2(-215f, -38f), new Vector2(120f, 42f), TextAlignmentOptions.MidlineLeft);
        TMP_Text musicValue = CreatePauseText("ValorMusicaPausa", "75%", containerRect, template,
            new Vector2(245f, 38f), new Vector2(74f, 42f), TextAlignmentOptions.Center);
        TMP_Text effectsValue = CreatePauseText("ValorEfectosPausa", "85%", containerRect, template,
            new Vector2(245f, -38f), new Vector2(74f, 42f), TextAlignmentOptions.Center);

        RectTransform cardRect = (RectTransform)card.transform;
        cardRect.sizeDelta = new Vector2(680f, 700f);
        Move("TituloPausa", 274f); Move("Decor_CabeceraPausa", 274f);
        Move("Decor_PausaIzquierda", 274f); Move("Decor_PausaDerecha", 274f);
        Move("Decor_LineaPausa", 217f);
        containerRect.anchoredPosition = new Vector2(0f, 100f);
        Move("BotonReanudar", -55f); Move("BotonReiniciar", -145f); Move("BotonSalirPausa", -235f);

        Undo.RecordObject(pause, "Configurar volumen del menú de pausa");
        pause.sliderMusica = music;
        pause.sliderEfectos = effects;
        pause.valorMusica = musicValue;
        pause.valorEfectos = effectsValue;
        pause.audioManager = audio;
        EditorUtility.SetDirty(pause);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("PAUSE_VOLUME_APPLY_OK sliders=2 sources=6");
    }

    public static void ApplyPendingFinals()
    {
        string[] pending = { "Zona1_Lavadero", "Zona3_Emgranaje", "Zona3_Carretilla", "Zona3_Bolsas" };
        ApplyMappings(Mappings.Where(mapping => pending.Contains(mapping.Interactable)).ToArray());
    }

    public static void ApplyCompatibleOnly()
    {
        OpenScene();
        Mapping[] compatible = Mappings.Where(IsSpriteCompatible).ToArray();
        foreach (Mapping skipped in Mappings.Except(compatible))
            Debug.LogWarning("RESOLVED_VISUAL_SKIPPED incompatible=" + skipped.Interactable);
        ApplyMappings(compatible);
    }

    public static void ValidatePilot()
    {
        ValidateMappings(Mappings.Where(m => m.Interactable == "Zona2_Canaleta").ToArray(), 1);
    }

    public static void ValidateAll()
    {
        ValidateMappings(Mappings, Mappings.Length);
    }

    public static void ValidateCompatibleOnly()
    {
        OpenScene();
        Mapping[] compatible = Mappings.Where(IsSpriteCompatible).ToArray();
        ValidateMappings(compatible, compatible.Length);
    }

    public static void BatchHealthCheck()
    {
        OpenScene();
        Collider2D[] colliders = Resources.FindObjectsOfTypeAll<Collider2D>()
            .Where(collider => collider.gameObject.scene.IsValid() && collider.gameObject.scene.isLoaded)
            .ToArray();
        int triggers = colliders.Count(collider => collider.isTrigger);
        Sprite canaletaResuelta = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Sprites/Objetos ya arreglados/Canaleta arreglado.png");

        if (colliders.Length != 215 || triggers != 25)
            throw new InvalidOperationException($"Conteo fisico inesperado: colliders={colliders.Length}, triggers={triggers}.");
        if (canaletaResuelta == null)
            throw new InvalidOperationException("No se pudo cargar Canaleta arreglado.png.");

        AssetDatabase.SaveAssets();
        Debug.Log("BATCH_HEALTH_OK colliders=215 triggers=25 sprite=Canaleta arreglado");
    }

    public static void ValidateCheckpointSystem()
    {
        OpenScene();
        GameManager manager = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        RutaLinealController route = UnityEngine.Object.FindAnyObjectByType<RutaLinealController>();
        ObjetoInteractuable[] interactables = Resources.FindObjectsOfTypeAll<ObjetoInteractuable>()
            .Where(item => item.gameObject.scene.IsValid() && item.gameObject.scene.isLoaded)
            .ToArray();
        ObjetoInteractuable[] interactablesDeZona = interactables
            .Where(item => item.indiceZona >= 1 && item.indiceZona <= 3)
            .ToArray();
        ObjetoResueltoVisual[] visuals = Resources.FindObjectsOfTypeAll<ObjetoResueltoVisual>()
            .Where(item => item.gameObject.scene.IsValid() && item.gameObject.scene.isLoaded)
            .ToArray();
        Collider2D[] colliders = Resources.FindObjectsOfTypeAll<Collider2D>()
            .Where(item => item.gameObject.scene.IsValid() && item.gameObject.scene.isLoaded)
            .ToArray();

        if (manager == null || manager.playerMovement == null || manager.progresoZonas == null || manager.rutaLineal == null)
            throw new InvalidOperationException("GameManager no tiene todas las referencias requeridas por checkpoints.");
        if (route == null || route.bloqueoPasoZona2 == null || route.bloqueoPasoZona3 == null)
            throw new InvalidOperationException("Faltan los bloqueos lineales existentes.");
        if (interactablesDeZona.Length != 13 || visuals.Length != 13)
            throw new InvalidOperationException($"Conteo inesperado: interactuables de zona={interactablesDeZona.Length}, visuales={visuals.Length}.");
        if (visuals.Any(visual => visual.interactuable == null || visual.spriteObjetivo == null || visual.spriteResuelto == null))
            throw new InvalidOperationException("Hay una transformación visual incompleta.");
        if (colliders.Length != 215 || colliders.Count(item => item.isTrigger) != 25)
            throw new InvalidOperationException("El conteo físico cambió.");
        if (interactablesDeZona.Count(item => item.indiceZona == 1) != 3 ||
            interactablesDeZona.Count(item => item.indiceZona == 2) != 6 ||
            interactablesDeZona.Count(item => item.indiceZona == 3) != 4)
            throw new InvalidOperationException("La distribución de los 13 interactuables por zona es incorrecta.");

        Debug.Log("CHECKPOINT_VALIDATE_OK interactuables=13 visuales=13 colliders=215 triggers=25 zonas=3/6/4");
    }

    public static void ValidateFinalReview()
    {
        ValidateCheckpointSystem();

        GameObject pauseButtonGo = FindUnique("BotonReiniciar");
        Button pauseButton = pauseButtonGo.GetComponent<Button>();
        TMP_Text pauseLabel = pauseButtonGo.GetComponentInChildren<TMP_Text>(true);
        MenuPausaUI pauseMenu = UnityEngine.Object.FindAnyObjectByType<MenuPausaUI>();
        GameManager manager = UnityEngine.Object.FindAnyObjectByType<GameManager>();

        if (pauseButton == null || pauseLabel == null || pauseMenu == null || manager == null)
            throw new InvalidOperationException("Faltan referencias del botón de reinicio o del menú de pausa.");
        if (pauseLabel.text != "REINICIAR NIVEL")
            throw new InvalidOperationException("La etiqueta del botón de pausa no es REINICIAR NIVEL.");
        if (pauseButton.onClick.GetPersistentEventCount() != 1 ||
            pauseButton.onClick.GetPersistentTarget(0) != pauseMenu ||
            pauseButton.onClick.GetPersistentMethodName(0) != nameof(MenuPausaUI.ReiniciarZona))
            throw new InvalidOperationException("El botón REINICIAR NIVEL no apunta a MenuPausaUI.ReiniciarZona.");

        Button[] buttons = Resources.FindObjectsOfTypeAll<Button>()
            .Where(button => button.gameObject.scene.IsValid() && button.gameObject.scene.isLoaded)
            .ToArray();
        bool retryUsesCheckpoint = buttons.Any(button =>
            Enumerable.Range(0, button.onClick.GetPersistentEventCount()).Any(index =>
                button.onClick.GetPersistentTarget(index) == manager &&
                button.onClick.GetPersistentMethodName(index) == nameof(GameManager.ReiniciarZona)));
        if (!retryUsesCheckpoint)
            throw new InvalidOperationException("No se encontró el botón Reintentar conectado a GameManager.ReiniciarZona.");

        GameObject[] sceneObjects = Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(go => go.scene.IsValid() && go.scene.isLoaded)
            .ToArray();
        int missingScripts = sceneObjects.Sum(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount);
        if (missingScripts != 0)
            throw new InvalidOperationException("La escena contiene componentes Missing Script: " + missingScripts);

        RectTransform rect = pauseButton.transform as RectTransform;
        if (rect == null || rect.sizeDelta.x < 300f || rect.sizeDelta.y < 60f)
            throw new InvalidOperationException("El botón REINICIAR NIVEL no conserva el tamaño esperado del menú.");

        Debug.Log("FINAL_REVIEW_OK pauseLabel=REINICIAR_NIVEL pauseEvent=MenuPausaUI.ReiniciarZona retryEvent=GameManager.ReiniciarZona missingScripts=0");
    }

    public static void ValidatePauseVolumeControls()
    {
        ValidateFinalReview();
        MenuPausaUI pause = UnityEngine.Object.FindAnyObjectByType<MenuPausaUI>();
        AudioManager audio = UnityEngine.Object.FindAnyObjectByType<AudioManager>();
        AudioSource[] sources = Resources.FindObjectsOfTypeAll<AudioSource>()
            .Where(source => source.gameObject.scene.IsValid() && source.gameObject.scene.isLoaded).ToArray();
        if (pause == null || pause.sliderMusica == null || pause.sliderEfectos == null ||
            pause.valorMusica == null || pause.valorEfectos == null || pause.audioManager != audio)
            throw new InvalidOperationException("Referencias incompletas en los controles de volumen de pausa.");
        if (sources.Length != 6 || audio == null || audio.musicaA == null || audio.musicaB == null ||
            audio.sfxGeneral == null || audio.dialogo == null || audio.fuentePisadas == null || audio.fuenteRio == null)
            throw new InvalidOperationException("Las seis fuentes centralizadas no están completamente clasificadas.");
        if (pause.sliderMusica.minValue != 0f || pause.sliderMusica.maxValue != 1f ||
            pause.sliderEfectos.minValue != 0f || pause.sliderEfectos.maxValue != 1f)
            throw new InvalidOperationException("Los sliders no cubren el rango 0-100%.");
        Debug.Log("PAUSE_VOLUME_VALIDATE_OK sliders=2 musicSources=2 sfxSources=4 playerPrefs=2");
    }

    public static void DumpRouteGeometry()
    {
        OpenScene();
        Physics2D.SyncTransforms();
        string[] names = { "Player", "BloqueoPasoZona2", "BloqueoPasoZona3", "TriggerEntradaZona2",
            "TriggerEntradaZona3", "CheckpointSalidaZona1", "CheckpointSalidaZona2", "ControlZona1" };
        foreach (string name in names)
        {
            GameObject go = FindOptional(name);
            if (go == null) { Debug.Log("GEOMETRY_MISSING " + name); continue; }
            Collider2D[] own = go.GetComponents<Collider2D>();
            Debug.Log($"GEOMETRY name={name} active={go.activeInHierarchy} pos={go.transform.position} " +
                string.Join(" | ", own.Select(c => $"type={c.GetType().Name} trigger={c.isTrigger} bounds={c.bounds}")));
        }
        foreach (Collider2D trigger in Resources.FindObjectsOfTypeAll<Collider2D>()
            .Where(c => c.gameObject.scene.IsValid() && c.gameObject.scene.isLoaded && c.isTrigger &&
                (c.gameObject.name.Contains("Zona") || c.gameObject.name.Contains("Checkpoint"))))
            Debug.Log($"TRIGGER_GEOMETRY name={trigger.gameObject.name} active={trigger.gameObject.activeInHierarchy} bounds={trigger.bounds}");
        Debug.Log("GEOMETRY_DUMP_OK");
    }

    public static void DumpZone3CorridorObstacles()
    {
        OpenScene();
        Bounds corridor = new Bounds(new Vector3(5.5f, 8.0f, 0f), new Vector3(12f, 3f, 1f));
        foreach (Collider2D collider in Resources.FindObjectsOfTypeAll<Collider2D>()
            .Where(c => c.gameObject.scene.IsValid() && c.gameObject.scene.isLoaded && !c.isTrigger && c.bounds.Intersects(corridor)))
            Debug.Log($"CORRIDOR_OBSTACLE name={collider.gameObject.name} path={GetPath(collider.transform)} active={collider.gameObject.activeInHierarchy} bounds={collider.bounds}");
        Debug.Log("CORRIDOR_OBSTACLE_DUMP_OK");
    }

    private static string GetPath(Transform transform)
    {
        return transform.parent == null ? transform.name : GetPath(transform.parent) + "/" + transform.name;
    }

    public static void ApplySafeZoneTransitions()
    {
        OpenScene();
        RutaLinealController route = UnityEngine.Object.FindAnyObjectByType<RutaLinealController>();
        GameObject trigger1 = FindUnique("TriggerEntradaZona1");
        if (route == null || trigger1.GetComponent<Collider2D>() == null)
            throw new InvalidOperationException("Faltan RutaLinealController o TriggerEntradaZona1.");

        route.limiteEntradaZona1 = trigger1.GetComponent<Collider2D>();
        route.puntoSeguroZona1 = CreateSafePoint(route.transform, "CheckpointSeguroZona1", new Vector3(12.42f, 42.0f, 0f));
        route.puntoSeguroZona2 = CreateSafePoint(route.transform, "CheckpointSeguroZona2", new Vector3(-3.05f, 7.94f, 0f));
        route.puntoSeguroZona3 = CreateSafePoint(route.transform, "CheckpointSeguroZona3", new Vector3(13.25f, 8.05f, 0f));
        EditorUtility.SetDirty(route);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("SAFE_TRANSITIONS_APPLY_OK points=3 reusedColliders=3 addedColliders=0");
    }

    public static void ValidateSafeZoneTransitions()
    {
        ValidatePauseVolumeControls();
        RutaLinealController route = UnityEngine.Object.FindAnyObjectByType<RutaLinealController>();
        PlayerMovement player = UnityEngine.Object.FindAnyObjectByType<PlayerMovement>();
        if (route == null || player == null || route.limiteEntradaZona1 == null ||
            route.puntoSeguroZona1 == null || route.puntoSeguroZona2 == null || route.puntoSeguroZona3 == null)
            throw new InvalidOperationException("Configuración incompleta de transiciones seguras.");
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        foreach (Transform point in new[] { route.puntoSeguroZona1, route.puntoSeguroZona2, route.puntoSeguroZona3 })
        {
            Collider2D[] overlaps = Physics2D.OverlapBoxAll(point.position, playerCollider.bounds.size, 0f)
                .Where(c => c != playerCollider && !c.isTrigger && c.gameObject.activeInHierarchy).ToArray();
            if (overlaps.Length != 0)
                throw new InvalidOperationException(point.name + " se superpone a: " + string.Join(", ", overlaps.Select(c => c.name)));
        }
        Collider2D trigger2 = FindUnique("TriggerEntradaZona2").GetComponent<Collider2D>();
        Collider2D trigger3 = FindUnique("TriggerEntradaZona3").GetComponent<Collider2D>();
        if (trigger2.bounds.Contains(route.puntoSeguroZona2.position) || trigger3.bounds.Contains(route.puntoSeguroZona3.position) ||
            route.limiteEntradaZona1.bounds.Contains(route.puntoSeguroZona1.position))
            throw new InvalidOperationException("Un checkpoint seguro todavía está dentro de un trigger de entrada.");
        Debug.Log("SAFE_TRANSITIONS_VALIDATE_OK safePoints=3 overlap=0 outsideEntryTriggers=3 addedColliders=0");
    }

    private static Transform CreateSafePoint(Transform parent, string name, Vector3 worldPosition)
    {
        GameObject existing = FindOptional(name);
        if (existing == null) existing = new GameObject(name);
        existing.transform.SetParent(parent, true);
        existing.transform.position = worldPosition;
        return existing.transform;
    }

    private static TMP_Text CreatePauseText(string name, string value, Transform parent, TMP_Text template,
        Vector2 position, Vector2 size, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        SetRect(rect, position, size);
        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = template.font;
        text.fontSize = 20f;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color32(37, 67, 49, 255);
        text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }

    private static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
    }

    private static void Move(string name, float y)
    {
        RectTransform rect = FindUnique(name).transform as RectTransform;
        if (rect == null) throw new InvalidOperationException("No existe RectTransform en " + name);
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);
    }

    private static GameObject FindOptional(string name)
    {
        return Resources.FindObjectsOfTypeAll<GameObject>()
            .FirstOrDefault(go => go.scene.IsValid() && go.scene.isLoaded && go.name == name);
    }

    private static void ValidateMappings(IReadOnlyCollection<Mapping> mappings, int expectedComponentCount)
    {
        OpenScene();
        ObjetoResueltoVisual[] all = Resources.FindObjectsOfTypeAll<ObjetoResueltoVisual>()
            .Where(visual => visual.gameObject.scene.IsValid() && visual.gameObject.scene.isLoaded)
            .ToArray();
        if (all.Length != expectedComponentCount)
            throw new InvalidOperationException($"Se esperaban {expectedComponentCount} ObjetoResueltoVisual y se encontraron {all.Length}.");

        Collider2D[] colliders = Resources.FindObjectsOfTypeAll<Collider2D>()
            .Where(collider => collider.gameObject.scene.IsValid() && collider.gameObject.scene.isLoaded)
            .ToArray();
        int triggers = colliders.Count(collider => collider.isTrigger);
        if (colliders.Length != 215 || triggers != 25)
            throw new InvalidOperationException($"Conteo fisico inesperado: colliders={colliders.Length}, triggers={triggers}.");

        foreach (Mapping mapping in mappings)
        {
            GameObject interactableGo = FindUnique(mapping.Interactable);
            GameObject rendererGo = FindUnique(mapping.Renderer);
            ObjetoResueltoVisual visual = interactableGo.GetComponent<ObjetoResueltoVisual>();
            SpriteRenderer renderer = rendererGo.GetComponent<SpriteRenderer>();
            Sprite fixedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(mapping.FixedSprite);

            if (visual == null || visual.interactuable != interactableGo.GetComponent<ObjetoInteractuable>() ||
                visual.spriteObjetivo != renderer || visual.spriteResuelto != fixedSprite)
                throw new InvalidOperationException("Configuracion incorrecta: " + mapping.Interactable);
            if (Math.Abs(visual.duracionDesvanecerOriginal - 0.14f) > 0.001f ||
                Math.Abs(visual.duracionAparecerResuelto - 0.16f) > 0.001f)
                throw new InvalidOperationException("Duracion incorrecta: " + mapping.Interactable);
            if (renderer.sprite == null || renderer.sprite.rect.size != fixedSprite.rect.size ||
                Math.Abs(renderer.sprite.pixelsPerUnit - fixedSprite.pixelsPerUnit) > 0.001f ||
                renderer.sprite.pivot != fixedSprite.pivot || renderer.sprite.bounds.size != fixedSprite.bounds.size)
                throw new InvalidOperationException("Dimensiones, pivote o PPU incompatibles: " + mapping.Interactable);
        }

        Debug.Log("RESOLVED_VISUAL_VALIDATE_OK count=" + mappings.Count);
    }

    private static void ApplyMappings(IReadOnlyCollection<Mapping> mappings)
    {
        OpenScene();
        foreach (Mapping mapping in mappings)
        {
            SpriteRenderer renderer = FindUnique(mapping.Renderer).GetComponent<SpriteRenderer>();
            Sprite fixedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(mapping.FixedSprite);
            if (renderer == null || renderer.sprite == null)
                throw new InvalidOperationException("Falta SpriteRenderer o sprite original en " + mapping.Renderer);
            if (fixedSprite == null)
                throw new InvalidOperationException("No se pudo cargar " + mapping.FixedSprite);
            if (renderer.sprite.rect.size != fixedSprite.rect.size ||
                Math.Abs(renderer.sprite.pixelsPerUnit - fixedSprite.pixelsPerUnit) > 0.001f ||
                renderer.sprite.pivot != fixedSprite.pivot || renderer.sprite.bounds.size != fixedSprite.bounds.size)
                throw new InvalidOperationException("Dimensiones, pivote o PPU incompatibles: " + mapping.Interactable);
        }

        foreach (Mapping mapping in mappings)
        {
            GameObject interactableGo = FindUnique(mapping.Interactable);
            GameObject rendererGo = FindUnique(mapping.Renderer);
            ObjetoInteractuable interactable = interactableGo.GetComponent<ObjetoInteractuable>();
            SpriteRenderer renderer = rendererGo.GetComponent<SpriteRenderer>();
            Sprite fixedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(mapping.FixedSprite);

            if (interactable == null) throw new InvalidOperationException("Falta ObjetoInteractuable en " + mapping.Interactable);
            if (renderer == null) throw new InvalidOperationException("Falta SpriteRenderer en " + mapping.Renderer);
            if (fixedSprite == null) throw new InvalidOperationException("No se pudo cargar " + mapping.FixedSprite);

            ObjetoResueltoVisual visual = interactableGo.GetComponent<ObjetoResueltoVisual>();
            if (visual == null) visual = Undo.AddComponent<ObjetoResueltoVisual>(interactableGo);
            Undo.RecordObject(visual, "Configurar transformacion visual resuelta");
            visual.interactuable = interactable;
            visual.spriteObjetivo = renderer;
            visual.spriteResuelto = fixedSprite;
            visual.iconoResuelto = null;
            visual.duracionDesvanecerOriginal = 0.14f;
            visual.duracionAparecerResuelto = 0.16f;
            EditorUtility.SetDirty(visual);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("RESOLVED_VISUAL_APPLY_OK count=" + mappings.Count);
    }

    private static void OpenScene()
    {
        if (SceneManager.GetActiveScene().path != ScenePath)
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    private static bool IsSpriteCompatible(Mapping mapping)
    {
        SpriteRenderer renderer = FindUnique(mapping.Renderer).GetComponent<SpriteRenderer>();
        Sprite fixedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(mapping.FixedSprite);
        return renderer != null && renderer.sprite != null && fixedSprite != null &&
            renderer.sprite.rect.size == fixedSprite.rect.size &&
            Math.Abs(renderer.sprite.pixelsPerUnit - fixedSprite.pixelsPerUnit) <= 0.001f &&
            renderer.sprite.pivot == fixedSprite.pivot && renderer.sprite.bounds.size == fixedSprite.bounds.size;
    }

    private static GameObject FindUnique(string name)
    {
        GameObject[] matches = Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(go => go.scene.IsValid() && go.scene.isLoaded && go.name == name)
            .ToArray();
        if (matches.Length != 1)
            throw new InvalidOperationException($"Se esperaba un GameObject llamado '{name}' y se encontraron {matches.Length}.");
        return matches[0];
    }
}
