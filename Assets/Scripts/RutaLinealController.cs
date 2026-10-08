using UnityEngine;

public class RutaLinealController : MonoBehaviour
{
    [Header("Bloqueos de progreso")]
    public GameObject bloqueoPasoZona2;
    public GameObject bloqueoPasoZona3;
    [Header("Entrada y checkpoints seguros")]
    public Collider2D limiteEntradaZona1;
    public Transform puntoSeguroZona1;
    public Transform puntoSeguroZona2;
    public Transform puntoSeguroZona3;

    public void ZonaCompletada(int indiceZona)
    {
        if (indiceZona == 1 && bloqueoPasoZona2 != null)
        {
            if (limiteEntradaZona1 != null) limiteEntradaZona1.enabled = false;
            bloqueoPasoZona2.SetActive(false);
        }
        else if (indiceZona == 2 && bloqueoPasoZona3 != null)
        {
            if (bloqueoPasoZona2 != null) bloqueoPasoZona2.SetActive(false);
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

    public bool IntentarObtenerPuntoSeguro(int indiceZona, out Vector3 posicion)
    {
        Transform punto = indiceZona == 1 ? puntoSeguroZona1 :
            indiceZona == 2 ? puntoSeguroZona2 : indiceZona == 3 ? puntoSeguroZona3 : null;
        posicion = punto != null ? punto.position : Vector3.zero;
        return punto != null;
    }

    public void CerrarZonaAlIniciar(int indiceZona)
    {
        if (indiceZona == 1 && limiteEntradaZona1 != null)
        {
            limiteEntradaZona1.enabled = true;
            limiteEntradaZona1.isTrigger = false;
        }
        else if (indiceZona == 2)
        {
            if (limiteEntradaZona1 != null) { limiteEntradaZona1.enabled = true; limiteEntradaZona1.isTrigger = false; }
            CerrarPasoAnterior(1);
        }
        else if (indiceZona == 3)
        {
            if (limiteEntradaZona1 != null) { limiteEntradaZona1.enabled = true; limiteEntradaZona1.isTrigger = false; }
            if (bloqueoPasoZona2 != null) bloqueoPasoZona2.SetActive(false);
            CerrarPasoAnterior(2);
        }
    }

    public void RestaurarBloqueosDeZona(int indiceZona)
    {
        if (limiteEntradaZona1 != null)
        {
            limiteEntradaZona1.enabled = true;
            limiteEntradaZona1.isTrigger = false;
        }
        if (bloqueoPasoZona2 != null)
            bloqueoPasoZona2.SetActive(indiceZona != 3);
        if (bloqueoPasoZona3 != null)
            bloqueoPasoZona3.SetActive(true);
    }

    public void PrepararNuevaPartida()
    {
        if (limiteEntradaZona1 != null)
        {
            limiteEntradaZona1.enabled = true;
            limiteEntradaZona1.isTrigger = true;
        }
        if (bloqueoPasoZona2 != null) bloqueoPasoZona2.SetActive(true);
        if (bloqueoPasoZona3 != null) bloqueoPasoZona3.SetActive(true);
    }
}
