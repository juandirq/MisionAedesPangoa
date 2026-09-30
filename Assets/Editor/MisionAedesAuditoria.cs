using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MisionAedesAuditoria
{
    private sealed class Informe
    {
        public int errores, advertencias, informacion;
        public void Error(string texto, Object objeto = null) { errores++; Debug.LogError(texto, objeto); }
        public void Aviso(string texto, Object objeto = null) { advertencias++; Debug.LogWarning(texto, objeto); }
        public void Info(string texto, Object objeto = null) { informacion++; Debug.Log(texto, objeto); }
        public void Requiere(Object referencia, string texto, Object contexto)
        { if (referencia == null) Error(texto, contexto); }
    }

    [MenuItem("Mision Aedes/Auditoria final")]
    private static void Auditar()
    {
        var r = new Informe();
        MisionAedesTools.ValidarObjetosInteractuables(out r.errores, out r.advertencias);
        var jugadores = Object.FindObjectsByType<PlayerMovement>(FindObjectsInactive.Include);
        var gestores = Object.FindObjectsByType<GameManager>(FindObjectsInactive.Include);
        var ventanas = Object.FindObjectsByType<VentanaDecisionUI>(FindObjectsInactive.Include);
        var zonas = Object.FindObjectsByType<ZonaConfigurable>(FindObjectsInactive.Include);
        var progresos = Object.FindObjectsByType<ProgresoZonas>(FindObjectsInactive.Include);
        if (jugadores.Length == 0) r.Error("PLAYER: falta PlayerMovement.");
        if (jugadores.Length > 1) r.Aviso("PLAYER: más de un PlayerMovement.");
        foreach (var jugador in jugadores)
        {
            r.Requiere(jugador.GetComponent<Rigidbody2D>(), "PLAYER: falta Rigidbody2D.", jugador);
            r.Requiere(jugador.GetComponent<Collider2D>(), "PLAYER: falta Collider2D.", jugador);
            r.Requiere(jugador.GetComponent<SpriteRenderer>(), "PLAYER: falta SpriteRenderer.", jugador);
            if (jugador.GetComponent<YSpriteSorter>() == null) r.Aviso("PLAYER: falta YSpriteSorter.", jugador);
            if (!jugador.CompareTag("Player")) r.Error("PLAYER: se necesita tag Player.", jugador);
        }
        if (progresos.Length != 1) r.Error("PROGRESO: debe existir exactamente un ProgresoZonas.");
        foreach (var gestor in gestores)
        {
            r.Requiere(gestor.progresoZonas, "GAMEMANAGER: asigna ProgresoZonas.", gestor);
            r.Requiere(gestor.ventanaDecision, "GAMEMANAGER: falta VentanaDecision.", gestor);
            r.Requiere(gestor.textoInteraccion, "GAMEMANAGER: falta TextoInteraccion.", gestor);
            r.Requiere(gestor.textoMotivoDerrota, "GAMEMANAGER: falta motivo de derrota.", gestor);
            RevisarCampos(gestor, new[] { "textoRiesgo", "textoAciertos", "textoErrores", "textoTiempo", "textoObjetivos" }, r);
            RevisarOpcionales(gestor, r);
            RevisarBotones(gestor.panelVictoria, "ContinuarDespuesDeVictoria", r);
            RevisarBotones(gestor.panelDerrota, "ReiniciarZona", r);
        }
        foreach (var ventana in ventanas)
        {
            RevisarCampos(ventana, new[] { "ventanaDecision", "tituloObjeto", "descripcionObjeto",
                "textoFeedback", "textoBoton1", "textoBoton2", "textoBoton3" }, r);
            foreach (string metodo in new[] { "Opcion1", "Opcion2", "Opcion3" })
                RevisarBotones(ventana.ventanaDecision, metodo, r);
        }
        foreach (var zona in zonas)
        {
            if (zona.indiceZona < 1 || zona.indiceZona > 3) r.Error("ZONA: índice fuera de 1–3.", zona);
            if (string.IsNullOrWhiteSpace(zona.nombreZona)) r.Error("ZONA: falta nombre.", zona);
            if (zona.cantidadObjetivos <= 0 || zona.tiempoZona <= 0f ||
                float.IsNaN(zona.tiempoZona) || float.IsInfinity(zona.tiempoZona))
                r.Error("ZONA: objetivos/tiempo inválidos.", zona);
            r.Requiere(zona.gameManager, "ZONA: falta GameManager.", zona);
            r.Requiere(zona.playerMovement, "ZONA: falta Player.", zona);
            r.Requiere(zona.progresoZonas, "ZONA: falta ProgresoZonas.", zona);
            if (zona.gameManager != null && zona.progresoZonas != zona.gameManager.progresoZonas)
                r.Error("ZONA: progreso distinto al de GameManager.", zona);
            if (!zona.iniciarAutomaticamente)
            {
                r.Requiere(zona.panelInicio, "ZONA: falta panel inicial.", zona);
                if (!zona.GetComponents<Collider2D>().Any(c => c.isTrigger && c.enabled))
                    r.Error("ZONA: falta collider trigger habilitado.", zona);
                RevisarBotones(zona.panelInicio, "ComenzarZona", r);
            }
            if (zonas.Count(z => z.indiceZona == zona.indiceZona) > 1)
                r.Error("ZONA: índice duplicado.", zona);
        }
        var inicios2 = Object.FindObjectsByType<InicioZona2UI>(FindObjectsInactive.Include);
        foreach (var inicio in inicios2)
        {
            r.Requiere(inicio.gameManager, "ZONA 2: falta GameManager.", inicio);
            r.Requiere(inicio.panelInicioZona2, "ZONA 2: falta panel inicial.", inicio);
            if (inicio.indiceZona != 2 || inicio.objetivosZona != 6 || !Mathf.Approximately(inicio.tiempoZona, 345f))
                r.Error("ZONA 2: debe conservar índice 2, seis objetivos y 345 segundos.", inicio);
            if (zonas.Any(z => z.indiceZona == inicio.indiceZona))
                r.Error("ZONA 2: inicio antiguo y ZonaConfigurable compiten por el mismo índice.", inicio);
            RevisarBotones(inicio.panelInicioZona2, "ComenzarInspeccion", r);
        }
        foreach (var trigger in Object.FindObjectsByType<TriggerZona2>(FindObjectsInactive.Include))
        {
            r.Requiere(trigger.panelInicioZona2, "TRIGGER ZONA 2: falta panel.", trigger);
            if (trigger.progresoZonas == null || trigger.gameManager == null)
                r.Aviso("TRIGGER ZONA 2: asigna progreso y GameManager explícitamente (hay búsqueda de respaldo).", trigger);
        }
        for (int i = 1; i <= 3; i++)
            if (!zonas.Any(z => z.indiceZona == i) && !(i == 2 && inicios2.Length > 0))
                r.Error($"ZONA {i}: falta componente de inicio configurado.");
        var objetos = Object.FindObjectsByType<ObjetoInteractuable>(FindObjectsInactive.Include);
        foreach (var objeto in objetos)
        {
            if (objeto.indiceZona == 0) r.Aviso("INTERACTUABLE: asigna índice de zona para evitar contabilizar en otra misión.", objeto);
            else if (objeto.indiceZona < 1 || objeto.indiceZona > 3) r.Error("INTERACTUABLE: índice inválido.", objeto);
            if (objeto.indiceZona == 2 && objeto.reduccionRiesgo != 15)
                r.Error("INTERACTUABLE ZONA 2: debe reducir 15 puntos.", objeto);
            if (objeto.ventanaUI != null && objeto.gameManager != objeto.ventanaUI.gameManager)
                r.Error("INTERACTUABLE y ventana apuntan a gestores diferentes.", objeto);
        }
        foreach (var zona in zonas)
        {
            var asignados = objetos.Where(o => o.indiceZona == zona.indiceZona).ToArray();
            if (asignados.Length != zona.cantidadObjetivos)
                r.Aviso($"{zona.nombreZona}: {asignados.Length} objetos asignados; se esperan {zona.cantidadObjetivos}.", zona);
            if (asignados.Sum(o => o.reduccionRiesgo) < 80)
                r.Aviso($"{zona.nombreZona}: las reducciones asignadas no alcanzan los 80 puntos necesarios.", zona);
        }
        foreach (var jugador in jugadores)
        {
            var sprite = jugador.GetComponent<SpriteRenderer>();
            if (sprite == null || jugador.GetComponent<YSpriteSorter>() == null) continue;
            foreach (var sorter in Object.FindObjectsByType<YSpriteSorter>(FindObjectsInactive.Include))
            {
                var otro = sorter.GetComponent<SpriteRenderer>();
                if (otro != null && otro.sortingLayerID != sprite.sortingLayerID)
                    r.Aviso("PROFUNDIDAD: sorter usa otra Sorting Layer que el jugador; revisar si es intencional.", sorter);
            }
        }
        var pausas = Object.FindObjectsByType<MenuPausaUI>(FindObjectsInactive.Include);
        if (pausas.Length == 0) r.Aviso("PAUSA: falta MenuPausaUI.");
        foreach (var pausa in pausas)
        {
            RevisarCampos(pausa, new[] { "panelPausa", "gameManager", "playerMovement" }, r);
            if (pausa.panelPausa != null && pausa.transform.IsChildOf(pausa.panelPausa.transform))
                r.Error("PAUSA: coloca MenuPausaUI fuera de PanelPausa en un objeto siempre activo.", pausa);
            RevisarBotones(pausa.panelPausa, "Reanudar", r);
            RevisarBotones(pausa.panelPausa, "ReiniciarZona", r);
        }
        var escenas = EditorBuildSettings.scenes.Where(s => s.enabled).ToArray();
        if (escenas.Length == 0) r.Error("BUILD: no hay escenas habilitadas.");
        if (!escenas.Any(s => s.path == SceneManager.GetActiveScene().path))
            r.Error("BUILD: la escena actual no está incluida/habilitada.");
        r.Info("El estado resuelto y las colisiones requieren Play Mode. Las referencias opcionales vacías son válidas.");
        Debug.Log($"AUDITORIA FINAL\nErrores críticos: {r.errores}\nAdvertencias: {r.advertencias}\nInformación: {r.informacion}");
    }

    private static void RevisarCampos(Object objeto, string[] nombres, Informe r)
    {
        var serializado = new SerializedObject(objeto);
        foreach (string nombre in nombres)
        {
            var campo = serializado.FindProperty(nombre);
            if (campo != null && campo.objectReferenceValue == null)
                r.Error(objeto.name + ": falta " + nombre, objeto);
        }
    }

    private static void RevisarOpcionales(GameManager gestor, Informe r)
    {
        var serializado = new SerializedObject(gestor);
        var propiedad = serializado.GetIterator();
        while (propiedad.NextVisible(true))
            if (propiedad.propertyType == SerializedPropertyType.ObjectReference &&
                propiedad.objectReferenceValue == null && propiedad.objectReferenceEntityIdValue != EntityId.None)
                r.Aviso("Referencia destruida: " + propiedad.displayName, gestor);
    }

    private static void RevisarBotones(GameObject panel, string metodo, Informe r)
    {
        if (panel == null) return;
        bool conectado = false;
        foreach (var boton in panel.GetComponentsInChildren<Button>(true))
            for (int i = 0; i < boton.onClick.GetPersistentEventCount(); i++)
                if (boton.onClick.GetPersistentTarget(i) != null &&
                    boton.onClick.GetPersistentMethodName(i) == metodo &&
                    boton.onClick.GetPersistentListenerState(i) != UnityEngine.Events.UnityEventCallState.Off)
                    conectado = true;
        if (!conectado) r.Aviso("UI: no se detectó un botón conectado a " + metodo + " en " + panel.name, panel);
    }
}
