using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("HUD")]
    public TextMeshProUGUI textoRiesgo;
    public TextMeshProUGUI textoAciertos;
    public TextMeshProUGUI textoErrores;
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoObjetivos;

    [Header("Resultados de la zona")]
    public TextMeshProUGUI resultadoRiesgo;
    public TextMeshProUGUI resultadoObjetivos;
    public TextMeshProUGUI resultadoAciertos;
    public TextMeshProUGUI resultadoErrores;
    public TextMeshProUGUI resultadoTiempo;
    public TextMeshProUGUI resultadoNombreZona;

    [Header("Resultados de derrota opcionales")]
    public TextMeshProUGUI derrotaRiesgo;
    public TextMeshProUGUI derrotaObjetivos;
    public TextMeshProUGUI derrotaAciertos;
    public TextMeshProUGUI derrotaErrores;
    public TextMeshProUGUI derrotaTiempo;
    public TextMeshProUGUI derrotaNombreZona;

    [Header("Paneles")]
    public GameObject panelDerrota;
    public GameObject panelVictoria;
    public GameObject ventanaDecision;
    public GameObject textoInteraccion;

    [Header("Paneles HUD")]
    public GameObject panelHUDIzquierdo;
    public GameObject panelHUDDerecho;

    [Header("Texto de derrota")]
    public TextMeshProUGUI textoMotivoDerrota;

    [Header("Jugador")]
    public PlayerMovement playerMovement;

    [Header("Configuración actual")]
    public float tiempoRestante = 0f;
    public int objetivosTotales = 0;
    public string nombreZonaActual = "Zona";
    [Range(0, 3)] public int indiceZonaActual;
    public ProgresoZonas progresoZonas;

    [Header("Ruta lineal")]
    public RutaLinealController rutaLineal;

    private int objetivosCompletados = 0;
    private int riesgo = 100;
    private int aciertos = 0;
    private int errores = 0;

    private bool juegoTerminado = false;
    private bool cronometroActivo = false;
    private bool zonaActiva = false;
    private bool cronometroPausado = false;
    private bool victoriaPendiente = false;
    private bool fueVictoria;

    public bool JuegoTerminado => juegoTerminado;
    public bool ZonaActiva => zonaActiva;
    public bool CronometroEnMarcha => cronometroActivo;

    void Awake()
    {
        if (progresoZonas == null)
            progresoZonas = FindAnyObjectByType<ProgresoZonas>();
        Time.timeScale = 1f;

        cronometroActivo = false;
        zonaActiva = false;
        cronometroPausado = false;
        victoriaPendiente = false;

        if (panelDerrota != null)
            panelDerrota.SetActive(false);

        if (panelVictoria != null)
            panelVictoria.SetActive(false);

        ActualizarHUD();
        MostrarHUDExploracion(0);
    }

    void Update()
    {
        if (juegoTerminado)
            return;

        if (!cronometroActivo)
            return;

        tiempoRestante -= Time.deltaTime;

        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;

            ActualizarTiempo();

            Derrota("Se acabó el tiempo.");

            return;
        }

        ActualizarTiempo();
    }

    public void IniciarZona(int cantidadObjetivos, float tiempoZona)
    {
        IniciarZona(cantidadObjetivos, tiempoZona, "Zona");
    }

    public void IniciarZona(int cantidadObjetivos, float tiempoZona,
        string nombreZona)
    {
        IniciarZona(cantidadObjetivos, tiempoZona, nombreZona, 0);
    }

    public bool PuedeIniciarZona(int indice)
    {
        return !zonaActiva && !juegoTerminado && indice >= 0 && indice <= 3 &&
            (indice == 0 || progresoZonas == null ||
             (progresoZonas.EstaZonaDesbloqueada(indice) &&
              !progresoZonas.EstaZonaCompletada(indice)));
    }

    public void IniciarZona(int cantidadObjetivos, float tiempoZona,
        string nombreZona, int indiceZona)
    {
        if (!PuedeIniciarZona(indiceZona) || cantidadObjetivos <= 0 ||
            tiempoZona <= 0f || float.IsNaN(tiempoZona) || float.IsInfinity(tiempoZona))
        {
            Debug.LogWarning("No se puede iniciar la zona: revisa progreso, estado y configuración.", this);
            return;
        }
        indiceZonaActual = indiceZona;
        fueVictoria = false;
        objetivosTotales = cantidadObjetivos;
        tiempoRestante = tiempoZona;
        nombreZonaActual = string.IsNullOrWhiteSpace(nombreZona)
            ? "Zona"
            : nombreZona;

        objetivosCompletados = 0;
        riesgo = 100;
        aciertos = 0;
        errores = 0;

        juegoTerminado = false;
        zonaActiva = true;
        cronometroActivo = true;
        cronometroPausado = false;
        victoriaPendiente = false;

        AudioManager.Instancia?.ReproducirMusicaZona(indiceZonaActual);

        if (panelDerrota != null)
            panelDerrota.SetActive(false);

        if (panelVictoria != null)
            panelVictoria.SetActive(false);

        if (panelHUDIzquierdo != null)
            panelHUDIzquierdo.SetActive(true);

        if (panelHUDDerecho != null)
            panelHUDDerecho.SetActive(true);

        if (playerMovement != null)
            playerMovement.enabled = true;

        ActualizarHUD();

        Debug.Log(
            "Zona iniciada. Objetivos: " +
            objetivosTotales +
            " | Tiempo: " +
            tiempoRestante +
            " | Nombre: " +
            nombreZonaActual
        );
    }

    public void PausarCronometro()
    {
        if (!zonaActiva || juegoTerminado)
            return;

        cronometroPausado = true;
        cronometroActivo = false;
    }

    public void ReanudarCronometro()
    {
        if (!zonaActiva || juegoTerminado || !cronometroPausado)
            return;

        cronometroPausado = false;
        cronometroActivo = true;
    }

    public void RegistrarAcierto(int reduccionRiesgo)
    {
        if (juegoTerminado || !zonaActiva)
            return;

        aciertos++;
        objetivosCompletados++;

        riesgo -= reduccionRiesgo;

        if (riesgo < 0)
            riesgo = 0;

        ActualizarHUD();
        ComprobarVictoria();
    }

    public void RegistrarError()
    {
        if (juegoTerminado || !zonaActiva)
            return;

        errores++;

        ActualizarHUD();

        if (errores >= 3)
        {
            Derrota("Alcanzaste 3 errores.");
        }
    }

    void ComprobarVictoria()
    {
        if (objetivosCompletados >= objetivosTotales &&
            riesgo <= 20)
        {
            Victoria();
        }
    }

    void ActualizarHUD()
    {
        if (textoRiesgo != null)
            textoRiesgo.text = "Riesgo: " + riesgo + "%";

        if (textoAciertos != null)
            textoAciertos.text = "Aciertos: " + aciertos;

        if (textoErrores != null)
            textoErrores.text = "Errores: " + errores;

        if (textoObjetivos != null)
        {
            textoObjetivos.text =
                "Objetivos: " +
                objetivosCompletados +
                "/" +
                objetivosTotales;
        }

        ActualizarTiempo();
    }

    private void MostrarHUDExploracion(int zonaCompletada)
    {
        if (panelHUDIzquierdo != null)
            panelHUDIzquierdo.SetActive(true);

        if (panelHUDDerecho != null)
            panelHUDDerecho.SetActive(true);

        string destino;
        string siguiente;
        int progreso;

        if (zonaCompletada == 1)
        {
            destino = "Zona 2";
            siguiente = "Área residencial";
            progreso = 1;
        }
        else if (zonaCompletada >= 2)
        {
            destino = "Zona 3";
            siguiente = "Espacio comunitario";
            progreso = 2;
        }
        else
        {
            destino = "Escuela";
            siguiente = "Sigue el cartel";
            progreso = 0;
        }

        if (textoRiesgo != null)
            textoRiesgo.text = "Destino: " + destino;

        if (textoTiempo != null)
            textoTiempo.text = "Estado: Explorando";

        if (textoErrores != null)
            textoErrores.text = "Progreso: " + progreso + "/3";

        if (textoAciertos != null)
            textoAciertos.text = "Siguiente: " + siguiente;

        if (textoObjetivos != null)
            textoObjetivos.text = "Objetivo: Llegar";
    }

    void ActualizarTiempo()
    {
        if (textoTiempo != null)
        {
            textoTiempo.text = "Tiempo: " + FormatearTiempo(tiempoRestante);
        }
    }

    string FormatearTiempo(float tiempo)
    {
        int minutos = Mathf.FloorToInt(Mathf.Max(0f, tiempo) / 60f);
        int segundos = Mathf.FloorToInt(Mathf.Max(0f, tiempo) % 60f);
        return minutos.ToString("00") + ":" + segundos.ToString("00");
    }

    void ActualizarResultados()
    {
        if (resultadoNombreZona != null)
            resultadoNombreZona.text = nombreZonaActual;

        if (resultadoRiesgo != null)
            resultadoRiesgo.text = "Riesgo final: " + riesgo + "%";

        if (resultadoObjetivos != null)
        {
            resultadoObjetivos.text =
                "Objetivos: " + objetivosCompletados + "/" + objetivosTotales;
        }

        if (resultadoAciertos != null)
            resultadoAciertos.text = "Aciertos: " + aciertos;

        if (resultadoErrores != null)
            resultadoErrores.text = "Errores: " + errores;

        if (resultadoTiempo != null)
            resultadoTiempo.text = "Tiempo restante: " +
                                    FormatearTiempo(tiempoRestante);
    }

    void ActualizarResultadosDerrota()
    {
        if (derrotaNombreZona != null)
            derrotaNombreZona.text = nombreZonaActual;

        if (derrotaRiesgo != null)
            derrotaRiesgo.text = "Riesgo final: " + riesgo + "%";

        if (derrotaObjetivos != null)
        {
            derrotaObjetivos.text =
                "Objetivos: " + objetivosCompletados + "/" + objetivosTotales;
        }

        if (derrotaAciertos != null)
            derrotaAciertos.text = "Aciertos: " + aciertos;

        if (derrotaErrores != null)
            derrotaErrores.text = "Errores: " + errores;

        if (derrotaTiempo != null)
            derrotaTiempo.text = "Tiempo restante: " +
                                  FormatearTiempo(tiempoRestante);
    }

    void Victoria()
    {
        if (juegoTerminado)
            return;

        juegoTerminado = true;
        zonaActiva = false;
        cronometroActivo = false;
        cronometroPausado = false;
        victoriaPendiente = true;
        fueVictoria = true;
        AudioManager.Instancia?.ReproducirVictoria();
        if (progresoZonas != null && indiceZonaActual > 0)
            progresoZonas.CompletarZona(indiceZonaActual);

        if (rutaLineal != null)
            rutaLineal.ZonaCompletada(indiceZonaActual);

        if (textoInteraccion != null)
            textoInteraccion.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (panelHUDIzquierdo != null)
            panelHUDIzquierdo.SetActive(false);

        if (panelHUDDerecho != null)
            panelHUDDerecho.SetActive(false);

        ActualizarResultados();

        // Si la victoria se produjo al responder la ultima pregunta, la ventana
        // conserva el feedback durante sus 4 segundos antes de mostrar resultados.
        if (ventanaDecision == null || !ventanaDecision.activeSelf)
            MostrarVictoriaPendiente();
    }

    public void MostrarVictoriaPendiente()
    {
        if (!victoriaPendiente)
            return;

        victoriaPendiente = false;

        if (panelVictoria != null)
            panelVictoria.SetActive(true);
    }

    public void ContinuarDespuesDeVictoria()
    {
        if (!fueVictoria || victoriaPendiente)
            return;
        int zonaCompletada = indiceZonaActual;
        fueVictoria = false;
        if (panelVictoria != null)
            panelVictoria.SetActive(false);

        MostrarHUDExploracion(zonaCompletada);

        if (zonaCompletada < 3)
            AudioManager.Instancia?.ReproducirMusicaExploracion();

        victoriaPendiente = false;
        juegoTerminado = false;
        zonaActiva = false;
        cronometroActivo = false;
        cronometroPausado = false;

        if (playerMovement != null)
            playerMovement.enabled = true;
    }

    void Derrota(string motivo)
    {
        if (juegoTerminado)
            return;

        AudioManager.Instancia?.ReproducirDerrota();

        juegoTerminado = true;
        zonaActiva = false;
        cronometroActivo = false;
        cronometroPausado = false;
        victoriaPendiente = false;

        if (ventanaDecision != null)
            ventanaDecision.SetActive(false);

        if (textoInteraccion != null)
            textoInteraccion.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (panelHUDIzquierdo != null)
            panelHUDIzquierdo.SetActive(false);

        if (panelHUDDerecho != null)
            panelHUDDerecho.SetActive(false);

        if (textoMotivoDerrota != null)
            textoMotivoDerrota.text = motivo;

        ActualizarResultadosDerrota();

        if (panelDerrota != null)
            panelDerrota.SetActive(true);
    }

    public void ReiniciarZona()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}
