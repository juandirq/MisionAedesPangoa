using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class FinalGameUI : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public RectTransform titulo;
    public Button botonVolverMenu;
    public Image[] personajes;
    public Sprite[] framesPlayer;
    public Sprite[] framesDocente;
    public Sprite[] framesVecino;
    public Sprite[] framesPromotor;

    private RectTransform[] rectsPersonajes;
    private Vector2[] posicionesBase;
    private Coroutine animacion;
    private bool visible;

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (botonVolverMenu != null) botonVolverMenu.onClick.AddListener(VolverAlMenu);
    }

    public void Mostrar()
    {
        if (visible) return;
        visible = true;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        PrepararPersonajes();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;
        }
        if (titulo != null) titulo.localScale = Vector3.one * 0.9f;
        if (botonVolverMenu != null) botonVolverMenu.interactable = true;
        if (animacion != null) StopCoroutine(animacion);
        animacion = StartCoroutine(AnimarEntrada());
    }

    private void PrepararPersonajes()
    {
        if (personajes == null) return;
        rectsPersonajes = new RectTransform[personajes.Length];
        posicionesBase = new Vector2[personajes.Length];
        for (int i = 0; i < personajes.Length; i++)
        {
            if (personajes[i] == null) continue;
            rectsPersonajes[i] = personajes[i].rectTransform;
            posicionesBase[i] = rectsPersonajes[i].anchoredPosition;
            Color color = personajes[i].color;
            color.a = 0f;
            personajes[i].color = color;
        }
    }

    private IEnumerator AnimarEntrada()
    {
        const float duracion = 0.55f;
        float inicio = Time.unscaledTime;
        while (Time.unscaledTime - inicio < duracion)
        {
            float t = Mathf.SmoothStep(0f, 1f, (Time.unscaledTime - inicio) / duracion);
            if (canvasGroup != null) canvasGroup.alpha = t;
            if (titulo != null) titulo.localScale = Vector3.one * Mathf.Lerp(0.9f, 1f, t);
            if (personajes != null)
                foreach (Image personaje in personajes)
                    if (personaje != null)
                    {
                        Color color = personaje.color;
                        color.a = t;
                        personaje.color = color;
                    }
            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
        }

        float siguienteFrame = 0f;
        int frame = 0;
        while (visible)
        {
            float ahora = Time.unscaledTime;
            if (ahora >= siguienteFrame)
            {
                siguienteFrame = ahora + 0.18f;
                frame++;
                AplicarFrame(0, framesPlayer, frame);
                AplicarFrame(1, framesDocente, frame);
                AplicarFrame(2, framesVecino, frame);
                AplicarFrame(3, framesPromotor, frame);
            }
            if (rectsPersonajes != null)
                for (int i = 0; i < rectsPersonajes.Length; i++)
                    if (rectsPersonajes[i] != null)
                        rectsPersonajes[i].anchoredPosition = posicionesBase[i] +
                            Vector2.up * (Mathf.Sin(ahora * 2.3f + i * 0.8f) * 3.5f);
            yield return null;
        }
    }

    private void AplicarFrame(int indice, Sprite[] frames, int frame)
    {
        if (personajes == null || indice >= personajes.Length || personajes[indice] == null ||
            frames == null || frames.Length == 0) return;
        personajes[indice].sprite = frames[frame % frames.Length];
    }

    public void VolverAlMenu()
    {
        if (!visible) return;
        visible = false;
        if (botonVolverMenu != null) botonVolverMenu.interactable = false;
        AudioManager.Instancia?.ReproducirBoton();
        StartCoroutine(RecargarEscena());
    }

    private IEnumerator RecargarEscena()
    {
        yield return new WaitForSecondsRealtime(0.12f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
