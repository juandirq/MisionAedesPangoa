using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MisionAedesTools
{
    private const float PorcentajeAncho = 0.55f;
    private const float PorcentajeAlto = 0.20f;

    [MenuItem("Mision Aedes/Crear collider de base en seleccion")]
    private static void CrearColliderDeBase()
    {
        int creados = 0;
        int omitidos = 0;

        foreach (GameObject objeto in Selection.gameObjects)
        {
            SpriteRenderer renderer = objeto.GetComponent<SpriteRenderer>();
            if (renderer == null || renderer.sprite == null ||
                objeto.GetComponent<Collider2D>() != null)
            {
                omitidos++;
                continue;
            }

            CrearColliderBase(objeto, renderer);
            creados++;
        }

        Debug.Log("Colliders de base creados: " + creados +
                  ". Objetos omitidos: " + omitidos +
                  " (sin sprite o con collider existente).");
    }

    [MenuItem("Mision Aedes/Crear collider de base en seleccion", true)]
    private static bool HaySeleccion()
    {
        return Selection.gameObjects.Length > 0;
    }

    [MenuItem("Mision Aedes/Preparar objetos top-down seleccionados")]
    private static void PrepararObjetosTopDown()
    {
        int sortersCreados = 0;
        int collidersCreados = 0;
        int omitidos = 0;

        foreach (GameObject objeto in Selection.gameObjects)
        {
            SpriteRenderer renderer = objeto.GetComponent<SpriteRenderer>();
            if (renderer == null || renderer.sprite == null)
            {
                omitidos++;
                continue;
            }

            bool huboCambios = false;
            YSpriteSorter sorter = objeto.GetComponent<YSpriteSorter>();

            if (sorter == null)
            {
                sorter = Undo.AddComponent<YSpriteSorter>(objeto);
                sortersCreados++;
                huboCambios = true;
            }

            if (objeto.isStatic && sorter.actualizarMientrasSeMueve)
            {
                Undo.RecordObject(sorter, "Configurar sorter estatico");
                sorter.actualizarMientrasSeMueve = false;
                EditorUtility.SetDirty(sorter);
                huboCambios = true;
            }

            if (objeto.GetComponent<Collider2D>() == null)
            {
                CrearColliderBase(objeto, renderer);
                collidersCreados++;
                huboCambios = true;
            }

            if (!huboCambios)
                omitidos++;
        }

        Debug.Log("Preparacion top-down completada. Sorters añadidos: " +
                  sortersCreados + ", colliders añadidos: " + collidersCreados +
                  ", objetos omitidos o sin cambios: " + omitidos + ".");
    }

    [MenuItem("Mision Aedes/Preparar objetos top-down seleccionados", true)]
    private static bool PuedePrepararTopDown()
    {
        return Selection.gameObjects.Length > 0;
    }

    [MenuItem("Mision Aedes/Configurar Player para profundidad")]
    private static void ConfigurarPlayerParaProfundidad()
    {
        if (!ObtenerSeleccionUnica(out GameObject player))
            return;

        if (player.GetComponent<SpriteRenderer>() == null)
        {
            Debug.LogWarning("El objeto seleccionado no tiene SpriteRenderer.", player);
            return;
        }

        YSpriteSorter sorter = player.GetComponent<YSpriteSorter>();
        if (sorter == null)
            sorter = Undo.AddComponent<YSpriteSorter>(player);

        if (!sorter.actualizarMientrasSeMueve)
        {
            Undo.RecordObject(sorter, "Configurar sorter dinamico");
            sorter.actualizarMientrasSeMueve = true;
            EditorUtility.SetDirty(sorter);
        }

        Debug.Log("Player preparado para profundidad por Y.", player);
    }

    [MenuItem("Mision Aedes/Configurar Player para profundidad", true)]
    private static bool PuedeConfigurarPlayer()
    {
        return Selection.gameObjects.Length == 1;
    }

    [MenuItem("Mision Aedes/Crear obstaculo invisible en seleccion")]
    private static void CrearObstaculoInvisible()
    {
        int creados = 0;
        int omitidos = 0;

        foreach (GameObject objeto in Selection.gameObjects)
        {
            SpriteRenderer renderer = objeto.GetComponent<SpriteRenderer>();
            if (renderer == null || renderer.sprite == null ||
                objeto.GetComponent<Collider2D>() != null)
            {
                omitidos++;
                continue;
            }

            CrearColliderBase(objeto, renderer);
            creados++;
        }

        Debug.Log("Obstaculos invisibles creados: " + creados +
                  ". Objetos omitidos: " + omitidos + ".");
    }

    [MenuItem("Mision Aedes/Crear obstaculo invisible en seleccion", true)]
    private static bool PuedeCrearObstaculo()
    {
        return Selection.gameObjects.Length > 0;
    }

    [MenuItem("Mision Aedes/Crear limite rectangular vacio")]
    private static void CrearLimiteRectangularVacio()
    {
        if (!ObtenerSeleccionUnica(out GameObject padre))
            return;

        GameObject limite = new GameObject("LimiteMovimiento");
        Undo.RegisterCreatedObjectUndo(limite, "Crear limite rectangular");
        Undo.SetTransformParent(limite.transform, padre.transform,
            "Asignar padre del limite");
        limite.transform.localPosition = Vector3.zero;
        limite.layer = padre.layer;

        BoxCollider2D collider = Undo.AddComponent<BoxCollider2D>(limite);
        collider.isTrigger = false;
        Selection.activeGameObject = limite;

        Debug.Log("LimiteMovimiento creado. Ajusta el BoxCollider2D y la Layer " +
                  "desde el Inspector.", limite);
    }

    [MenuItem("Mision Aedes/Crear limite rectangular vacio", true)]
    private static bool PuedeCrearLimite()
    {
        return Selection.gameObjects.Length == 1;
    }

    [MenuItem("Mision Aedes/Crear interactuable desde seleccion")]
    private static void CrearInteractuableDesdeSeleccion()
    {
        if (!ObtenerSeleccionUnica(out GameObject seleccionado))
            return;

        Collider2D collider = seleccionado.GetComponent<Collider2D>();
        if (collider == null)
        {
            BoxCollider2D nuevoCollider = Undo.AddComponent<BoxCollider2D>(seleccionado);
            SpriteRenderer renderer = seleccionado.GetComponent<SpriteRenderer>();

            if (renderer != null && renderer.sprite != null)
            {
                Bounds limites = renderer.sprite.bounds;
                nuevoCollider.size = limites.size;
                nuevoCollider.offset = limites.center;
            }

            collider = nuevoCollider;
        }

        if (!collider.isTrigger)
        {
            Undo.RecordObject(collider, "Configurar trigger interactuable");
            collider.isTrigger = true;
            EditorUtility.SetDirty(collider);
        }

        ObjetoInteractuable interactuable =
            seleccionado.GetComponent<ObjetoInteractuable>();
        if (interactuable == null)
            interactuable = Undo.AddComponent<ObjetoInteractuable>(seleccionado);

        Undo.RecordObject(interactuable, "Asignar referencias del interactuable");

        if (interactuable.textoInteraccion == null)
            interactuable.textoInteraccion = BuscarGameObjectUnico("TextoInteraccion");

        VentanaDecisionUI[] ventanas =
            Object.FindObjectsByType<VentanaDecisionUI>(FindObjectsInactive.Include);
        GameManager[] gestores =
            Object.FindObjectsByType<GameManager>(FindObjectsInactive.Include);

        if (interactuable.ventanaUI == null && ventanas.Length == 1)
            interactuable.ventanaUI = ventanas[0];

        if (interactuable.gameManager == null && gestores.Length == 1)
            interactuable.gameManager = gestores[0];

        EditorUtility.SetDirty(interactuable);

        if (interactuable.textoInteraccion == null)
            Debug.LogWarning("No se encontro un unico TextoInteraccion en la escena.",
                interactuable);

        if (interactuable.ventanaUI == null)
            Debug.LogWarning("No se encontro una unica VentanaDecisionUI en la escena.",
                interactuable);

        if (interactuable.gameManager == null)
            Debug.LogWarning("No se encontro un unico GameManager en la escena.",
                interactuable);

        Debug.Log("Interactuable preparado. Completa titulo, descripcion, opciones, " +
                  "respuesta correcta, feedback y reduccion de riesgo.", interactuable);
    }

    [MenuItem("Mision Aedes/Crear interactuable desde seleccion", true)]
    private static bool PuedeCrearInteractuable()
    {
        return Selection.gameObjects.Length == 1;
    }

    [MenuItem("Mision Aedes/Validar objetos interactuables")]
    private static void ValidarObjetosInteractuables()
    {
        ValidarObjetosInteractuables(out _, out _);
    }

    public static void ValidarObjetosInteractuables(out int errores, out int advertencias)
    {
        ObjetoInteractuable[] objetos =
            Object.FindObjectsByType<ObjetoInteractuable>(FindObjectsInactive.Include);
        GameManager[] gestores =
            Object.FindObjectsByType<GameManager>(FindObjectsInactive.Include);
        VentanaDecisionUI[] ventanas =
            Object.FindObjectsByType<VentanaDecisionUI>(FindObjectsInactive.Include);
        GameObject textoInteraccionUnico = BuscarGameObjectUnico("TextoInteraccion");

        errores = 0;
        advertencias = 0;

        if (gestores.Length == 0)
            errores += InformarError("No existe GameManager en la escena.", null);
        else if (gestores.Length > 1)
            advertencias += InformarAdvertencia(
                "Hay mas de un GameManager en la escena.", gestores[0]);

        if (ventanas.Length > 1)
            advertencias += InformarAdvertencia(
                "Hay mas de una VentanaDecisionUI en la escena.", ventanas[0]);
        else if (ventanas.Length == 0)
            errores += InformarError(
                "No existe VentanaDecisionUI en la escena.", null);

        int cantidadTextosInteraccion = ContarGameObjects("TextoInteraccion");
        if (cantidadTextosInteraccion > 1)
            advertencias += InformarAdvertencia(
                "Hay mas de un GameObject llamado TextoInteraccion en la escena.",
                textoInteraccionUnico);

        foreach (GameManager gestor in gestores)
        {
            errores += ValidarReferenciaError(gestor.playerMovement,
                "GameManager no tiene playerMovement", gestor);
            errores += ValidarReferenciaError(gestor.panelVictoria,
                "GameManager no tiene panelVictoria", gestor);
            errores += ValidarReferenciaError(gestor.panelDerrota,
                "GameManager no tiene panelDerrota", gestor);
        }

        foreach (VentanaDecisionUI ventana in ventanas)
        {
            errores += ValidarReferenciaError(ventana.playerMovement,
                "VentanaDecisionUI no tiene playerMovement", ventana);
            errores += ValidarReferenciaError(ventana.gameManager,
                "VentanaDecisionUI no tiene GameManager", ventana);
        }

        foreach (ObjetoInteractuable objeto in objetos)
        {
            errores += ValidarReferenciaError(objeto.textoInteraccion,
                "falta textoInteraccion", objeto);
            errores += ValidarReferenciaError(objeto.ventanaUI,
                "falta VentanaDecisionUI", objeto);
            errores += ValidarReferenciaError(objeto.gameManager,
                "falta GameManager", objeto);

            Collider2D[] colliders = objeto.GetComponents<Collider2D>();
            if (colliders.Length == 0)
            {
                errores += InformarError("Falta Collider2D.", objeto);
            }
            else
            {
                bool tieneTrigger = false;
                foreach (Collider2D collider in colliders)
                    tieneTrigger |= collider.isTrigger;

                if (!tieneTrigger)
                    errores += InformarError(
                        "Ningun Collider2D tiene Is Trigger activado.", objeto);
            }

            if (objeto.opcionCorrecta < 0 || objeto.opcionCorrecta > 2)
                errores += InformarError(
                    "opcionCorrecta debe estar entre 0 y 2", objeto);

            if (string.IsNullOrWhiteSpace(objeto.tituloObjeto))
                errores += InformarError("el titulo esta vacio", objeto);

            if (string.IsNullOrWhiteSpace(objeto.opcion1) ||
                string.IsNullOrWhiteSpace(objeto.opcion2) ||
                string.IsNullOrWhiteSpace(objeto.opcion3))
            {
                errores += InformarError("una o mas opciones estan vacias", objeto);
            }

            if (objeto.reduccionRiesgo <= 0)
                errores += InformarError(
                    "reduccionRiesgo debe ser mayor que cero", objeto);

            if (gestores.Length == 1 && objeto.gameManager != null &&
                objeto.gameManager != gestores[0])
            {
                advertencias += InformarAdvertencia(
                    "referencia un GameManager distinto al unico gestor de la escena",
                    objeto);
            }

            if (ventanas.Length == 1 && objeto.ventanaUI != null &&
                objeto.ventanaUI != ventanas[0])
            {
                advertencias += InformarAdvertencia(
                    "referencia una VentanaDecisionUI distinta a la unica de la escena",
                    objeto);
            }

            if (textoInteraccionUnico != null && objeto.textoInteraccion != null &&
                objeto.textoInteraccion != textoInteraccionUnico)
            {
                advertencias += InformarAdvertencia(
                    "referencia un TextoInteraccion diferente al objeto unico esperado",
                    objeto);
            }
        }

        string resumen = "Validación completada: " + errores + " errores, " +
                         advertencias + " advertencias";

        if (errores > 0)
            Debug.LogError(resumen);
        else if (advertencias > 0)
            Debug.LogWarning(resumen);
        else
            Debug.Log(resumen);
    }

    private static BoxCollider2D CrearColliderBase(GameObject objeto,
        SpriteRenderer renderer)
    {
        BoxCollider2D collider = Undo.AddComponent<BoxCollider2D>(objeto);
        Bounds limites = renderer.sprite.bounds;
        float alto = limites.size.y * PorcentajeAlto;

        collider.size = new Vector2(limites.size.x * PorcentajeAncho, alto);
        collider.offset = new Vector2(
            limites.center.x,
            limites.min.y + alto * 0.5f
        );
        collider.isTrigger = false;
        EditorUtility.SetDirty(collider);
        return collider;
    }

    private static bool ObtenerSeleccionUnica(out GameObject seleccionado)
    {
        seleccionado = Selection.activeGameObject;
        if (Selection.gameObjects.Length == 1 && seleccionado != null)
            return true;

        Debug.LogWarning("Selecciona exactamente un GameObject.");
        return false;
    }

    private static GameObject BuscarGameObjectUnico(string nombre)
    {
        Scene escena = SceneManager.GetActiveScene();
        GameObject encontrado = null;
        int coincidencias = 0;

        foreach (GameObject raiz in escena.GetRootGameObjects())
        {
            foreach (Transform transformEncontrado in
                     raiz.GetComponentsInChildren<Transform>(true))
            {
                if (transformEncontrado.name != nombre)
                    continue;

                encontrado = transformEncontrado.gameObject;
                coincidencias++;
            }
        }

        return coincidencias == 1 ? encontrado : null;
    }

    private static int ContarGameObjects(string nombre)
    {
        Scene escena = SceneManager.GetActiveScene();
        int coincidencias = 0;

        foreach (GameObject raiz in escena.GetRootGameObjects())
        {
            foreach (Transform transformEncontrado in
                     raiz.GetComponentsInChildren<Transform>(true))
            {
                if (transformEncontrado.name == nombre)
                    coincidencias++;
            }
        }

        return coincidencias;
    }

    private static int ValidarReferenciaError(Object referencia, string mensaje,
        Object contexto)
    {
        return referencia == null ? InformarError(mensaje, contexto) : 0;
    }

    private static int InformarError(string mensaje, Object contexto)
    {
        Debug.LogError("[Mision Aedes] " + mensaje, contexto);
        return 1;
    }

    private static int InformarAdvertencia(string mensaje, Object contexto)
    {
        Debug.LogWarning("[Mision Aedes] " + mensaje, contexto);
        return 1;
    }
}
