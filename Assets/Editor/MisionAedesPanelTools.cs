using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class MisionAedesPanelTools
{
    [MenuItem("Mision Aedes/UI/Embellecer panel de resultado seleccionado")]
    private static void Embellecer()
    {
        GameObject panel = Selection.activeGameObject;
        if (Selection.gameObjects.Length != 1 || panel == null ||
            !panel.scene.IsValid() || EditorUtility.IsPersistent(panel) ||
            panel.GetComponent<RectTransform>() == null || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Selecciona un único panel con RectTransform de la escena fuera de Play Mode.");
            return;
        }
        Undo.IncrementCurrentGroup();
        int grupo = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Embellecer panel de resultado");
        // Crear en orden inverso: cada elemento NUEVO va al principio. No reordena los existentes.
        Crear(panel, "LineaDecorativa", new Color32(138, 90, 54, 255),
            new Vector2(0.08f, 0.79f), new Vector2(0.92f, 0.80f), 0f);
        Crear(panel, "FondoCrema", new Color32(255, 240, 201, 255), Vector2.zero, Vector2.one, 8f);
        Crear(panel, "MarcoMarron", new Color32(107, 63, 36, 255), Vector2.zero, Vector2.one, 0f);
        if (panel.GetComponent<CanvasGroup>() == null) Undo.AddComponent<CanvasGroup>(panel);
        if (panel.GetComponent<PanelUIAnimator>() == null) Undo.AddComponent<PanelUIAnimator>(panel);
        EditorSceneManager.MarkSceneDirty(panel.scene);
        Undo.CollapseUndoOperations(grupo);
        Debug.Log("Panel preparado. Los elementos existentes no se han reemplazado.", panel);
    }

    private static void Crear(GameObject padre, string nombre, Color color,
        Vector2 minimo, Vector2 maximo, float margen)
    {
        if (padre.transform.Find(nombre) != null) return;
        var hijo = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        Undo.RegisterCreatedObjectUndo(hijo, "Crear decoración UI");
        Undo.SetTransformParent(hijo.transform, padre.transform, "Asignar panel");
        Undo.RecordObject(hijo.transform, "Configurar decoración UI");
        var rect = (RectTransform)hijo.transform;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.anchorMin = minimo; rect.anchorMax = maximo;
        rect.offsetMin = Vector2.one * margen; rect.offsetMax = Vector2.one * -margen;
        rect.localPosition = new Vector3(rect.localPosition.x, rect.localPosition.y, 0f);
        rect.SetAsFirstSibling();
        var imagen = hijo.GetComponent<Image>();
        Undo.RecordObject(imagen, "Color decoración UI");
        imagen.color = color;
        imagen.raycastTarget = false;
        hijo.layer = padre.layer;
        // No permitir que LayoutGroup del panel redistribuya sus textos existentes.
        var layout = Undo.AddComponent<LayoutElement>(hijo);
        layout.ignoreLayout = true;
    }
}
