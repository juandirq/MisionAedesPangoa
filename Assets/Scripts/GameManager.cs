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

    [Header("Jugador")]
    public PlayerMovement playerMovement;

    [Header("Configuración de la zona")]
    public float tiempoRestante = 120f;
    public int objetivosTotales = 4;

    private int objetivosCompletados = 0;
    private int riesgo = 100;
    private int aciertos = 0;
    private int errores = 0;

    private bool juegoTerminado = false;

    // Permite que otros scripts sepan si la partida terminó
    public bool JuegoTerminado => juegoTerminado;

    void Start()
    {
        ActualizarHUD();

        if (panelDerrota != null)
            panelDerrota.SetActive(false);

        if (panelVictoria != null)
            panelVictoria.SetActive(false);
    }

    void Update()
    {
        if (juegoTerminado)
            return;

        tiempoRestante -= Time.deltaTime;

        if (tiempoRestante <= 0)
        {
            tiempoRestante = 0;
            ActualizarTiempo();
            Derrota();
            return;
        }

        ActualizarTiempo();
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
            Derrota();
        }
    }

    void ComprobarVictoria()
    {
        if (objetivosCompletados >= objetivosTotales && riesgo <= 20)
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
        int minutos = Mathf.FloorToInt(tiempoRestante / 60);
        int segundos = Mathf.FloorToInt(tiempoRestante % 60);

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

        if (ventanaDecision != null)
            ventanaDecision.SetActive(false);

        if (textoInteraccion != null)
            textoInteraccion.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = false;

        // Mostrar resultados reales
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

    void Derrota()
    {
        if (juegoTerminado)
            return;

        juegoTerminado = true;

        if (ventanaDecision != null)
            ventanaDecision.SetActive(false);

        if (textoInteraccion != null)
            textoInteraccion.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (panelDerrota != null)
            panelDerrota.SetActive(true);
    }

    public void ReiniciarZona()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}