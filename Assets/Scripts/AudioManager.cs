using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-900)]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instancia { get; private set; }

    [Header("Clips de música")]
    public AudioClip musicaInicio;
    public AudioClip musicaVictoria;
    public AudioClip musicaDerrota;
    public AudioClip musicaZona1;
    public AudioClip musicaZona2;
    public AudioClip musicaZona3;

    [Header("Clips de efectos")]
    public AudioClip efectoBoton;
    public AudioClip efectoCorrecto;
    public AudioClip efectoIncorrecto;
    public AudioClip efectoEscribirNPC;
    public AudioClip pisadas;
    public AudioClip ambienteRio;

    [Header("Fuentes centralizadas")]
    public AudioSource musicaA;
    public AudioSource musicaB;
    public AudioSource sfxGeneral;
    public AudioSource dialogo;
    public AudioSource fuentePisadas;
    public AudioSource fuenteRio;

    [Header("Referencias de escena")]
    public PlayerMovement playerMovement;
    public GameManager gameManager;
    public Transform[] referenciasRio;

    [Header("Volúmenes")]
    [Range(0f, 1f)] public float volumenMusica = 0.25f;
    [Range(0f, 1f)] public float volumenBoton = 0.42f;
    [Range(0f, 1f)] public float volumenRespuesta = 0.60f;
    [Range(0f, 1f)] public float volumenBlip = 0.25f;
    [Range(0f, 1f)] public float volumenPisadas = 0.20f;
    [Range(0f, 1f)] public float volumenRioMaximo = 0.30f;

    [Header("Inicios medidos (segundos)")]
    [Min(0f)] public float offsetMusicaInicio = 0.46f;
    [Min(0f)] public float offsetMusicaDerrota = 0.66f;
    [Min(0f)] public float offsetMusicaVictoria = 0f;
    [Min(0f)] public float offsetMusicaZona1 = 0.27f;
    [Min(0f)] public float offsetMusicaZona2 = 0.14f;
    [Min(0f)] public float offsetMusicaZona3 = 0.08f;
    [Min(0f)] public float offsetBoton = 2.37f;
    [Min(0f)] public float offsetCorrecto = 1.06f;
    [Min(0f)] public float offsetIncorrecto = 1.84f;
    [Min(0f)] public float offsetBlipNPC = 0.74f;
    [Min(0f)] public float offsetPisadas = 0.31f;

    [Header("Transiciones")]
    [Min(0.05f)] public float duracionCrossfade = 0.35f;
    [Min(0.1f)] public float intervaloPisada = 0.35f;
    [Min(0.01f)] public float cooldownBlip = 0.055f;
    [Min(0f)] public float distanciaRioMaximo = 3f;
    [Min(0.1f)] public float distanciaRioSilencio = 12f;
    [Min(0.1f)] public float suavizadoRio = 2.5f;

    private AudioSource musicaActiva;
    private Coroutine crossfadeActual;
    private float proximaPisada;
    private float proximoBlip;
    private bool estabaCaminandoAudio;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Debug.LogWarning("AudioManager duplicado desactivado.", this);
            enabled = false;
            return;
        }
        Instancia = this;
        ConfigurarFuente2D(musicaA);
        ConfigurarFuente2D(musicaB);
        ConfigurarFuente2D(sfxGeneral);
        ConfigurarFuente2D(dialogo);
        ConfigurarFuente2D(fuentePisadas);
        ConfigurarFuente2D(fuenteRio);
        if (musicaA != null) musicaA.volume = 0f;
        if (musicaB != null) musicaB.volume = 0f;
        if (fuentePisadas != null)
        {
            fuentePisadas.mute = false;
            fuentePisadas.volume = 0f;
        }
        musicaActiva = musicaA;
    }

    private void Start()
    {
        ReproducirMusicaExploracion();
        if (fuenteRio != null && ambienteRio != null)
        {
            fuenteRio.clip = ambienteRio;
            fuenteRio.loop = true;
            fuenteRio.volume = 0f;
            if (!fuenteRio.isPlaying) fuenteRio.Play();
        }
    }

    private void Update()
    {
        ActualizarRio();
    }

    private void LateUpdate()
    {
        ActualizarPisadas();
    }

    public void ReproducirMusicaExploracion() =>
        CambiarMusica(musicaInicio, true, offsetMusicaInicio);

    public void ReiniciarMusicaExploracion()
    {
        if (musicaInicio == null || musicaA == null || musicaB == null) return;
        if (crossfadeActual != null)
        {
            StopCoroutine(crossfadeActual);
            crossfadeActual = null;
        }
        musicaA.Stop();
        musicaB.Stop();
        musicaB.volume = 0f;
        musicaA.clip = musicaInicio;
        musicaA.loop = true;
        musicaA.volume = VolumenMusicaActual;
        AplicarOffset(musicaA, musicaInicio, offsetMusicaInicio);
        musicaA.Play();
        musicaActiva = musicaA;
    }

    public void AplicarAjustesGuardados()
    {
        if (musicaA != null)
            musicaA.volume = musicaA == musicaActiva && musicaA.isPlaying ? VolumenMusicaActual : 0f;
        if (musicaB != null)
            musicaB.volume = musicaB == musicaActiva && musicaB.isPlaying ? VolumenMusicaActual : 0f;
    }

    public void ReproducirMusicaZona(int indice)
    {
        AudioClip clip = indice == 1 ? musicaZona1 : indice == 2 ? musicaZona2 :
            indice == 3 ? musicaZona3 : null;
        float offset = indice == 1 ? offsetMusicaZona1 : indice == 2 ? offsetMusicaZona2 :
            indice == 3 ? offsetMusicaZona3 : 0f;
        if (clip != null) CambiarMusica(clip, true, offset);
    }

    public void ReproducirVictoria() =>
        CambiarMusica(musicaVictoria, false, offsetMusicaVictoria, true);
    public void ReproducirDerrota() =>
        CambiarMusica(musicaDerrota, false, offsetMusicaDerrota, true);
    public void ReproducirBoton() =>
        ReproducirDesdeOffset(sfxGeneral, efectoBoton,
            volumenBoton * AjustesAudio.Sfx, offsetBoton);
    public void ReproducirCorrecto() =>
        ReproducirDesdeOffset(sfxGeneral, efectoCorrecto,
            volumenRespuesta * AjustesAudio.Sfx, offsetCorrecto);
    public void ReproducirIncorrecto() =>
        ReproducirDesdeOffset(sfxGeneral, efectoIncorrecto,
            volumenRespuesta * AjustesAudio.Sfx, offsetIncorrecto);

    public void ReproducirBlipDialogo()
    {
        if (Time.unscaledTime < proximoBlip || dialogo == null || efectoEscribirNPC == null) return;
        proximoBlip = Time.unscaledTime + cooldownBlip;
        ReproducirDesdeOffset(dialogo, efectoEscribirNPC,
            volumenBlip * AjustesAudio.Sfx, offsetBlipNPC);
    }

    public void DetenerBlipDialogo()
    {
        if (dialogo != null && dialogo.isPlaying)
            dialogo.Stop();
        proximoBlip = 0f;
    }

    private static void ReproducirDesdeOffset(AudioSource fuente, AudioClip clip,
        float volumen, float offsetSegundos)
    {
        if (fuente == null || clip == null) return;
        fuente.Stop();
        fuente.clip = clip;
        fuente.loop = false;
        fuente.volume = volumen;
        AplicarOffset(fuente, clip, offsetSegundos);
        fuente.Play();
    }

    private void CambiarMusica(AudioClip clip, bool loop, float offsetSegundos,
        bool entradaInmediata = false)
    {
        if (clip == null || musicaA == null || musicaB == null) return;
        if (crossfadeActual == null && musicaActiva != null &&
            musicaActiva.clip == clip && musicaActiva.isPlaying)
        {
            musicaActiva.loop = loop;
            musicaActiva.volume = VolumenMusicaActual;
            return;
        }

        AudioSource destino;
        if (musicaA.clip == clip && musicaA.isPlaying)
            destino = musicaA;
        else if (musicaB.clip == clip && musicaB.isPlaying)
            destino = musicaB;
        else
            destino = musicaA.volume <= musicaB.volume ? musicaA : musicaB;

        if (crossfadeActual != null) StopCoroutine(crossfadeActual);
        crossfadeActual = StartCoroutine(
            Crossfade(destino, clip, loop, offsetSegundos, entradaInmediata));
    }

    private IEnumerator Crossfade(AudioSource destino, AudioClip clip, bool loop,
        float offsetSegundos, bool entradaInmediata)
    {
        AudioSource anterior = destino == musicaA ? musicaB : musicaA;
        bool continuarClipActual = destino.clip == clip && destino.isPlaying;
        if (!continuarClipActual)
        {
            destino.Stop();
            destino.clip = clip;
            destino.volume = entradaInmediata ? VolumenMusicaActual : 0f;
            AplicarOffset(destino, clip, offsetSegundos);
            destino.Play();
        }
        destino.loop = loop;
        float inicialDestino = destino.volume;
        float inicialAnterior = anterior != null ? anterior.volume : 0f;
        float tiempo = 0f;
        while (tiempo < duracionCrossfade)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / duracionCrossfade);
            destino.volume = Mathf.Lerp(inicialDestino, VolumenMusicaActual, t);
            if (anterior != null) anterior.volume = Mathf.Lerp(inicialAnterior, 0f, t);
            yield return null;
        }
        destino.volume = VolumenMusicaActual;
        if (anterior != null) { anterior.Stop(); anterior.volume = 0f; }
        musicaActiva = destino;
        crossfadeActual = null;
    }

    private void ActualizarPisadas()
    {
        if (fuentePisadas == null || pisadas == null || playerMovement == null) return;
        bool caminar = Time.timeScale > 0f && playerMovement.enabled &&
            playerMovement.EstaMoviendose && (gameManager == null || !gameManager.JuegoTerminado);
        bool esLoop = pisadas.length >= 0.8f;
        if (esLoop)
        {
            fuentePisadas.clip = pisadas;
            fuentePisadas.loop = true;
            if (caminar && !estabaCaminandoAudio)
            {
                fuentePisadas.Stop();
                fuentePisadas.volume = volumenPisadas * AjustesAudio.Sfx;
                AplicarOffset(fuentePisadas, pisadas, offsetPisadas);
                fuentePisadas.Play();
            }
            else if (!caminar && estabaCaminandoAudio)
            {
                fuentePisadas.Stop();
                fuentePisadas.volume = 0f;
            }
            else if (caminar && !fuentePisadas.isPlaying)
            {
                fuentePisadas.volume = volumenPisadas * AjustesAudio.Sfx;
                AplicarOffset(fuentePisadas, pisadas, offsetPisadas);
                fuentePisadas.Play();
            }
        }
        else if (caminar && Time.time >= proximaPisada)
        {
            proximaPisada = Time.time + intervaloPisada;
            fuentePisadas.PlayOneShot(pisadas, volumenPisadas * AjustesAudio.Sfx);
        }
        estabaCaminandoAudio = caminar;
    }

    private void ActualizarRio()
    {
        if (fuenteRio == null || playerMovement == null || referenciasRio == null) return;
        float menor = float.PositiveInfinity;
        Vector2 jugador = playerMovement.transform.position;
        foreach (Transform rio in referenciasRio)
            if (rio != null) menor = Mathf.Min(menor, Vector2.Distance(jugador, rio.position));
        float t = Mathf.InverseLerp(distanciaRioSilencio, distanciaRioMaximo, menor);
        float objetivo = Mathf.Clamp01(t) * volumenRioMaximo * AjustesAudio.Sfx;
        fuenteRio.volume = Mathf.MoveTowards(fuenteRio.volume, objetivo,
            Time.unscaledDeltaTime * suavizadoRio);
    }

    private static void ConfigurarFuente2D(AudioSource fuente)
    {
        if (fuente == null) return;
        fuente.playOnAwake = false;
        fuente.spatialBlend = 0f;
    }

    private float VolumenMusicaActual => volumenMusica * AjustesAudio.Musica;

    private static void AplicarOffset(AudioSource fuente, AudioClip clip, float segundos)
    {
        if (fuente == null || clip == null || clip.samples <= 1) return;
        int muestra = Mathf.RoundToInt(Mathf.Max(0f, segundos) * clip.frequency);
        fuente.timeSamples = Mathf.Clamp(muestra, 0, clip.samples - 1);
    }
}
