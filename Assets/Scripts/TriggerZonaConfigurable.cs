using UnityEngine;

public class TriggerZonaConfigurable : MonoBehaviour
{
    [SerializeField] private ZonaConfigurable zona;

    private bool activado;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activado || zona == null || !other.CompareTag("Player"))
            return;

        if (zona.IntentarComenzarDesdeTrigger())
            activado = true;
    }
}
