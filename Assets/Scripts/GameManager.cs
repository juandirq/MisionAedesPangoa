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

    [Header("Paneles")]
    public GameObject panelDerrota;
    public GameObject panelVictoria;
    public GameObject ventanaDecision;
    public GameObject textoInteraccion;

    [Header("Texto de derrota")]
    public TextMeshProUGUI textoMotivoDerrota;

    [Header("Jugador")]
    public PlayerMovement playerMovement;

    [Header("Configuración actual")]
    public float tiempoRestante = 0f;
    public int objetivosTotales = 0;

    private int objetivosCompletados = 0;
    private int riesgo = 100;
    private int aciertos = 0;
    private int errores = 0;

    private bool juegoTerminado = false;
    private bool cronometroActivo = false;

    public bool JuegoTerminado => juegoTerminado;

    void Start()
    {
        Time.timeScale = 1f;

        cronometroActivo = false;

        if (panelDerrota != null)
            panelDerrota.SetActive(false);

        if (panelVictoria != null)
            panelVictoria.SetActive(false);

        ActualizarHUD();
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
        objetivosTotales = cantidadObjetivos;
        tiempoRestante = tiempoZona;

        objetivosCompletados = 0;
        riesgo = 100;
        aciertos = 0;
        errores = 0;

        juegoTerminado = false;
        cronometroActivo = true;

        if (panelDerrota != null)
            panelDerrota.SetActive(false);

        if (panelVictoria != null)
            panelVictoria.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = true;

        ActualizarHUD();

        Debug.Log(
            "Zona iniciada. Objetivos: " +
            objetivosTotales +
            " | Tiempo: " +
            tiempoRestante
        );
    }

    public void RegistrarAcierto(int reduccionRiesgo)
    {
        if (juegoTerminado)
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
        if (juegoTerminado)
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

    void ActualizarTiempo()
    {
        int minutos =
            Mathf.FloorToInt(tiempoRestante / 60);

        int segundos =
            Mathf.FloorToInt(tiempoRestante % 60);

        if (textoTiempo != null)
        {
            textoTiempo.text =
                "Tiempo: " +
                minutos.ToString("00") +
                ":" +
                segundos.ToString("00");
        }
    }

    void Victoria()
    {
        if (juegoTerminado)
            return;

        juegoTerminado = true;
        cronometroActivo = false;

        if (ventanaDecision != null)
            ventanaDecision.SetActive(false);

        if (textoInteraccion != null)
            textoInteraccion.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (resultadoRiesgo != null)
            resultadoRiesgo.text =
                "Riesgo final: " + riesgo + "%";

        if (resultadoObjetivos != null)
        {
            resultadoObjetivos.text =
                "Objetivos: " +
                objetivosCompletados +
                "/" +
                objetivosTotales;
        }

        if (resultadoAciertos != null)
            resultadoAciertos.text =
                "Aciertos: " + aciertos;

        if (resultadoErrores != null)
            resultadoErrores.text =
                "Errores: " + errores;

        if (panelVictoria != null)
            panelVictoria.SetActive(true);
    }

    void Derrota(string motivo)
    {
        if (juegoTerminado)
            return;

        juegoTerminado = true;
        cronometroActivo = false;

        if (ventanaDecision != null)
            ventanaDecision.SetActive(false);

        if (textoInteraccion != null)
            textoInteraccion.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (textoMotivoDerrota != null)
            textoMotivoDerrota.text = motivo;

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