using UnityEngine;

public class InicioZona2UI : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject panelInicioZona2;
    public GameManager gameManager;
    [Range(1, 3)] public int indiceZona = 2;
    public ZonaBannerUI banner;
    public string subtituloBanner = "Área residencial";

    [Header("Configuración Zona 2")]
    public string nombreZona = "Zona 2";
    public int objetivosZona = 6;
    public float tiempoZona = 105f;

    public void ComenzarInspeccion()
    {
        if (gameManager == null || !gameManager.PuedeIniciarZona(indiceZona) ||
            objetivosZona <= 0 || tiempoZona <= 0f || float.IsNaN(tiempoZona) ||
            float.IsInfinity(tiempoZona)) return;
        if (panelInicioZona2 != null) panelInicioZona2.SetActive(false);

        Time.timeScale = 1f;

        gameManager.IniciarZona(
            objetivosZona,
            tiempoZona,
            nombreZona,
            indiceZona
        );

        if (gameManager.ZonaActiva && banner != null) banner.Mostrar(nombreZona, subtituloBanner);
        Debug.Log("Zona 2 iniciada");
    }
}
