using UnityEngine;
using TMPro;

public class NPCDialogo : MonoBehaviour
{
    public GameObject textoHablar;
    public GameObject panelDialogo;
    public TextMeshProUGUI nombreNPC;
    public TextMeshProUGUI textoDialogo;
    public PlayerMovement playerMovement;

    [Header("Datos del NPC")]
    public string nombre = "Docente";

    [TextArea(2, 5)]
    public string[] dialogos;

    private bool jugadorCerca = false;
    private bool hablando = false;
    private int dialogoActual = 0;

    void Update()
    {
        if (jugadorCerca && !hablando && Input.GetKeyDown(KeyCode.E))
        {
            AbrirDialogo();
        }
    }

    void AbrirDialogo()
    {
        hablando = true;
        dialogoActual = 0;

        textoHablar.SetActive(false);
        panelDialogo.SetActive(true);

        nombreNPC.text = nombre;
        textoDialogo.text = dialogos[dialogoActual];

        if (playerMovement != null)
            playerMovement.enabled = false;
    }

    public void SiguienteDialogo()
    {
        dialogoActual++;

        if (dialogoActual < dialogos.Length)
        {
            textoDialogo.text = dialogos[dialogoActual];
        }
        else
        {
            CerrarDialogo();
        }
    }

    public void CerrarDialogo()
    {
        hablando = false;
        panelDialogo.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (jugadorCerca)
            textoHablar.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;

            if (!hablando)
                textoHablar.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            textoHablar.SetActive(false);
        }
    }
}