using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class MisionAedesPreparacionWindow : EditorWindow
{
    private bool soloLayer;
    private bool sorter = true;
    private bool colliderBase = true;
    private bool asignarLayer = true;
    private bool incluirHijos;
    private int layerIndice;

    [MenuItem("Mision Aedes/Asignar Sorting Layer a seleccion")]
    private static void AbrirLayers()
    {
        var ventana = GetWindow<MisionAedesPreparacionWindow>("Sorting Layer");
        ventana.soloLayer = true;
    }

    [MenuItem("Mision Aedes/Preparacion Top-Down Avanzada")]
    private static void AbrirAvanzada()
    {
        var ventana = GetWindow<MisionAedesPreparacionWindow>("Top-Down");
        ventana.soloLayer = false;
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Solo selección de escena. No procesa UI ni assets del Project. " +
            "Los componentes existentes se conservan. Ctrl+Z deshace la operación.", MessageType.Info);
        if (!soloLayer)
        {
            sorter = EditorGUILayout.Toggle("Añadir YSpriteSorter", sorter);
            colliderBase = EditorGUILayout.Toggle("Añadir collider de base", colliderBase);
            asignarLayer = EditorGUILayout.Toggle("Asignar Sorting Layer", asignarLayer);
        }
        incluirHijos = EditorGUILayout.Toggle("Incluir hijos", incluirHijos);
        SortingLayer[] layers = SortingLayer.layers;
        if (layers.Length == 0) return;
        layerIndice = Mathf.Clamp(layerIndice, 0, layers.Length - 1);
        layerIndice = EditorGUILayout.Popup("Sorting Layer", layerIndice,
            layers.Select(layer => layer.name).ToArray());
        using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode ||
                   Selection.gameObjects.Length == 0))
        {
            if (GUILayout.Button("Aplicar a selección")) Aplicar(layers[layerIndice].id);
        }
    }

    private void Aplicar(int layerId)
    {
        var renderers = new HashSet<SpriteRenderer>();
        foreach (GameObject objeto in Selection.gameObjects)
        {
            if (!objeto.scene.IsValid() || EditorUtility.IsPersistent(objeto)) continue;
            if (incluirHijos)
                foreach (var renderer in objeto.GetComponentsInChildren<SpriteRenderer>(true))
                    renderers.Add(renderer);
            else if (objeto.TryGetComponent(out SpriteRenderer renderer)) renderers.Add(renderer);
        }
        Undo.IncrementCurrentGroup();
        int grupo = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Preparación top-down seleccionada");
        int sorters = 0, colliders = 0, layers = 0, omitidos = 0;
        foreach (var renderer in renderers)
        {
            if (renderer.GetComponentInParent<Canvas>(true) != null ||
                renderer.GetComponent<RectTransform>() != null)
            { omitidos++; continue; }
            bool cambio = false;
            if (!soloLayer && sorter && renderer.GetComponent<YSpriteSorter>() == null)
            {
                Undo.RecordObject(renderer, "Orden por Y");
                var nuevo = Undo.AddComponent<YSpriteSorter>(renderer.gameObject);
                Undo.RecordObject(nuevo, "Configurar sorter");
                nuevo.actualizarMientrasSeMueve = !renderer.gameObject.isStatic;
                PrefabUtility.RecordPrefabInstancePropertyModifications(nuevo);
                sorters++; cambio = true;
            }
            if (!soloLayer && colliderBase && renderer.sprite != null &&
                renderer.GetComponent<Collider2D>() == null)
            {
                var nuevo = Undo.AddComponent<BoxCollider2D>(renderer.gameObject);
                Undo.RecordObject(nuevo, "Configurar collider base");
                Bounds b = renderer.localBounds;
                nuevo.size = new Vector2(b.size.x * 0.55f, b.size.y * 0.2f);
                nuevo.offset = new Vector2(b.center.x, b.min.y + nuevo.size.y * 0.5f);
                nuevo.isTrigger = false;
                PrefabUtility.RecordPrefabInstancePropertyModifications(nuevo);
                colliders++; cambio = true;
            }
            if ((soloLayer || asignarLayer) && renderer.sortingLayerID != layerId)
            {
                Undo.RecordObject(renderer, "Asignar Sorting Layer");
                renderer.sortingLayerID = layerId;
                layers++; cambio = true;
            }
            if (cambio)
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
                EditorSceneManager.MarkSceneDirty(renderer.gameObject.scene);
            }
            else omitidos++;
        }
        Undo.CollapseUndoOperations(grupo);
        Debug.Log($"Top-down: {sorters} sorters, {colliders} colliders, {layers} Sorting Layers modificadas; {omitidos} omitidos/sin cambios.");
    }
}
