using UnityEngine;
using TMPro;

public class VentanaDecisionUI : MonoBehaviour
{
    [Header("Referencias de UI")]
    public GameObject ventanaDecision;

    public TextMeshProUGUI tituloObjeto;
    public TextMeshProUGUI descripcionObjeto;
    public TextMeshProUGUI textoFeedback;

    public TextMeshProUGUI textoBoton1;
    public TextMeshProUGUI textoBoton2;
    public TextMeshProUGUI textoBoton3;

    [Header("Referencias del juego")]
    public PlayerMovement playerMovement;
    public GameManager gameManager;

    private ObjetoInteractuable objetoActual;

    // COLORES DEL FEEDBACK
    private Color colorNormal = new Color32(85, 98, 74, 255);     // #55624A
    private Color colorCorrecto = new Color32(47, 143, 58, 255);  // #2F8F3A
    private Color colorIncorrecto = new Color32(201, 74, 67, 255);// #C94A43

    void Update()
    {
        if (ventanaDecision != null &&
            ventanaDecision.activeSelf &&
            Input.GetKeyDown(KeyCode.Escape))
        {
            CerrarVentana();
        }
    }

    public void AbrirVentana(ObjetoInteractuable objeto)
    {
        objetoActual = objeto;

        if (objetoActual == null)
            return;

        tituloObjeto.text = objeto.tituloObjeto;
        descripcionObjeto.text = objeto.descripcionObjeto;

        textoBoton1.text = objeto.opcion1;
        textoBoton2.text = objeto.opcion2;
        textoBoton3.text = objeto.opcion3;

        // Mensaje inicial
        textoFeedback.text = "Selecciona una opción";
        textoFeedback.color = colorNormal;
        textoFeedback.gameObject.SetActive(true);

        ventanaDecision.SetActive(true);

        if (playerMovement != null)
            playerMovement.enabled = false;
    }

    public void Opcion1()
    {
        ProcesarOpcion(0);
    }

    public void Opcion2()
    {
        ProcesarOpcion(1);
    }

    public void Opcion3()
    {
        ProcesarOpcion(2);
    }

    void ProcesarOpcion(int opcionElegida)
    {
        if (objetoActual == null)
            return;

        bool correcto =
            objetoActual.ProcesarRespuesta(opcionElegida);

        if (correcto)
        {
            textoFeedback.text =
                objetoActual.feedbackCorrecto;

            // VERDE BONITO
            textoFeedback.color = colorCorrecto;
            textoFeedback.gameObject.SetActive(true);

            // Antes era 1.5 segundos.
            // Ahora tiene 4 segundos para leer.
            Invoke(nameof(CerrarVentana), 4f);
        }
        else
        {
            textoFeedback.text =
                objetoActual.feedbackIncorrecto;

            // ROJO MÁS SUAVE Y LEGIBLE
            textoFeedback.color = colorIncorrecto;
            textoFeedback.gameObject.SetActive(true);
        }
    }

    public void CerrarVentana()
    {
        CancelInvoke();

        if (ventanaDecision != null)
            ventanaDecision.SetActive(false);

        // Solo devuelve el movimiento si la partida NO terminó
        if (playerMovement != null)
        {
            if (gameManager == null || !gameManager.JuegoTerminado)
            {
                playerMovement.enabled = true;
            }
        }

        // Solo vuelve a mostrar la interacción si la partida NO terminó
        if (objetoActual != null)
        {
            if (gameManager == null || !gameManager.JuegoTerminado)
            {
                objetoActual.AlCerrarVentana();
            }
        }

        objetoActual = null;
    }
}