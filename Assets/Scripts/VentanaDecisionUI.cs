using UnityEngine;
using TMPro;

public class VentanaDecisionUI : MonoBehaviour
{
    public GameObject ventanaDecision;

    public TextMeshProUGUI tituloObjeto;
    public TextMeshProUGUI descripcionObjeto;
    public TextMeshProUGUI textoFeedback;

    public TextMeshProUGUI textoBoton1;
    public TextMeshProUGUI textoBoton2;
    public TextMeshProUGUI textoBoton3;

    public PlayerMovement playerMovement;

    private ObjetoInteractuable objetoActual;

    void Update()
    {
        if (ventanaDecision.activeSelf &&
            Input.GetKeyDown(KeyCode.Escape))
        {
            CerrarVentana();
        }
    }

    public void AbrirVentana(ObjetoInteractuable objeto)
    {
        objetoActual = objeto;

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

        ventanaDecision.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (objetoActual != null)
        {
            objetoActual.AlCerrarVentana();
        }

        objetoActual = null;
    }
}