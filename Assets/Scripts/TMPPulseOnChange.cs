using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPPulseOnChange : MonoBehaviour
{
    [Min(0.01f)] public float duracion = 0.25f;
    [Range(1f, 1.3f)] public float escalaMaxima = 1.08f;
    public bool usarColor;
    public Color colorFlash = new Color(1f, 0.75f, 0.25f);
    private TextMeshProUGUI texto;
    private string anterior;
    private Vector3 escalaBase;
    private Color colorBase;
    private float transcurrido;
    private bool animando;

    private void Awake()
    {
        texto = GetComponent<TextMeshProUGUI>();
        escalaBase = transform.localScale;
        colorBase = texto.color;
    }

    private void OnEnable() { anterior = texto.text; animando = false; }

    private void LateUpdate()
    {
        if (texto.text != anterior)
        {
            anterior = texto.text;
            transcurrido = 0f;
            animando = true;
        }
        if (!animando) return;
        transcurrido += Time.unscaledDeltaTime;
        float p = Mathf.Clamp01(transcurrido / Mathf.Max(0.01f, duracion));
        float pulso = Mathf.Sin(p * Mathf.PI);
        transform.localScale = escalaBase * (1f + (escalaMaxima - 1f) * pulso);
        if (usarColor) texto.color = Color.Lerp(colorBase, colorFlash, pulso);
        if (p >= 1f) Restaurar();
    }

    private void Restaurar()
    {
        animando = false;
        transform.localScale = escalaBase;
        if (usarColor && texto != null) texto.color = colorBase;
    }

    private void OnDisable() { if (texto != null) Restaurar(); }
}
