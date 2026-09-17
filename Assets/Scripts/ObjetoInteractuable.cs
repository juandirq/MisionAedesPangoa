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

    private bool jugadorCerca = false;
    private bool resuelto = false;

    void Update()
    {
        if (jugadorCerca &&
            !resuelto &&
            Input.GetKeyDown(KeyCode.E))
        {
            AbrirInspeccion();
        }
    }

    void AbrirInspeccion()
    {
        textoInteraccion.SetActive(false);

        if (ventanaUI != null)
        {
            ventanaUI.AbrirVentana(this);
        }
    }

    public bool ProcesarRespuesta(int opcionElegida)
    {
        if (resuelto)
            return false;

        if (opcionElegida == opcionCorrecta)
        {
            resuelto = true;

            if (gameManager != null)
            {
                gameManager.RegistrarAcierto(reduccionRiesgo);
            }

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
            textoInteraccion.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;

            if (!resuelto)
            {
                textoInteraccion.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            textoInteraccion.SetActive(false);
        }
    }
}