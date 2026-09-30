using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ZonaBannerUI : MonoBehaviour
{
    public TextMeshProUGUI titulo;
    public TextMeshProUGUI subtitulo;
    [Min(0.01f)] public float duracionFade = 0.3f;
    [Min(0f)] public float permanencia = 2f;
    private CanvasGroup grupo;
    private Coroutine rutina;

    private void Awake()
    {
        grupo = GetComponent<CanvasGroup>();
        grupo.alpha = 0f;
        grupo.blocksRaycasts = false;
        grupo.interactable = false;
    }

    public void Mostrar(string textoTitulo, string textoSubtitulo)
    {
        gameObject.SetActive(true);
        if (grupo == null) grupo = GetComponent<CanvasGroup>();
        if (titulo != null) titulo.text = textoTitulo;
        if (subtitulo != null) subtitulo.text = textoSubtitulo;
        if (rutina != null) StopCoroutine(rutina);
        rutina = StartCoroutine(Animar());
    }

    private IEnumerator Animar()
    {
        float fade = Mathf.Max(0.01f, duracionFade);
        float espera = Mathf.Max(0f, permanencia);
        float total = fade * 2f + espera;
        grupo.alpha = 0f;
        for (float t = 0f; t < total; t += Time.unscaledDeltaTime)
        {
            grupo.alpha = t < fade ? t / fade :
                t < fade + espera ? 1f : Mathf.Clamp01((total - t) / fade);
            yield return null;
        }
        grupo.alpha = 0f;
        rutina = null;
    }

    private void OnDisable()
    {
        if (rutina != null) StopCoroutine(rutina);
        rutina = null;
        if (grupo != null) grupo.alpha = 0f;
    }
}
