using UnityEngine;

public class TriggerZona2 : MonoBehaviour
{
    public GameObject panelInicioZona2;
    public ProgresoZonas progresoZonas;
    public GameManager gameManager;
    [Range(1, 3)] public int indiceZona = 2;

    private void Start()
    {
        if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
        if (progresoZonas == null && gameManager != null)
            progresoZonas = gameManager.progresoZonas;
    }

    private bool yaSeActivo = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (yaSeActivo)
            return;

        if (other.CompareTag("Player"))
        {
            if (progresoZonas != null && !progresoZonas.EstaZonaDesbloqueada(indiceZona))
            {
                Debug.Log("Completa primero la zona anterior.", this);
                return;
            }
            if (panelInicioZona2 == null || Time.timeScale <= 0f ||
                (gameManager != null && (!gameManager.PuedeIniciarZona(indiceZona) ||
                 (gameManager.playerMovement != null && !gameManager.playerMovement.enabled)))) return;
            yaSeActivo = true;

            panelInicioZona2.SetActive(true);

            // Pausa al jugador mientras lee la misión
            Time.timeScale = 0f;
        }
    }
}
