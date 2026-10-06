using UnityEngine;

public class RutaLinealController : MonoBehaviour
{
    [Header("Bloqueos de progreso")]
    public GameObject bloqueoPasoZona2;
    public GameObject bloqueoPasoZona3;

    public void ZonaCompletada(int indiceZona)
    {
        if (indiceZona == 1 && bloqueoPasoZona2 != null)
        {
            bloqueoPasoZona2.SetActive(false);
        }
        else if (indiceZona == 2 && bloqueoPasoZona3 != null)
        {
            bloqueoPasoZona3.SetActive(false);
        }
        else if (indiceZona == 3 && bloqueoPasoZona3 != null)
        {
            bloqueoPasoZona3.SetActive(false);
        }
    }

    public void CerrarPasoAnterior(int indiceZona)
    {
        if (indiceZona == 1 && bloqueoPasoZona2 != null)
        {
            bloqueoPasoZona2.SetActive(true);
        }
        else if (indiceZona == 2 && bloqueoPasoZona3 != null)
        {
            bloqueoPasoZona3.SetActive(true);
        }
    }
}