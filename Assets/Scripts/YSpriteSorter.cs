using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSpriteSorter : MonoBehaviour
{
    [Tooltip("Todos los sprites ordenados por Y deben usar la misma Sorting Layer.")]
    public int offset = 0;

    [Tooltip("Desactivar en decoracion estatica. El orden se calculara al habilitar el objeto.")]
    public bool actualizarMientrasSeMueve = true;

    [Min(1)] public int precision = 100;

    private SpriteRenderer spriteRenderer;
    private float ultimaY = float.NaN;
    private int ultimoOrden = int.MinValue;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        ActualizarOrden(true);
    }

    private void LateUpdate()
    {
        if (actualizarMientrasSeMueve)
            ActualizarOrden(false);
    }

    private void ActualizarOrden(bool forzar)
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        float posicionY = transform.position.y;

        if (!forzar && Mathf.Approximately(posicionY, ultimaY))
            return;

        int nuevoOrden = -Mathf.RoundToInt(posicionY * precision) + offset;

        if (forzar || nuevoOrden != ultimoOrden)
        {
            spriteRenderer.sortingOrder = nuevoOrden;
            ultimoOrden = nuevoOrden;
        }

        ultimaY = posicionY;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        precision = Mathf.Max(1, precision);

        if (!Application.isPlaying)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            ActualizarOrden(true);
        }
    }
#endif
}
