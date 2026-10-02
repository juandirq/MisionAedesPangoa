using UnityEngine;

public class CheckpointCierreRuta : MonoBehaviour
{
    public RutaLinealController rutaLineal;

    [Tooltip("1 = cerrar paso de Zona 1, 2 = cerrar paso de Zona 2")]
    public int zonaQueDejaAtras = 1;

    private bool usado = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (usado)
            return;

        if (!other.CompareTag("Player"))
            return;

        usado = true;

        if (rutaLineal != null)
        {
            rutaLineal.CerrarPasoAnterior(zonaQueDejaAtras);
        }

        gameObject.SetActive(false);
    }
}