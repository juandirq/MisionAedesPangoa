using System.Collections;
using UnityEngine;

public class ObjetoResueltoVisual : MonoBehaviour
{
    public ObjetoInteractuable interactuable;
    public SpriteRenderer spriteObjetivo;
    public Sprite spriteResuelto;
    public GameObject iconoResuelto;
    [Min(0.01f)] public float duracionDesvanecerOriginal = 0.14f;
    [Min(0.01f)] public float duracionAparecerResuelto = 0.16f;

    private Color colorOriginal;
    private Sprite spriteOriginal;
    private bool inicializado;
    private bool transicionIniciada;
    private Coroutine rutina;

    private void Awake()
    {
        if (interactuable == null) interactuable = GetComponent<ObjetoInteractuable>();
        if (spriteObjetivo == null) spriteObjetivo = GetComponent<SpriteRenderer>();
        if (spriteObjetivo == null) return;
        colorOriginal = spriteObjetivo.color;
        spriteOriginal = spriteObjetivo.sprite;
        inicializado = true;
    }

    private void OnEnable()
    {
        if (interactuable == null || !inicializado) return;
        interactuable.AlResolverse += Animar;
        if (IconoSeguro())
            iconoResuelto.SetActive(interactuable.Resuelto);
        if (interactuable.Resuelto) AplicarFinal();
    }

    private void Animar()
    {
        if (!isActiveAndEnabled || !inicializado || transicionIniciada) return;
        transicionIniciada = true;
        rutina = StartCoroutine(Transicion());
    }

    private IEnumerator Transicion()
    {
        if (IconoSeguro()) iconoResuelto.SetActive(true);
        yield return CambiarAlfa(colorOriginal.a, 0f, duracionDesvanecerOriginal);
        if (spriteObjetivo == null) yield break;

        if (spriteResuelto != null) spriteObjetivo.sprite = spriteResuelto;
        yield return CambiarAlfa(0f, colorOriginal.a, duracionAparecerResuelto);

        AplicarFinal();
        rutina = null;
    }

    private IEnumerator CambiarAlfa(float desde, float hasta, float duracion)
    {
        float tiempo = Mathf.Max(0.01f, duracion);
        for (float t = 0f; t < tiempo; t += Time.unscaledDeltaTime)
        {
            if (spriteObjetivo == null) yield break;
            Color color = colorOriginal;
            color.a = Mathf.Lerp(desde, hasta, t / tiempo);
            spriteObjetivo.color = color;
            yield return null;
        }

        if (spriteObjetivo != null)
        {
            Color color = colorOriginal;
            color.a = hasta;
            spriteObjetivo.color = color;
        }
    }

    private void AplicarFinal()
    {
        if (spriteObjetivo == null) return;
        if (spriteResuelto != null) spriteObjetivo.sprite = spriteResuelto;
        spriteObjetivo.color = colorOriginal;
        transicionIniciada = true;
    }

    public void RestablecerParaReintento()
    {
        if (!inicializado || spriteObjetivo == null) return;
        if (rutina != null) StopCoroutine(rutina);
        rutina = null;
        transicionIniciada = false;
        spriteObjetivo.sprite = spriteOriginal;
        spriteObjetivo.color = colorOriginal;
        if (IconoSeguro()) iconoResuelto.SetActive(false);
    }

    private void OnDisable()
    {
        if (interactuable != null) interactuable.AlResolverse -= Animar;
        if (rutina != null) StopCoroutine(rutina);
        rutina = null;
        if (inicializado && spriteObjetivo != null && interactuable != null && interactuable.Resuelto)
            AplicarFinal();
    }

    private bool IconoSeguro()
    {
        return iconoResuelto != null && !transform.IsChildOf(iconoResuelto.transform) &&
            (spriteObjetivo == null || !spriteObjetivo.transform.IsChildOf(iconoResuelto.transform));
    }
}
