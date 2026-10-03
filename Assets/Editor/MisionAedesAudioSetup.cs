using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

internal static class MisionAedesAudioSetup
{
    private const string RutaEscena = "Assets/Scenes/Zona1_Escuela.unity";

    [MenuItem("Mision Aedes/Configurar audio completo")]
    private static void Configurar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Configurar audio", "Sal del modo Play primero.", "Aceptar");
            return;
        }
        if (!AbrirEscena()) return;

        string[] nombres = { "Musica Inicio", "MusicaDerrota", "MusicaVictoria", "Zona1",
            "Zona2", "Zona3", "Botones", "EfectoDeCorrecto", "EfectoDeIncorrecto",
            "EfectoEscribirNPC", "PISADAS", "RIO" };
        var clips = new Dictionary<string, AudioClip>();
        foreach (string nombre in nombres)
        {
            AudioClip clip = BuscarClipExacto(nombre);
            if (clip == null)
            {
                EditorUtility.DisplayDialog("Configuración cancelada",
                    "No se encontró exactamente el AudioClip: " + nombre, "Aceptar");
                return;
            }
            clips[nombre] = clip;
        }

        AudioManager[] existentes = UnityEngine.Object.FindObjectsByType<AudioManager>(
            FindObjectsInactive.Include);
        if (existentes.Length > 1)
        {
            EditorUtility.DisplayDialog("Configuración cancelada",
                "Hay más de un AudioManager. Elimina el duplicado antes de continuar.", "Aceptar");
            return;
        }

        Undo.IncrementCurrentGroup();
        int grupo = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Configurar audio completo");
        AudioManager audio = existentes.FirstOrDefault();
        if (audio == null)
        {
            GameObject go = new GameObject("AudioManager");
            Undo.RegisterCreatedObjectUndo(go, "Crear AudioManager");
            audio = Undo.AddComponent<AudioManager>(go);
        }
        Undo.RecordObject(audio, "Asignar audio completo");

        audio.musicaInicio = clips["Musica Inicio"];
        audio.musicaDerrota = clips["MusicaDerrota"];
        audio.musicaVictoria = clips["MusicaVictoria"];
        audio.musicaZona1 = clips["Zona1"];
        audio.musicaZona2 = clips["Zona2"];
        audio.musicaZona3 = clips["Zona3"];
        audio.efectoBoton = clips["Botones"];
        audio.efectoCorrecto = clips["EfectoDeCorrecto"];
        audio.efectoIncorrecto = clips["EfectoDeIncorrecto"];
        audio.efectoEscribirNPC = clips["EfectoEscribirNPC"];
        audio.pisadas = clips["PISADAS"];
        audio.ambienteRio = clips["RIO"];
        audio.duracionCrossfade = 0.35f;
        audio.offsetMusicaInicio = 0.46f;
        audio.offsetMusicaDerrota = 0.66f;
        audio.offsetMusicaVictoria = 0f;
        audio.offsetMusicaZona1 = 0.27f;
        audio.offsetMusicaZona2 = 0.14f;
        audio.offsetMusicaZona3 = 0.08f;
        audio.offsetBoton = 2.37f;
        audio.offsetCorrecto = 1.06f;
        audio.offsetIncorrecto = 1.84f;
        audio.offsetBlipNPC = 0.74f;
        audio.offsetPisadas = 0.31f;

        audio.musicaA = Fuente(audio.transform, "Audio_MusicaA");
        audio.musicaB = Fuente(audio.transform, "Audio_MusicaB");
        audio.sfxGeneral = Fuente(audio.transform, "Audio_SFX");
        audio.dialogo = Fuente(audio.transform, "Audio_Dialogo");
        audio.fuentePisadas = Fuente(audio.transform, "Audio_Pisadas");
        audio.fuenteRio = Fuente(audio.transform, "Audio_Rio");

        audio.playerMovement = UnityEngine.Object.FindAnyObjectByType<PlayerMovement>(FindObjectsInactive.Include);
        audio.gameManager = UnityEngine.Object.FindAnyObjectByType<GameManager>(FindObjectsInactive.Include);
        audio.referenciasRio = BuscarTransformExactos("RIO_1", "RIO_3", "RIO_5");
        if (audio.playerMovement == null || audio.gameManager == null || audio.referenciasRio.Length != 3)
        {
            Undo.RevertAllDownToGroup(grupo);
            EditorUtility.DisplayDialog("Configuración cancelada",
                "Falta Player, GameManager o alguno de RIO_1/RIO_3/RIO_5.", "Aceptar");
            return;
        }

        int botonesConfigurados = 0;
        foreach (Button boton in UnityEngine.Object.FindObjectsByType<Button>(
                     FindObjectsInactive.Include))
        {
            bool respuesta = EsBotonRespuesta(boton);
            AudioBotonSFX existente = boton.GetComponent<AudioBotonSFX>();
            if (respuesta)
            {
                if (existente != null) Undo.DestroyObjectImmediate(existente);
                continue;
            }
            if (existente == null) Undo.AddComponent<AudioBotonSFX>(boton.gameObject);
            botonesConfigurados++;
        }

        EditorUtility.SetDirty(audio);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Undo.CollapseUndoOperations(grupo);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        int listeners = UnityEngine.Object.FindObjectsByType<AudioListener>(
            FindObjectsInactive.Include).Length;
        Debug.Log($"Audio completo configurado: {clips.Count} clips, 6 AudioSources, " +
                  $"{botonesConfigurados} botones normales, 3 referencias de río, " +
                  $"AudioListeners existentes: {listeners}.");
        EditorUtility.DisplayDialog("Audio configurado",
            $"{clips.Count} clips encontrados.\n6 AudioSources configurados.\n" +
            $"{botonesConfigurados} botones normales.\n3 referencias de río.\n" +
            "Los botones de respuesta quedaron excluidos.", "Aceptar");
    }

    private static AudioClip BuscarClipExacto(string nombre)
    {
        foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Audio" }))
        {
            string ruta = AssetDatabase.GUIDToAssetPath(guid);
            if (string.Equals(Path.GetFileNameWithoutExtension(ruta), nombre,
                    StringComparison.OrdinalIgnoreCase))
                return AssetDatabase.LoadAssetAtPath<AudioClip>(ruta);
        }
        return null;
    }

    private static AudioSource Fuente(Transform padre, string nombre)
    {
        Transform hijo = padre.Find(nombre);
        GameObject go;
        if (hijo == null)
        {
            go = new GameObject(nombre);
            Undo.RegisterCreatedObjectUndo(go, "Crear fuente de audio");
            Undo.SetTransformParent(go.transform, padre, "Vincular fuente de audio");
            go.transform.localPosition = Vector3.zero;
        }
        else go = hijo.gameObject;
        AudioSource[] fuentes = go.GetComponents<AudioSource>();
        AudioSource fuente = fuentes.FirstOrDefault();
        if (fuente == null)
            fuente = Undo.AddComponent<AudioSource>(go);
        else
            for (int i = 1; i < fuentes.Length; i++)
                Undo.DestroyObjectImmediate(fuentes[i]);
        Undo.RecordObject(fuente, "Configurar fuente de audio");
        fuente.playOnAwake = false;
        fuente.spatialBlend = 0f;
        fuente.loop = false;
        return fuente;
    }

    private static bool EsBotonRespuesta(Button boton)
    {
        for (int i = 0; i < boton.onClick.GetPersistentEventCount(); i++)
        {
            string metodo = boton.onClick.GetPersistentMethodName(i);
            if (metodo == "Opcion1" || metodo == "Opcion2" || metodo == "Opcion3") return true;
        }
        return false;
    }

    private static Transform[] BuscarTransformExactos(params string[] nombres)
    {
        Transform[] todos = UnityEngine.Object.FindObjectsByType<Transform>(
            FindObjectsInactive.Include);
        var resultado = new List<Transform>();
        foreach (string nombre in nombres)
        {
            Transform[] encontrados = todos.Where(t => t.name == nombre).ToArray();
            if (encontrados.Length == 1) resultado.Add(encontrados[0]);
        }
        return resultado.ToArray();
    }

    private static bool AbrirEscena()
    {
        if (SceneManager.GetActiveScene().path == RutaEscena) return true;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(RutaEscena) == null) return false;
        EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);
        return true;
    }
}
