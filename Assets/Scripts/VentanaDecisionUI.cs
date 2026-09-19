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

        textoFeedback.gameObject.SetActive(false);

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

            textoFeedback.color = Color.green;
            textoFeedback.gameObject.SetActive(true);

            Invoke(nameof(CerrarVentana), 1.5f);
        }
        else
        {
            textoFeedback.text =
                objetoActual.feedbackIncorrecto;

            textoFeedback.color = Color.red;
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