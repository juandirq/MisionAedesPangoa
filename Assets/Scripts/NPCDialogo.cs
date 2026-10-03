using System.Collections;
using UnityEngine;
using TMPro;

public class NPCDialogo : MonoBehaviour
{
    [Header("Referencias")]
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

    [Header("Efecto de texto")]
    [SerializeField] private float velocidadTexto = 0.025f;

    private bool jugadorCerca = false;
    private bool hablando = false;
    private bool escribiendo = false;

    private int dialogoActual = 0;
    private Coroutine escrituraActual;

    public bool Hablando => hablando;

    void Update()
    {
        if (jugadorCerca &&
            !hablando &&
            Time.timeScale > 0f &&
            (playerMovement == null || playerMovement.enabled) &&
            Input.GetKeyDown(KeyCode.E))
        {
            AbrirDialogo();
        }
    }

    void AbrirDialogo()
    {
        if (dialogos == null || dialogos.Length == 0)
        {
            Debug.LogWarning("El NPC no tiene diálogos configurados.", this);
            return;
        }

        if (panelDialogo == null || nombreNPC == null || textoDialogo == null)
        {
            Debug.LogWarning("Faltan referencias del diálogo.", this);
            return;
        }

        hablando = true;
        dialogoActual = 0;

        if (textoHablar != null)
            textoHablar.SetActive(false);

        panelDialogo.SetActive(true);

        nombreNPC.text = nombre;

        MostrarDialogoActual();

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (gameManager == null)
            gameManager = FindAnyObjectByType<GameManager>();

        if (gameManager != null)
            gameManager.PausarCronometro();
    }

    void MostrarDialogoActual()
    {
        if (escrituraActual != null)
            StopCoroutine(escrituraActual);

        escrituraActual = StartCoroutine(
            EscribirTexto(dialogos[dialogoActual])
        );
    }

    IEnumerator EscribirTexto(string mensaje)
    {
        escribiendo = true;
        textoDialogo.text = "";
        int caracteresAudibles = 0;

        foreach (char letra in mensaje)
        {
            textoDialogo.text += letra;

            if (char.IsLetterOrDigit(letra))
            {
                caracteresAudibles++;
                if ((caracteresAudibles - 1) % 3 == 0)
                    AudioManager.Instancia?.ReproducirBlipDialogo();
            }

            yield return new WaitForSecondsRealtime(
                velocidadTexto
            );
        }

        escribiendo = false;
        escrituraActual = null;
    }

    public void SiguienteDialogo()
    {
        if (!hablando ||
            dialogos == null ||
            textoDialogo == null)
        {
            return;
        }

        // Si el texto todavía se está escribiendo,
        // el primer clic lo completa inmediatamente.
        if (escribiendo)
        {
            if (escrituraActual != null)
            {
                StopCoroutine(escrituraActual);
                escrituraActual = null;
            }

            textoDialogo.text = dialogos[dialogoActual];
            escribiendo = false;

            return;
        }

        // Si ya terminó de escribir,
        // avanzar al siguiente mensaje.
        dialogoActual++;

        if (dialogoActual < dialogos.Length)
        {
            MostrarDialogoActual();
        }
        else
        {
            CerrarDialogo();
        }
    }

    public void CerrarDialogo()
    {
        if (!hablando)
            return;

        hablando = false;
        escribiendo = false;

        if (escrituraActual != null)
        {
            StopCoroutine(escrituraActual);
            escrituraActual = null;
        }

        if (panelDialogo != null)
            panelDialogo.SetActive(false);

        if (playerMovement != null &&
            (gameManager == null || !gameManager.JuegoTerminado))
        {
            playerMovement.enabled = true;
        }

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

            if (textoHablar != null)
                textoHablar.SetActive(false);
        }
    }
}
