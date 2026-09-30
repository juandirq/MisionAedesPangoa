using UnityEngine;
using TMPro;

public class NPCDialogo : MonoBehaviour
{
    public GameObject textoHablar;
    public GameObject panelDialogo;
    public TextMeshProUGUI nombreNPC;
    public TextMeshProUGUI textoDialogo;
    public PlayerMovement playerMovement;
    public GameManager gameManager;

    [Header("Datos del NPC")]
    public string nombre = "Docente";

    [TextArea(2, 5)]
    public string[] dialogos;

    private bool jugadorCerca = false;
    private bool hablando = false;
    private int dialogoActual = 0;
    public bool Hablando => hablando;

    void Update()
    {
        if (jugadorCerca && !hablando && Time.timeScale > 0f &&
            (playerMovement == null || playerMovement.enabled) && Input.GetKeyDown(KeyCode.E))
        {
            AbrirDialogo();
        }
    }

    void AbrirDialogo()
    {
        if (dialogos == null || dialogos.Length == 0)
        {
            Debug.LogWarning("El NPC no tiene dialogos configurados.", this);
            return;
        }
        if (panelDialogo == null || nombreNPC == null || textoDialogo == null)
        {
            Debug.LogWarning("Faltan referencias del diálogo.", this);
            return;
        }

        hablando = true;
        dialogoActual = 0;

        if (textoHablar != null) textoHablar.SetActive(false);
        panelDialogo.SetActive(true);

        nombreNPC.text = nombre;
        textoDialogo.text = dialogos[dialogoActual];

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (gameManager == null)
            gameManager = FindAnyObjectByType<GameManager>();

        if (gameManager != null)
            gameManager.PausarCronometro();
    }

    public void SiguienteDialogo()
    {
        if (!hablando || dialogos == null || textoDialogo == null) return;
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
        if (!hablando) return;
        hablando = false;
        if (panelDialogo != null) panelDialogo.SetActive(false);

        if (playerMovement != null && (gameManager == null || !gameManager.JuegoTerminado))
            playerMovement.enabled = true;

        if (gameManager != null)
            gameManager.ReanudarCronometro();

        if (jugadorCerca && textoHablar != null)
            textoHablar.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;

            if (!hablando && textoHablar != null)
                textoHablar.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            if (textoHablar != null) textoHablar.SetActive(false);
        }
    }
}
