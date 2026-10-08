using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class RiverWaterPlayModeValidator
{
    private const string Flag = "MisionAedes.RiverWaterPlayModeValidation";
    private const string ScenePath = "Assets/Scenes/Zona1_Escuela.unity";
    private static int frames;
    private static float runningTime;
    private static float pausedTime;

    static RiverWaterPlayModeValidator()
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
        if (!SessionState.GetBool(Flag, false) || state != PlayModeStateChange.EnteredPlayMode) return;
        Time.timeScale = 1f;
        frames = 0;
        EditorApplication.update -= WaitRunning;
        EditorApplication.update += WaitRunning;
    }

    private static void WaitRunning()
    {
        if (++frames < 12) return;
        EditorApplication.update -= WaitRunning;
        try
        {
            RiverWaterAnimator[] rivers = FindRivers();
            if (rivers.Length != 5) throw new InvalidOperationException("Conteo de rios incorrecto en Play Mode.");
            if (rivers.Any(river => river.TiempoAnimacion <= 0f))
                throw new InvalidOperationException("La animacion no avanzo durante el juego.");

            runningTime = rivers[0].TiempoAnimacion;
            Time.timeScale = 0f;
            frames = 0;
            EditorApplication.update += WaitPaused;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            Finish(1);
        }
    }

    private static void WaitPaused()
    {
        if (++frames < 12) return;
        EditorApplication.update -= WaitPaused;
        try
        {
            RiverWaterAnimator[] rivers = FindRivers();
            if (Math.Abs(rivers[0].TiempoAnimacion - runningTime) > 0.0001f)
                throw new InvalidOperationException("La animacion de agua avanzo con el juego pausado.");

            pausedTime = rivers[0].TiempoAnimacion;
            Time.timeScale = 1f;
            frames = 0;
            EditorApplication.update += WaitResumed;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            Finish(1);
        }
    }

    private static void WaitResumed()
    {
        if (++frames < 12) return;
        EditorApplication.update -= WaitResumed;
        try
        {
            RiverWaterAnimator[] rivers = FindRivers();
            if (rivers[0].TiempoAnimacion <= pausedTime)
                throw new InvalidOperationException("La animacion de agua no continuo al reanudar.");

            Debug.Log("RIVER_WATER_PLAYMODE_OK rivers=5 running=1 paused=1 resumed=1 scaledTime=1");
            Finish(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            Finish(1);
        }
    }

    private static RiverWaterAnimator[] FindRivers()
    {
        return UnityEngine.Object.FindObjectsByType<RiverWaterAnimator>(FindObjectsInactive.Exclude)
            .Where(river => river.name.StartsWith("RIO_", StringComparison.Ordinal))
            .ToArray();
    }

    private static void Finish(int exitCode)
    {
        Time.timeScale = 1f;
        SessionState.SetBool(Flag, false);
        EditorApplication.update -= WaitRunning;
        EditorApplication.update -= WaitPaused;
        EditorApplication.update -= WaitResumed;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.isPlaying = false;
        EditorApplication.delayCall += () => EditorApplication.Exit(exitCode);
    }
}
