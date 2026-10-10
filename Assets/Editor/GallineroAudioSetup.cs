using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GallineroAudioSetup
{
    private const string ScenePath = "Assets/Scenes/Zona1_Escuela.unity";
    private const string ClipPath = "Assets/Audio/AMBIENTE/CancionGallina.mp3";

    [MenuItem("Mision Aedes/Audio/Configurar ambiente del gallinero")]
    public static void Apply()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject gallinero = FindUnique("Gallinero");
        GameObject player = FindUnique("Player");
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ClipPath);
        if (clip == null) throw new InvalidOperationException("No se pudo cargar " + ClipPath);

        AudioSource[] sources = gallinero.GetComponents<AudioSource>();
        if (sources.Length > 1) throw new InvalidOperationException("Gallinero tiene mas de un AudioSource.");
        AudioSource source = sources.Length == 1 ? sources[0] : Undo.AddComponent<AudioSource>(gallinero);
        Undo.RecordObject(source, "Configurar audio ambiental del gallinero");
        source.clip = clip;
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.dopplerLevel = 0f;
        source.volume = 0f;
        EditorUtility.SetDirty(source);

        GallineroAmbiente ambience = gallinero.GetComponent<GallineroAmbiente>();
        if (ambience == null) ambience = Undo.AddComponent<GallineroAmbiente>(gallinero);
        Undo.RecordObject(ambience, "Configurar proximidad del gallinero");
        ambience.jugador = player.transform;
        ambience.fuente = source;
        ambience.volumenMaximo = 0.14f;
        ambience.distanciaVolumenMaximo = 3.5f;
        ambience.distanciaSilencio = 13.5f;
        ambience.velocidadFundido = 0.18f;
        EditorUtility.SetDirty(ambience);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("CHICKEN_AMBIENCE_APPLY_OK sources=1 maxVolume=0.14 near=3.5 silence=13.5");
    }

    public static void Validate()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject gallinero = FindUnique("Gallinero");
        GameObject player = FindUnique("Player");
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ClipPath);
        AudioSource[] sources = gallinero.GetComponents<AudioSource>();
        GallineroAmbiente ambience = gallinero.GetComponent<GallineroAmbiente>();
        if (clip == null || sources.Length != 1 || ambience == null || ambience.fuente != sources[0] ||
            ambience.jugador != player.transform || sources[0].clip != clip || !sources[0].loop ||
            sources[0].playOnAwake || sources[0].spatialBlend != 0f ||
            Math.Abs(ambience.volumenMaximo - 0.14f) > 0.001f)
            throw new InvalidOperationException("Configuracion del gallinero incompleta o incorrecta.");

        Collider2D[] colliders = Resources.FindObjectsOfTypeAll<Collider2D>()
            .Where(item => item.gameObject.scene.IsValid() && item.gameObject.scene.isLoaded).ToArray();
        int triggers = colliders.Count(item => item.isTrigger);
        if (colliders.Length != 219 || triggers != 25)
            throw new InvalidOperationException($"Conteo fisico inesperado: colliders={colliders.Length}, triggers={triggers}.");
        Debug.Log("CHICKEN_AMBIENCE_VALIDATE_OK sources=1 clip=1 player=1 colliders=219 triggers=25 audioAddedColliders=0");
    }

    private static GameObject FindUnique(string name)
    {
        GameObject[] matches = Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(go => go.scene.IsValid() && go.scene.isLoaded && go.name == name).ToArray();
        if (matches.Length != 1)
            throw new InvalidOperationException($"Se esperaba un GameObject {name} y se encontraron {matches.Length}.");
        return matches[0];
    }
}
