using UnityEngine;

public class ObjetoInteractuable : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject textoInteraccion;
    public VentanaDecisionUI ventanaUI;
    public GameManager gameManager;

    [Header("Datos del objeto")]
    public string tituloObjeto;

    [TextArea(2, 4)]
    public string descripcionObjeto;

    [Header("Opciones")]
    public string opcion1;
    public string opcion2;
    public string opcion3;

    [Tooltip("0 = opción 1, 1 = opción 2, 2 = opción 3")]
    public int opcionCorrecta;

    [Header("Retroalimentación")]
    [TextArea(2, 4)]
    public string feedbackCorrecto;

    [TextArea(2, 4)]
    public string feedbackIncorrecto;

    [Header("Riesgo")]
    public int reduccionRiesgo = 20;
    [Tooltip("0 conserva compatibilidad. Asigna 1, 2 o 3 para impedir contar en otra zona.")]
    [Range(0, 3)] public int indiceZona;
    public bool Resuelto => resuelto;
    public event System.Action AlResolverse;

    public bool PuedeResponder => !resuelto && gameManager != null &&
        gameManager.ZonaActiva && !gameManager.JuegoTerminado &&
        (indiceZona == 0 || indiceZona == gameManager.indiceZonaActual);

    private bool jugadorCerca = false;
    private bool resuelto = false;

    void Update()
    {
        if (jugadorCerca &&
            PuedeResponder && Time.timeScale > 0f &&
            Input.GetKeyDown(KeyCode.E))
        {
            AbrirInspeccion();
        }
    }

    void AbrirInspeccion()
    {
        CambiarVisibilidadTextoInteraccion(false);

        if (ventanaUI != null)
        {
            ventanaUI.AbrirVentana(this);
        }
    }

    public bool ProcesarRespuesta(int opcionElegida)
    {
        if (!PuedeResponder)
            return false;

        if (opcionElegida == opcionCorrecta)
        {
            resuelto = true;

            if (gameManager != null)
            {
                gameManager.RegistrarAcierto(reduccionRiesgo);
            }
            AlResolverse?.Invoke();

            return true;
        }

        if (gameManager != null)
        {
            gameManager.RegistrarError();
        }

        return false;
    }

    public void AlCerrarVentana()
    {
        if (jugadorCerca && !resuelto)
        {
            CambiarVisibilidadTextoInteraccion(true);
        }
    }

    private void CambiarVisibilidadTextoInteraccion(bool visible)
    {
        // La comparacion de Unity tambien detecta objetos que ya fueron destruidos.
        if (textoInteraccion != null)
        {
            textoInteraccion.SetActive(visible);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;

            if (!resuelto)
            {
                CambiarVisibilidadTextoInteraccion(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            CambiarVisibilidadTextoInteraccion(false);
        }
    }
}
