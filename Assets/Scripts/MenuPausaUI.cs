using UnityEngine;

// Lee ESC antes que las ventanas: cerrar decisión nunca abre pausa ese mismo frame.
[DefaultExecutionOrder(-1000)]
public class MenuPausaUI : MonoBehaviour
{
    public GameObject panelPausa;
    public GameManager gameManager;
    public PlayerMovement playerMovement;
    public GameObject[] otrosPanelesModales;
    private NPCDialogo[] dialogos;
    private bool pausado;
    private bool reanudarReloj;
    private bool movimientoAnterior;
    private float escalaAnterior;

    private void Start()
    {
        if (panelPausa != null && transform.IsChildOf(panelPausa.transform))
        {
            Debug.LogWarning("Coloca MenuPausaUI fuera de PanelPausa, en un objeto siempre activo.", this);
            enabled = false;
            return;
        }
        dialogos = FindObjectsByType<NPCDialogo>(FindObjectsInactive.Include);
        if (panelPausa != null && panelPausa != gameObject) panelPausa.SetActive(false);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (pausado) { Reanudar(); return; }
        if (panelPausa == null || panelPausa == gameObject || gameManager == null ||
            playerMovement == null || !playerMovement.enabled || gameManager.JuegoTerminado ||
            Time.timeScale <= 0f) return;
        if (gameManager.ventanaDecision != null && gameManager.ventanaDecision.activeInHierarchy) return;
        if (dialogos != null)
            foreach (NPCDialogo npc in dialogos)
                if (npc != null && (npc.Hablando ||
                    (npc.panelDialogo != null && npc.panelDialogo.activeInHierarchy))) return;
        if (otrosPanelesModales != null)
            foreach (GameObject panel in otrosPanelesModales)
                if (panel != null && panel.activeInHierarchy) return;
        AbrirPausa();
    }

    private void AbrirPausa()
    {
        pausado = true;
        escalaAnterior = Time.timeScale;
        movimientoAnterior = playerMovement.enabled;
        reanudarReloj = gameManager.CronometroEnMarcha;
        gameManager.PausarCronometro();
        playerMovement.enabled = false;
        Time.timeScale = 0f;
        panelPausa.SetActive(true);
    }

    public void Reanudar()
    {
        if (!pausado) return;
        pausado = false;
        Time.timeScale = escalaAnterior;
        if (panelPausa != null) panelPausa.SetActive(false);
        if (gameManager != null && !gameManager.JuegoTerminado)
        {
            if (playerMovement != null) playerMovement.enabled = movimientoAnterior;
            if (reanudarReloj) gameManager.ReanudarCronometro();
        }
    }

    public void ReiniciarZona()
    {
        Reanudar();
        if (gameManager != null) gameManager.ReiniciarZona();
    }

    public void SalirDelJuego() { Reanudar(); Application.Quit(); }
    private void OnDisable() { Reanudar(); }
}
