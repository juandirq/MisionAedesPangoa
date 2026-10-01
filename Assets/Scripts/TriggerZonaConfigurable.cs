using UnityEngine;

public class TriggerZonaConfigurable : MonoBehaviour
{
    [SerializeField] private ZonaConfigurable zona;
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private PlayerMovement playerMovement;

    private bool activado;
    private bool esperandoInicio;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activado || esperandoInicio || zona == null ||
            !other.CompareTag("Player"))
            return;

        if (panelInicio == null)
        {
            if (zona.IntentarComenzarDesdeTrigger())
                activado = true;
            return;
        }

        if (playerMovement == null)
            playerMovement = other.GetComponentInParent<PlayerMovement>();

        if (playerMovement == null)
        {
            Debug.LogWarning("El trigger necesita PlayerMovement para mostrar el panel de inicio.", this);
            return;
        }

        esperandoInicio = true;
        playerMovement.enabled = false;
        panelInicio.SetActive(true);
    }

    public void ComenzarDesdePanel()
    {
        if (activado || !esperandoInicio || zona == null)
            return;

        if (panelInicio != null)
            panelInicio.SetActive(false);

        if (zona.IntentarComenzarDesdeTrigger())
        {
            activado = true;
            esperandoInicio = false;

            if (playerMovement != null)
                playerMovement.enabled = true;
        }
        else if (panelInicio != null)
        {
            panelInicio.SetActive(true);
        }
    }
}
