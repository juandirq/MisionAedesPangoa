using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class RoutePlayModeValidator
{
    private const string Flag = "MisionAedes.RoutePlayModeValidation";
    private const string ScenePath = "Assets/Scenes/Zona1_Escuela.unity";
    private static int frames;

    static RoutePlayModeValidator()
    {
        if (SessionState.GetBool(Flag, false)) Register();
    }

    public static void Run()
    {
        SessionState.SetBool(Flag, true);
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Register();
        EditorApplication.isPlaying = true;
    }

    private static void Register()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(Flag, false)) return;
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            frames = 0;
            EditorApplication.update -= WaitForScene;
            EditorApplication.update += WaitForScene;
        }
    }

    private static void WaitForScene()
    {
        if (++frames < 8) return;
        EditorApplication.update -= WaitForScene;
        try
        {
            ExecuteTests();
            Debug.Log("ROUTE_PLAYMODE_OK zone1Locked=1 zone2Entered=1 zone2ReturnBlocked=1 " +
                "zone3Entered=1 zone3ReturnBlocked=1 diagonalCasts=1 checkpointTime=1 checkpointErrors=1");
            Finish(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            Finish(1);
        }
    }

    private static void ExecuteTests()
    {
        GameManager manager = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        RutaLinealController route = UnityEngine.Object.FindAnyObjectByType<RutaLinealController>();
        PlayerMovement player = UnityEngine.Object.FindAnyObjectByType<PlayerMovement>();
        if (manager == null || route == null || player == null) throw new InvalidOperationException("Faltan sistemas de ruta.");
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        Rigidbody2D body = player.GetComponent<Rigidbody2D>();

        manager.PrepararNuevaPartida();
        manager.IniciarZona(3, 210f, "Zona 1", 1);
        AssertNear(player.transform.position, route.puntoSeguroZona1.position, "entrada Zona 1");
        Assert(route.limiteEntradaZona1.enabled && !route.limiteEntradaZona1.isTrigger, "Zona 1 no cerró su salida.");
        AssertCast(playerCollider, route.limiteEntradaZona1, Vector2.down, 3f, "salida Zona 1");
        AssertDiagonalCasts(playerCollider, route.limiteEntradaZona1, Vector2.down, 3f, "diagonal Zona 1");

        Win(manager, 3, 30);
        Assert(!route.limiteEntradaZona1.enabled && !route.bloqueoPasoZona2.activeSelf, "Zona 1 no abrió el avance.");
        manager.ContinuarDespuesDeVictoria();

        manager.IniciarZona(6, 345f, "Zona 2", 2);
        AssertNear(player.transform.position, route.puntoSeguroZona2.position, "entrada Zona 2");
        Assert(route.bloqueoPasoZona2.activeSelf, "Zona 2 no cerró el retorno.");
        AssertCast(playerCollider, route.bloqueoPasoZona2.GetComponent<Collider2D>(), Vector2.right, 3f, "retorno Zona 2");
        AssertDiagonalCasts(playerCollider, route.bloqueoPasoZona2.GetComponent<Collider2D>(), Vector2.right, 3f, "diagonal Zona 2");

        body.position = (Vector2)route.puntoSeguroZona2.position + Vector2.up * 2f;
        player.transform.position = body.position;
        manager.tiempoRestante = 0f;
        typeof(GameManager).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(manager, null);
        Assert(manager.JuegoTerminado, "No se produjo derrota por tiempo.");
        manager.ReiniciarZona();
        AssertNear(player.transform.position, route.puntoSeguroZona2.position, "checkpoint por tiempo Zona 2");

        Win(manager, 6, 15);
        Assert(!route.bloqueoPasoZona2.activeSelf && !route.bloqueoPasoZona3.activeSelf,
            "Zona 2 no abrió completamente el corredor central.");
        manager.ContinuarDespuesDeVictoria();

        MovePhysicallyToZone3(body, player, route, route.puntoSeguroZona2.position);
        TriggerZonaConfigurable trigger3 = GameObject.Find("TriggerEntradaZona3").GetComponent<TriggerZonaConfigurable>();
        Assert(trigger3 != null, "No se encontró TriggerEntradaZona3.");
        trigger3.ComenzarDesdePanel();
        Assert(manager.ZonaActiva && manager.indiceZonaActual == 3, "El panel Continuar no inició Zona 3.");
        AssertNear(player.transform.position, route.puntoSeguroZona3.position, "entrada Zona 3");
        Assert(route.bloqueoPasoZona3.activeSelf, "Zona 3 no cerró el retorno.");
        AssertCast(playerCollider, route.bloqueoPasoZona3.GetComponent<Collider2D>(), Vector2.left, 12f, "retorno Zona 3");
        AssertDiagonalCasts(playerCollider, route.bloqueoPasoZona3.GetComponent<Collider2D>(), Vector2.left, 12f, "diagonal Zona 3");

        body.position = (Vector2)route.puntoSeguroZona3.position + Vector2.up * 2f;
        player.transform.position = body.position;
        manager.RegistrarError(); manager.RegistrarError(); manager.RegistrarError();
        Assert(manager.JuegoTerminado, "No se produjo derrota por errores.");
        manager.ReiniciarZona();
        AssertNear(player.transform.position, route.puntoSeguroZona3.position, "checkpoint por errores Zona 3");
    }

    private static void MovePhysicallyToZone3(Rigidbody2D body, PlayerMovement player, RutaLinealController route, Vector3 start)
    {
        body.position = start;
        body.transform.position = start;
        Physics2D.SyncTransforms();
        SimulationMode2D previous = Physics2D.simulationMode;
        Physics2D.simulationMode = SimulationMode2D.Script;
        body.linearVelocity = Vector2.right * 5f;
        for (int i = 0; i < 220 && player.enabled; i++) Physics2D.Simulate(0.02f);
        body.linearVelocity = Vector2.zero;
        Physics2D.simulationMode = previous;
        Assert(!player.enabled && body.position.x >= 8.7f,
            "El Rigidbody2D no alcanzó físicamente el trigger de Zona 3; posición=" + body.position);
        Assert(!route.bloqueoPasoZona2.activeSelf && !route.bloqueoPasoZona3.activeSelf,
            "Una barrera del corredor volvió a activarse durante el desplazamiento.");
    }

    private static void Win(GameManager manager, int objectives, int reduction)
    {
        for (int i = 0; i < objectives; i++) manager.RegistrarAcierto(reduction);
        Assert(manager.JuegoTerminado, "La zona no terminó tras completar sus objetivos.");
    }

    private static void AssertDiagonalCasts(Collider2D player, Collider2D target, Vector2 direction, float distance, string label)
    {
        Vector2 perpendicular = new Vector2(-direction.y, direction.x) * 0.18f;
        AssertCast(player, target, (direction + perpendicular).normalized, distance, label + " A");
        AssertCast(player, target, (direction - perpendicular).normalized, distance, label + " B");
    }

    private static void AssertCast(Collider2D player, Collider2D target, Vector2 direction, float distance, string label)
    {
        RaycastHit2D[] hits = new RaycastHit2D[32];
        int count = player.Cast(direction, ContactFilter2D.noFilter, hits, distance);
        Assert(hits.Take(count).Any(hit => hit.collider == target), "El cast físico no encontró el bloqueo: " + label);
    }

    private static void AssertNear(Vector3 actual, Vector3 expected, string label)
    {
        Assert(Vector2.Distance(actual, expected) < 0.05f, label + $" incorrecta: {actual} != {expected}");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void Finish(int exitCode)
    {
        SessionState.SetBool(Flag, false);
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.isPlaying = false;
        EditorApplication.delayCall += () => EditorApplication.Exit(exitCode);
    }
}
