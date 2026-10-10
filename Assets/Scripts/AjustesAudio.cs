using UnityEngine;

/// <summary>
/// Punto único para guardar los volúmenes elegidos en el menú.
/// Los valores son multiplicadores para no alterar el balance original del juego.
/// </summary>
public static class AjustesAudio
{
    public static event System.Action<float> SfxCambiado;

    public const string ClaveMusica = "MisionAedes.VolumenMusica";
    public const string ClaveSfx = "MisionAedes.VolumenSFX";

    public const float MusicaPredeterminada = 0.75f;
    public const float SfxPredeterminado = 0.85f;

    public static float Musica =>
        Mathf.Clamp01(PlayerPrefs.GetFloat(ClaveMusica, MusicaPredeterminada));

    public static float Sfx =>
        Mathf.Clamp01(PlayerPrefs.GetFloat(ClaveSfx, SfxPredeterminado));

    public static void GuardarMusica(float valor)
    {
        PlayerPrefs.SetFloat(ClaveMusica, Mathf.Clamp01(valor));
        PlayerPrefs.Save();
    }

    public static void GuardarSfx(float valor)
    {
        float ajustado = Mathf.Clamp01(valor);
        PlayerPrefs.SetFloat(ClaveSfx, ajustado);
        PlayerPrefs.Save();
        SfxCambiado?.Invoke(ajustado);
    }
}
