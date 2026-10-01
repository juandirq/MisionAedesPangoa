using UnityEngine;

public class ZonaConfigurable : MonoBehaviour
{
    [Header("Identificacion")]
    public string nombreZona = "Zona 3";
    [Range(1, 3)] public int indiceZona = 3;
    public ProgresoZonas progresoZonas;
    public bool iniciarAutomaticamente;
    public TMPro.TextMeshProUGUI mensajeZonaBloqueada;
    public GameObject panelZonaBloqueada;
    public ZonaBannerUI banner;
    public string subtituloBanner;

    [Header("Referencias")]
    public GameObject panelInicio;
    public GameManager gameManager;
    public PlayerMovement playerMovement;

    [Header("Configuracion")]
    [Min(1)] public int cantidadObjetivos = 1;
    [Min(1f)] public float tiempoZona = 120f;

    private bool yaSeActivo = false;
    private bool esperandoComenzar;

    private void Start()
    {
        if (progresoZonas == null && gameManager != null)
            progresoZonas = gameManager.progresoZonas;
        if (iniciarAutomaticamente) ComenzarZona();
    }

    private bool Disponible()
    {
        if (gameManager == null || progresoZonas == null || playerMovement == null ||
            indiceZona < 1 || indiceZona > 3 || cantidadObjetivos <= 0 || tiempoZona <= 0f ||
            float.IsNaN(tiempoZona) || float.IsInfinity(tiempoZona))
        {
            Debug.LogWarning(nombreZona + ": revisa referencias e índice/objetivos/tiempo.", this);
            return false;
        }
        if (gameManager.progresoZonas != progresoZonas)
        {
            Debug.LogWarning("La zona y GameManager deben compartir ProgresoZonas.", this);
            return false;
        }
        if (!progresoZonas.EstaZonaDesbloqueada(indiceZona))
        {
            if (mensajeZonaBloqueada != null)
                mensajeZonaBloqueada.text = "Completa primero la zona anterior.";
            if (panelZonaBloqueada != null) panelZonaBloqueada.SetActive(true);
            else Debug.Log("Completa primero la zona anterior.", this);
            return false;
        }
        return !yaSeActivo && gameManager.PuedeIniciarZona(indiceZona);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (yaSeActivo || esperandoComenzar || !other.CompareTag("Player"))
            return;

        if (playerMovement == null) playerMovement = other.GetComponentInParent<PlayerMovement>();
        if (!Disponible() || !playerMovement.enabled || Time.timeScale <= 0f) return;

        if (panelInicio == null)
        {
            Debug.LogWarning(nombreZona + ": falta asignar el panel de inicio.", this);
            return;
        }

        esperandoComenzar = true;

        if (playerMovement != null)
            playerMovement.enabled = false;

        panelInicio.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) CerrarAvisoBloqueo();
    }

    public void CerrarAvisoBloqueo()
    {
        if (panelZonaBloqueada != null) panelZonaBloqueada.SetActive(false);
    }

    public bool IntentarComenzarDesdeTrigger()
    {
        if (yaSeActivo) return false;

        bool esperandoAntes = esperandoComenzar;
        esperandoComenzar = true;
        ComenzarZona();

        if (!yaSeActivo)
            esperandoComenzar = esperandoAntes;

        return yaSeActivo;
    }

    public void ComenzarZona()
    {
        if (!Disponible()) return;
        if (!iniciarAutomaticamente && !esperandoComenzar) return;

        if (panelInicio != null)
            panelInicio.SetActive(false);

        gameManager.IniciarZona(cantidadObjetivos, tiempoZona, nombreZona, indiceZona);
        if (!gameManager.ZonaActiva) return;
        yaSeActivo = true;
        esperandoComenzar = false;
        CerrarAvisoBloqueo();
        if (banner != null) banner.Mostrar(nombreZona, subtituloBanner);

        if (playerMovement != null)
            playerMovement.enabled = true;

        Debug.Log(nombreZona + " iniciada. Objetivos: " + cantidadObjetivos +
                  " | Tiempo: " + tiempoZona, this);
    }
}
