using UnityEngine;

public class TriggerZona2 : MonoBehaviour
{
    public GameObject panelInicioZona2;

    private bool yaSeActivo = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (yaSeActivo)
            return;

        if (other.CompareTag("Player"))
        {
            yaSeActivo = true;

            panelInicioZona2.SetActive(true);

            // Pausa al jugador mientras lee la misión
            Time.timeScale = 0f;
        }
    }
}