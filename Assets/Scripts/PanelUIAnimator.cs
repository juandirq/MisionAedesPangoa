using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class PanelUIAnimator : MonoBehaviour
{
    [Min(0f)] public float duracion = 0.25f;
    public bool usarEscala = true;
    [Range(0.5f, 1f)] public float escalaInicial = 0.95f;

    private CanvasGroup canvasGroup;
    private Vector3 escalaFinal;
    private Coroutine animacionActual;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        escalaFinal = transform.localScale;
    }

    private void OnEnable()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        transform.localScale = usarEscala
            ? escalaFinal * escalaInicial
            : escalaFinal;

        if (animacionActual != null)
            StopCoroutine(animacionActual);

        animacionActual = StartCoroutine(AnimarEntrada());
    }

    private IEnumerator AnimarEntrada()
    {
        if (duracion <= 0f)
        {
            canvasGroup.alpha = 1f;
            transform.localScale = escalaFinal;
            animacionActual = null;
            yield break;
        }

        float transcurrido = 0f;
        Vector3 escalaDesde = transform.localScale;

        while (transcurrido < duracion)
        {
            transcurrido += Time.unscaledDeltaTime;
            float progreso = Mathf.Clamp01(transcurrido / duracion);

            canvasGroup.alpha = progreso;

            if (usarEscala)
                transform.localScale = Vector3.Lerp(escalaDesde, escalaFinal, progreso);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        transform.localScale = escalaFinal;
        animacionActual = null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        duracion = Mathf.Max(0f, duracion);
        escalaInicial = Mathf.Clamp(escalaInicial, 0.5f, 1f);
    }
#endif
}
