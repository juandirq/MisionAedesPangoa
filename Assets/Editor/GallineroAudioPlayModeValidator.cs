using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class GallineroAudioPlayModeValidator
{
    private const string Flag = "MisionAedes.ChickenAmbienceValidation";
    private const string ScenePath = "Assets/Scenes/Zona1_Escuela.unity";
    private static int frames;
    private static int stage;
    private static Vector3 originalPosition;
    private static float originalSfx;
    private static float nearVolume;

    static GallineroAudioPlayModeValidator()
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
        EditorApplication.playModeStateChanged -= OnState;
        EditorApplication.playModeStateChanged += OnState;
    }

    private static void OnState(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(Flag, false) || state != PlayModeStateChange.EnteredPlayMode) return;
        GallineroAmbiente ambience = UnityEngine.Object.FindAnyObjectByType<GallineroAmbiente>();
        if (ambience == null) { Fail("No se encontro GallineroAmbiente."); return; }
        originalPosition = ambience.jugador.position;
        originalSfx = AjustesAudio.Sfx;
        AjustesAudio.GuardarSfx(1f);
        Time.timeScale = 1f;
        ambience.jugador.position = ambience.transform.position;
        stage = 0;
        frames = 0;
        EditorApplication.update += Tick;
    }

    private static void Tick()
    {
        if (++frames < 20) return;
        frames = 0;
        try
        {
            GallineroAmbiente ambience = UnityEngine.Object.FindAnyObjectByType<GallineroAmbiente>();
            if (ambience == null) throw new InvalidOperationException("Se perdio GallineroAmbiente.");
            if (stage == 0)
            {
                if (ambience.VolumenObjetivo < 0.139f || ambience.fuente.volume <= 0f)
                    throw new InvalidOperationException("El volumen no aumento cerca del gallinero.");
                nearVolume = ambience.fuente.volume;
                ambience.jugador.position = ambience.transform.position + Vector3.right * 20f;
                stage = 1;
            }
            else if (stage == 1)
            {
                if (ambience.VolumenObjetivo > 0.0001f || ambience.fuente.volume >= nearVolume)
                    throw new InvalidOperationException("El volumen no disminuyo al alejarse.");
                AjustesAudio.GuardarSfx(0f);
                stage = 2;
            }
            else if (stage == 2)
            {
                if (ambience.VolumenObjetivo != 0f || ambience.fuente.volume != 0f)
                    throw new InvalidOperationException("EFECTOS al 0% no silencio el gallinero.");
                AjustesAudio.GuardarSfx(1f);
                ambience.jugador.position = ambience.transform.position;
                Time.timeScale = 0f;
                stage = 3;
            }
            else if (stage == 3)
            {
                if (!ambience.PausadoPorJuego)
                    throw new InvalidOperationException("El gallinero no se pauso con Time.timeScale=0.");
                Time.timeScale = 1f;
                stage = 4;
            }
            else
            {
                if (ambience.PausadoPorJuego)
                    throw new InvalidOperationException("El gallinero no se reanudo.");
                Debug.Log("CHICKEN_AMBIENCE_PLAYMODE_OK near=1 far=1 sfxMute=1 paused=1 resumed=1 duplicate=0");
                Finish(0);
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            Finish(1);
        }
    }

    private static void Fail(string message)
    {
        Debug.LogError(message);
        Finish(1);
    }

    private static void Finish(int exitCode)
    {
        EditorApplication.update -= Tick;
        GallineroAmbiente ambience = UnityEngine.Object.FindAnyObjectByType<GallineroAmbiente>();
        if (ambience != null && ambience.jugador != null) ambience.jugador.position = originalPosition;
        AjustesAudio.GuardarSfx(originalSfx);
        Time.timeScale = 1f;
        SessionState.SetBool(Flag, false);
        EditorApplication.playModeStateChanged -= OnState;
        EditorApplication.isPlaying = false;
        EditorApplication.delayCall += () => EditorApplication.Exit(exitCode);
    }
}
