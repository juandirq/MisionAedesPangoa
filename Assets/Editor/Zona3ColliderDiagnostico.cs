using System.Text;
using UnityEditor;
using UnityEngine;

public sealed class Zona3ColliderDiagnostico : EditorWindow
{
    private const string MenuRuta = "Mision Aedes/Diagnosticar bloqueo Zona 3";
    private const int CapacidadResultados = 128;

    private readonly RaycastHit2D[] resultados = new RaycastHit2D[CapacidadResultados];
    private BoxCollider2D colliderPlayer;
    private PlayerMovement movimientoPlayer;
    private bool monitoreando;
    private bool seleccionarImpactado = true;
    private float distanciaCast = 0.35f;
    private Collider2D ultimoCollider;
    private Vector2 ultimaDireccion;
    private string ultimoResultado = "Sin impactos registrados.";

    [MenuItem(MenuRuta)]
    private static void Abrir()
    {
        GetWindow<Zona3ColliderDiagnostico>("Bloqueo Zona 3");
    }

    private void OnEnable()
    {
        EditorApplication.update += ActualizarDiagnostico;
    }

    private void OnDisable()
    {
        EditorApplication.update -= ActualizarDiagnostico;
        monitoreando = false;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Diagnóstico físico Zona 2 → Zona 3", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "No modifica la escena. En Play Mode realiza un cast con el BoxCollider2D real " +
            "del Player en la dirección de WASD/flechas y registra el primer collider sólido.",
            MessageType.Info);

        distanciaCast = EditorGUILayout.Slider(
            new GUIContent("Distancia del cast", "Distancia de anticipación desde el collider actual."),
            distanciaCast, 0.05f, 1.5f);
        seleccionarImpactado = EditorGUILayout.Toggle("Seleccionar FIRST HIT", seleccionarImpactado);

        using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
        {
            string etiqueta = monitoreando ? "Detener monitoreo" : "Iniciar monitoreo";
            if (GUILayout.Button(etiqueta, GUILayout.Height(28f)))
            {
                if (monitoreando)
                    Detener();
                else
                    Iniciar();
            }
        }

        if (!EditorApplication.isPlaying)
            EditorGUILayout.HelpBox("Entra en Play Mode y pulsa “Iniciar monitoreo”.", MessageType.Warning);
        else if (monitoreando)
            EditorGUILayout.HelpBox("MONITOREANDO: intenta avanzar normalmente hacia Zona 3.", MessageType.None);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Último resultado", EditorStyles.boldLabel);
        EditorGUILayout.SelectableLabel(ultimoResultado, EditorStyles.textArea,
            GUILayout.MinHeight(120f), GUILayout.ExpandHeight(true));

