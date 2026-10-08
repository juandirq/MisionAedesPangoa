using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    private bool respuestaCorrectaProcesada = false;

    // COLORES DEL FEEDBACK
    private Color colorNormal = new Color32(85, 98, 74, 255);     // #55624A
    private Color colorCorrecto = new Color32(47, 143, 58, 255);  // #2F8F3A
    private Color colorIncorrecto = new Color32(201, 74, 67, 255);// #C94A43

    void Update()
    {
        if (ventanaDecision != null &&
            ventanaDecision.activeSelf &&
            !respuestaCorrectaProcesada &&
            Input.GetKeyDown(KeyCode.Escape))
        {
            CerrarVentana();
        }
    }

    public void AbrirVentana(ObjetoInteractuable objeto)
    {
        if (objeto == null || !objeto.PuedeResponder || Time.timeScale <= 0f ||
            (ventanaDecision != null && ventanaDecision.activeInHierarchy) ||
            (playerMovement != null && !playerMovement.enabled))
            return;
        if (ventanaDecision == null || tituloObjeto == null || descripcionObjeto == null ||
            textoFeedback == null || textoBoton1 == null || textoBoton2 == null || textoBoton3 == null)
        {
            Debug.LogWarning("Faltan referencias de VentanaDecisionUI.", this);
            return;
        }
        objetoActual = objeto;
        respuestaCorrectaProcesada = false;

        if (objetoActual == null)
            return;

        tituloObjeto.text = objeto.tituloObjeto;
        descripcionObjeto.text = objeto.descripcionObjeto;

        textoBoton1.text = objeto.opcion1;
        textoBoton2.text = objeto.opcion2;
        textoBoton3.text = objeto.opcion3;

        AjustarLayoutOpciones();

        // Mensaje inicial
        textoFeedback.text = "Selecciona una opción";
        textoFeedback.color = colorNormal;
        textoFeedback.gameObject.SetActive(true);

        ventanaDecision.SetActive(true);
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        if (gameManager != null)
            gameManager.PausarCronometro();

        if (playerMovement != null)
            playerMovement.enabled = false;
    }

    public void AjustarLayoutOpciones()
    {
        TextMeshProUGUI[] textos = { textoBoton1, textoBoton2, textoBoton3 };
        RectTransform[] botones = new RectTransform[textos.Length];
        float[] alturas = new float[textos.Length];
        const float espacio = 8f;
        const float limiteSuperior = 100f;
        const float limiteInferior = -155f;

        Canvas.ForceUpdateCanvases();
        float alturaTotal = espacio * (textos.Length - 1);
        for (int i = 0; i < textos.Length; i++)
        {
            if (textos[i] == null) return;
            Button boton = textos[i].GetComponentInParent<Button>(true);
            if (boton == null) return;
            botones[i] = boton.transform as RectTransform;
            textos[i].ForceMeshUpdate();
            alturas[i] = Mathf.Clamp(textos[i].preferredHeight + 20f, 54f, 78f);
            alturaTotal += alturas[i];
        }

        float cursor = (limiteSuperior + limiteInferior + alturaTotal) * 0.5f;
        for (int i = 0; i < botones.Length; i++)
        {
            botones[i].anchorMin = botones[i].anchorMax = new Vector2(0.5f, 0.5f);
            botones[i].sizeDelta = new Vector2(600f, alturas[i]);
            botones[i].anchoredPosition = new Vector2(0f, cursor - alturas[i] * 0.5f);
            cursor -= alturas[i] + espacio;
        }
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
        if (objetoActual == null || respuestaCorrectaProcesada || !objetoActual.PuedeResponder)
            return;

        bool correcto =
            objetoActual.ProcesarRespuesta(opcionElegida);

        if (correcto)
        {
            AudioManager.Instancia?.ReproducirCorrecto();
            respuestaCorrectaProcesada = true;

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
            AudioManager.Instancia?.ReproducirIncorrecto();
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
        respuestaCorrectaProcesada = false;

        if (gameManager != null)
        {
            gameManager.MostrarVictoriaPendiente();
            gameManager.ReanudarCronometro();
        }
    }
}
