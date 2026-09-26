using UnityEngine;

public class InicioZona2UI : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject panelInicioZona2;
    public GameManager gameManager;

    [Header("Configuración Zona 2")]
    public int objetivosZona = 6;
    public float tiempoZona = 105f;

    public void ComenzarInspeccion()
    {
        panelInicioZona2.SetActive(false);

        Time.timeScale = 1f;

        gameManager.IniciarZona(
            objetivosZona,
            tiempoZona
        );

        Debug.Log("Zona 2 iniciada");
    }
}