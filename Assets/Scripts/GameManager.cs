using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI textoRiesgo;
    public TextMeshProUGUI textoAciertos;
    public TextMeshProUGUI textoErrores;
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoObjetivos;

    public GameObject panelDerrota;
    public GameObject panelVictoria;
    public GameObject ventanaDecision;
    public GameObject textoInteraccion;

    public PlayerMovement playerMovement;

    public float tiempoRestante = 120f;

    public int objetivosTotales = 4;

    private int objetivosCompletados = 0;
    private int riesgo = 100;
    private int aciertos = 0;
    private int errores = 0;

    private bool juegoTerminado = false;

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
        textoRiesgo.text = "Riesgo: " + riesgo + "%";
        textoAciertos.text = "Aciertos: " + aciertos;
        textoErrores.text = "Errores: " + errores;

        textoObjetivos.text =
            "Objetivos: " + objetivosCompletados + "/" + objetivosTotales;

        ActualizarTiempo();
    }

    void ActualizarTiempo()
    {
        int minutos = Mathf.FloorToInt(tiempoRestante / 60);
        int segundos = Mathf.FloorToInt(tiempoRestante % 60);

        textoTiempo.text =
            "Tiempo: " +
            minutos.ToString("00") +
            ":" +
            segundos.ToString("00");
    }

    void Victoria()
    {
        juegoTerminado = true;

        if (ventanaDecision != null)
            ventanaDecision.SetActive(false);

        if (textoInteraccion != null)
            textoInteraccion.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = false;

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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}