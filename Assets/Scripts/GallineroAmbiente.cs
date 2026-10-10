using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public sealed class GallineroAmbiente : MonoBehaviour
{
    [Header("Referencias")]
    public Transform jugador;
    public AudioSource fuente;

    [Header("Proximidad XY")]
    [Range(0f, 1f)] public float volumenMaximo = 0.14f;
    [Min(0f)] public float distanciaVolumenMaximo = 3.5f;
    [Min(0.1f)] public float distanciaSilencio = 13.5f;
    [Min(0.01f)] public float velocidadFundido = 0.18f;

    private float multiplicadorSfx;
    private float volumenObjetivo;
    private bool pausadoPorJuego;

    public float VolumenObjetivo => volumenObjetivo;
    public bool PausadoPorJuego => pausadoPorJuego;

    private void Awake()
    {
        if (fuente == null) fuente = GetComponent<AudioSource>();
        multiplicadorSfx = AjustesAudio.Sfx;
        ConfigurarFuente();
    }

    private void OnEnable()
    {
        AjustesAudio.SfxCambiado += AlCambiarVolumenSfx;
        multiplicadorSfx = AjustesAudio.Sfx;
    }

    private void Start()
    {
        if (fuente == null || fuente.clip == null) return;
        fuente.volume = 0f;
        if (!fuente.isPlaying) fuente.Play();
    }

    private void Update()
    {
        if (fuente == null || jugador == null) return;

        if (Time.timeScale <= 0f)
        {
            if (!pausadoPorJuego)
            {
                fuente.Pause();
                pausadoPorJuego = true;
            }
            return;
        }

        if (pausadoPorJuego)
        {
            fuente.UnPause();
            pausadoPorJuego = false;
        }
        if (!fuente.isPlaying && fuente.clip != null) fuente.Play();

        Vector2 origen = transform.position;
        Vector2 destino = jugador.position;
        float distancia = Vector2.Distance(origen, destino);
        float cercania = Mathf.InverseLerp(distanciaSilencio, distanciaVolumenMaximo, distancia);
        volumenObjetivo = Mathf.Clamp01(cercania) * volumenMaximo * multiplicadorSfx;
        fuente.volume = Mathf.MoveTowards(fuente.volume, volumenObjetivo,
            Time.unscaledDeltaTime * velocidadFundido);
    }

    private void OnDisable()
    {
        AjustesAudio.SfxCambiado -= AlCambiarVolumenSfx;
        if (fuente != null) fuente.Stop();
        pausadoPorJuego = false;
    }

    private void AlCambiarVolumenSfx(float valor)
    {
        multiplicadorSfx = Mathf.Clamp01(valor);
        if (multiplicadorSfx <= 0f && fuente != null)
        {
            volumenObjetivo = 0f;
            fuente.volume = 0f;
        }
    }

    private void ConfigurarFuente()
    {
        if (fuente == null) return;
        fuente.playOnAwake = false;
        fuente.loop = true;
        fuente.spatialBlend = 0f;
        fuente.dopplerLevel = 0f;
        fuente.volume = 0f;
    }
}