        if (GUILayout.Button("Limpiar último resultado"))
        {
            ultimoCollider = null;
            ultimaDireccion = Vector2.zero;
            ultimoResultado = "Sin impactos registrados.";
            Repaint();
        }
    }

    private void Iniciar()
    {
        movimientoPlayer = Object.FindAnyObjectByType<PlayerMovement>();
        colliderPlayer = movimientoPlayer != null
            ? movimientoPlayer.GetComponent<BoxCollider2D>()
            : null;

        if (movimientoPlayer == null || colliderPlayer == null)
        {
            Debug.LogError("[DIAGNÓSTICO ZONA 3] No se encontró PlayerMovement con BoxCollider2D activo.");
            ultimoResultado = "ERROR: no se encontró el Player o su BoxCollider2D.";
            return;
        }

        monitoreando = true;
        ultimoCollider = null;
        ultimaDireccion = Vector2.zero;
        ultimoResultado = "Monitoreando. Mueve el Player hacia Zona 3.";

        Debug.Log(
            "[DIAGNÓSTICO ZONA 3] Monitoreo iniciado.\n" +
            $"Player: {RutaJerarquica(colliderPlayer.transform)}\n" +
            $"BoxCollider2D size local: {VectorTexto(colliderPlayer.size)}\n" +
            $"Offset local: {VectorTexto(colliderPlayer.offset)}\n" +
            $"Bounds actuales: {BoundsTexto(colliderPlayer.bounds)}\n" +
            $"Layer: {LayerTexto(colliderPlayer.gameObject.layer)}\n" +
            $"Distancia cast: {distanciaCast:F3}", colliderPlayer);
        Repaint();
    }

    private void Detener()
    {
        monitoreando = false;
        colliderPlayer = null;
        movimientoPlayer = null;
        ultimoCollider = null;
        ultimaDireccion = Vector2.zero;
        Debug.Log("[DIAGNÓSTICO ZONA 3] Monitoreo detenido.");
        Repaint();
    }

    private void ActualizarDiagnostico()
    {
        if (!monitoreando)
            return;

        if (!EditorApplication.isPlaying)
        {
            Detener();
            return;
        }

        if (colliderPlayer == null || !colliderPlayer.enabled ||
            !colliderPlayer.gameObject.activeInHierarchy)
        {
            ultimoResultado = "El BoxCollider2D del Player no está disponible o está deshabilitado.";
            Repaint();
            return;
        }

        Vector2 direccion = LeerDireccion();
        if (direccion.sqrMagnitude < 0.001f)
        {
            ultimoCollider = null;
            ultimaDireccion = Vector2.zero;
            return;
        }

        direccion.Normalize();

        int mascaraColision = Physics2D.GetLayerCollisionMask(colliderPlayer.gameObject.layer);
        var filtro = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = mascaraColision,
            useTriggers = false
        };

        int cantidad = colliderPlayer.Cast(
            direccion, filtro, resultados, distanciaCast, true);

        RaycastHit2D primero = default;
        bool encontrado = false;

        for (int i = 0; i < cantidad; i++)
        {
            RaycastHit2D candidato = resultados[i];
            Collider2D otro = candidato.collider;
            if (otro == null || !otro.enabled || otro.isTrigger ||
                !otro.gameObject.activeInHierarchy || EsParteDelPlayer(otro.transform))
                continue;

            if (!encontrado || candidato.distance < primero.distance)
            {
                primero = candidato;
                encontrado = true;
            }
        }

        if (!encontrado)
        {
            ultimoCollider = null;
            ultimaDireccion = direccion;
            return;
        }

        if (primero.collider == ultimoCollider && Vector2.Dot(direccion, ultimaDireccion) > 0.999f)
            return;

        ultimoCollider = primero.collider;
        ultimaDireccion = direccion;
        RegistrarFirstHit(primero, direccion, mascaraColision);
    }

    private void RegistrarFirstHit(RaycastHit2D hit, Vector2 direccion, int mascara)
    {
        Collider2D impactado = hit.collider;
        int layer = impactado.gameObject.layer;
        var texto = new StringBuilder();
        texto.AppendLine("========== FIRST HIT ZONA 3 ==========");
        texto.AppendLine($"GameObject: {impactado.gameObject.name}");
        texto.AppendLine($"Path: {RutaJerarquica(impactado.transform)}");
        texto.AppendLine($"Collider: {impactado.GetType().Name}");
        texto.AppendLine($"Is Trigger: {impactado.isTrigger}");
        texto.AppendLine($"Enabled: {impactado.enabled}");
        texto.AppendLine($"Active In Hierarchy: {impactado.gameObject.activeInHierarchy}");
        texto.AppendLine($"Bounds: {BoundsTexto(impactado.bounds)}");
        texto.AppendLine($"Point: {VectorTexto(hit.point)}");
        texto.AppendLine($"Distance: {hit.distance:F6}");
        texto.AppendLine($"Layer: {LayerTexto(layer)}");
        texto.AppendLine($"Dirección cast: {VectorTexto(direccion)}");
        texto.AppendLine($"Layer mask usada: 0x{mascara:X8}");
        texto.AppendLine($"Player bounds: {BoundsTexto(colliderPlayer.bounds)}");
        texto.Append("======================================");

        ultimoResultado = texto.ToString();
        Debug.Log(ultimoResultado, impactado);

        if (seleccionarImpactado)
        {
            Selection.activeGameObject = impactado.gameObject;
            EditorGUIUtility.PingObject(impactado.gameObject);
        }

        Repaint();
    }

    private bool EsParteDelPlayer(Transform otro)
    {
        Transform raizPlayer = colliderPlayer.transform;
        return otro == raizPlayer || otro.IsChildOf(raizPlayer) || raizPlayer.IsChildOf(otro);
    }

    private static Vector2 LeerDireccion()
    {
        Vector2 direccion = Vector2.zero;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) direccion.x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) direccion.x += 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) direccion.y += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) direccion.y -= 1f;
        return direccion;
    }

    private static string RutaJerarquica(Transform objetivo)
    {
        if (objetivo == null)
            return "<null>";

        string ruta = objetivo.name;
        while (objetivo.parent != null)
        {
            objetivo = objetivo.parent;
            ruta = objetivo.name + "/" + ruta;
        }
        return ruta;
    }

    private static string LayerTexto(int layer)
    {
        string nombre = LayerMask.LayerToName(layer);
        return string.IsNullOrEmpty(nombre) ? layer.ToString() : $"{nombre} ({layer})";
    }

    private static string VectorTexto(Vector2 valor) =>
        $"({valor.x:F6}, {valor.y:F6})";

    private static string BoundsTexto(Bounds bounds) =>
        $"center={VectorTexto(bounds.center)}, min={VectorTexto(bounds.min)}, " +
        $"max={VectorTexto(bounds.max)}, size={VectorTexto(bounds.size)}";
}
