using UnityEngine;

[DisallowMultipleComponent]
public class ProgresoZonas : MonoBehaviour
{
    private readonly bool[] completadas = new bool[3];

    public bool EstaZonaCompletada(int indiceZona)
    {
        return indiceZona >= 1 && indiceZona <= 3 && completadas[indiceZona - 1];
    }

    public bool EstaZonaDesbloqueada(int indiceZona)
    {
        return indiceZona >= 1 && indiceZona <= 3 &&
               (indiceZona == 1 || EstaZonaCompletada(indiceZona - 1));
    }

    public void CompletarZona(int indiceZona)
    {
        if (!EstaZonaDesbloqueada(indiceZona))
            return;
        completadas[indiceZona - 1] = true;
    }

    public bool TodasCompletadas => EstaZonaCompletada(3);
}
