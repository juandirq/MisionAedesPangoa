using UnityEngine;
using TMPro;
using System.Collections;

public class ObjetoInteractuable : MonoBehaviour
{
    public GameObject textoInteraccion;
    public GameObject ventanaDecision;
    public PlayerMovement playerMovement;
    public TextMeshProUGUI textoFeedback;
    public GameManager gameManager;

    private bool jugadorCerca = false;
    private bool inspeccionando = false;
    private bool resuelto = false;

    void Update()
    {
        if (jugadorCerca && !inspeccionando && !resuelto &&
            Input.GetKeyDown(KeyCode.E))
        {
            AbrirInspeccion();
        }

        if (inspeccionando && Input.GetKeyDown(KeyCode.Escape))
        {
            CerrarInspeccion();
        }
    }

    void AbrirInspeccion()
    {
        inspeccionando = true;

        textoInteraccion.SetActive(false);
        ventanaDecision.SetActive(true);
        textoFeedback.gameObject.SetActive(false);

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
    }

    public void RespuestaVaciar()
    {
        if (resuelto)
            return;

        resuelto = true;

        textoFeedback.text =
            "¡Correcto! Eliminaste el agua acumulada.";

        textoFeedback.color = Color.green;
        textoFeedback.gameObject.SetActive(true);

        if (gameManager != null)
        {
            gameManager.RegistrarAcierto(20);
        }

        StartCoroutine(CerrarDespuesDeAcierto());
    }

    public void RespuestaTapar()
    {
        textoFeedback.text =
            "Incorrecto. Primero debes eliminar el agua acumulada.";

        textoFeedback.color = Color.red;
        textoFeedback.gameObject.SetActive(true);

        if (gameManager != null)
        {
            gameManager.RegistrarError();
        }
    }

    public void RespuestaNada()
    {
        textoFeedback.text =
            "Incorrecto. Dejar el recipiente así mantiene el riesgo.";

        textoFeedback.color = Color.red;
        textoFeedback.gameObject.SetActive(true);

        if (gameManager != null)
        {
            gameManager.RegistrarError();
        }
    }

    IEnumerator CerrarDespuesDeAcierto()
    {
        yield return new WaitForSeconds(1.5f);

        CerrarInspeccion();
    }

    public void CerrarInspeccion()
    {
        inspeccionando = false;

        ventanaDecision.SetActive(false);

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

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

            if (!inspeccionando && !resuelto)
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