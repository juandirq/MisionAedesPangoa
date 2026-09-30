using System.Collections;
using UnityEngine;

public class ObjetoResueltoVisual : MonoBehaviour
{
    public ObjetoInteractuable interactuable;
    public SpriteRenderer spriteObjetivo;
    public GameObject iconoResuelto;
    [Min(0.01f)] public float duracion = 0.4f;
    [Range(1f, 1.3f)] public float escalaMaxima = 1.1f;
    public bool cambiarColor = true;
    public Color colorResuelto = new Color(0.75f, 0.9f, 0.75f, 1f);
    private Vector3 escalaOriginal;
    private Color colorOriginal;
    private bool inicializado;
    private bool animarEscala;
    private Coroutine rutina;

    private void Awake()
    {
        if (interactuable == null) interactuable = GetComponent<ObjetoInteractuable>();
        if (spriteObjetivo == null) spriteObjetivo = GetComponent<SpriteRenderer>();
        if (spriteObjetivo == null) return;
        escalaOriginal = spriteObjetivo.transform.localScale;
        colorOriginal = spriteObjetivo.color;
        inicializado = true;
        // Escalar un Transform con colliders también cambia su geometría física.
        // El punch solo se aplica a un sprite visual sin colliders en su rama.
        animarEscala = spriteObjetivo.GetComponentInChildren<Collider2D>(true) == null;
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
        if (!isActiveAndEnabled || !inicializado) return;
        if (rutina != null) StopCoroutine(rutina);
        rutina = StartCoroutine(Transicion());
    }

    private IEnumerator Transicion()
    {
        if (IconoSeguro()) iconoResuelto.SetActive(true);
        float tiempo = Mathf.Max(0.01f, duracion);
        for (float t = 0f; t < tiempo; t += Time.unscaledDeltaTime)
        {
            if (spriteObjetivo == null) yield break;
            float p = t / tiempo;
            if (animarEscala)
                spriteObjetivo.transform.localScale = escalaOriginal *
                    (1f + (escalaMaxima - 1f) * Mathf.Sin(p * Mathf.PI));
            if (cambiarColor) spriteObjetivo.color = Color.Lerp(colorOriginal, colorResuelto, p);
            yield return null;
        }
        AplicarFinal();
        rutina = null;
    }

    private void AplicarFinal()
    {
        if (spriteObjetivo == null) return;
        if (animarEscala) spriteObjetivo.transform.localScale = escalaOriginal;
        if (cambiarColor) spriteObjetivo.color = colorResuelto;
    }

    private void OnDisable()
    {
        if (interactuable != null) interactuable.AlResolverse -= Animar;
        if (rutina != null) StopCoroutine(rutina);
        rutina = null;
        if (inicializado && spriteObjetivo != null)
        {
            if (animarEscala) spriteObjetivo.transform.localScale = escalaOriginal;
            if (interactuable != null && interactuable.Resuelto) AplicarFinal();
        }
    }

    private bool IconoSeguro()
    {
        return iconoResuelto != null && !transform.IsChildOf(iconoResuelto.transform) &&
            (spriteObjetivo == null || !spriteObjetivo.transform.IsChildOf(iconoResuelto.transform));
    }
}
