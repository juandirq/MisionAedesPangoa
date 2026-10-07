using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Controla el menú inicial integrado en Zona1_Escuela sin recargar la escena.
/// </summary>
[DefaultExecutionOrder(100)]
public class MainMenuController : MonoBehaviour
{
    [Header("Raíz y paneles")]
    public GameObject raizMenu;
    public GameObject panelPrincipal;
    public GameObject panelOpciones;
    public CanvasGroup grupoMenu;

    [Header("Botones")]
    public Button botonJugar;
    public Button botonOpciones;
    public Button botonSalir;
    public Button botonVolver;

    [Header("Opciones")]
    public Slider sliderMusica;
    public Slider sliderSfx;
    public TMP_Text valorMusica;
    public TMP_Text valorSfx;

    [Header("Sistemas existentes de Zona1_Escuela")]
    public AudioManager audioManager;
    public GameManager gameManager;
    public PlayerMovement playerMovement;

    private bool iniciandoJuego;
    private bool controlesConfigurados;

    public bool MenuVisible => raizMenu != null && raizMenu.activeInHierarchy;

    private void Awake()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (audioManager == null) audioManager = FindAnyObjectByType<AudioManager>();
        if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
        if (playerMovement == null) playerMovement = FindAnyObjectByType<PlayerMovement>();

        PrepararMenuVisible();
        BloquearGameplay();
        ConfigurarControles();
    }

    private IEnumerator Start()
    {
        // AudioManager arranca antes (-900). Aquí aplicamos preferencias y dejamos
        // la música de exploración desde su inicio medido para el menú.
        audioManager?.AplicarAjustesGuardados();
        audioManager?.ReiniciarMusicaExploracion();
        yield return null;
        Seleccionar(botonJugar);
    }

    private void Update()
    {
        if (iniciandoJuego || !MenuVisible || !Input.GetKeyDown(KeyCode.Escape)) return;
        if (panelOpciones != null && panelOpciones.activeSelf) VolverAlMenu();
        // En el panel principal ESC se consume sin abrir el menú de pausa.
    }

    private void PrepararMenuVisible()
    {
        if (raizMenu != null) raizMenu.SetActive(true);
        if (panelPrincipal != null) panelPrincipal.SetActive(true);
        if (panelOpciones != null) panelOpciones.SetActive(false);
        if (grupoMenu != null)
        {
            grupoMenu.alpha = 1f;
            grupoMenu.interactable = true;
            grupoMenu.blocksRaycasts = true;
        }
    }

    private void BloquearGameplay()
    {
        if (playerMovement != null) playerMovement.enabled = false;
        if (gameManager == null) return;
        if (gameManager.panelHUDIzquierdo != null) gameManager.panelHUDIzquierdo.SetActive(false);
        if (gameManager.panelHUDDerecho != null) gameManager.panelHUDDerecho.SetActive(false);
        if (gameManager.textoInteraccion != null) gameManager.textoInteraccion.SetActive(false);
        if (gameManager.ventanaDecision != null) gameManager.ventanaDecision.SetActive(false);
    }

    private void ConfigurarControles()
    {
        if (controlesConfigurados) return;
        controlesConfigurados = true;

        if (sliderMusica != null)
        {
            sliderMusica.SetValueWithoutNotify(AjustesAudio.Musica);
            sliderMusica.onValueChanged.AddListener(CambiarVolumenMusica);
        }
        if (sliderSfx != null)
        {
            sliderSfx.SetValueWithoutNotify(AjustesAudio.Sfx);
            sliderSfx.onValueChanged.AddListener(CambiarVolumenSfx);
        }

        if (botonJugar != null) botonJugar.onClick.AddListener(Jugar);
        if (botonOpciones != null) botonOpciones.onClick.AddListener(AbrirOpciones);
        if (botonSalir != null) botonSalir.onClick.AddListener(Salir);
        if (botonVolver != null) botonVolver.onClick.AddListener(VolverAlMenu);
        ActualizarEtiquetas();
    }

    public void Jugar()
    {
        if (!iniciandoJuego) StartCoroutine(IniciarGameplay());
    }

    public void AbrirOpciones()
    {
        if (iniciandoJuego) return;
        ReproducirClick();
        if (panelPrincipal != null) panelPrincipal.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(true);
        Seleccionar(botonVolver);
    }

    public void VolverAlMenu()
    {
        if (iniciandoJuego) return;
        ReproducirClick();
        PlayerPrefs.Save();
        if (panelOpciones != null) panelOpciones.SetActive(false);
        if (panelPrincipal != null) panelPrincipal.SetActive(true);
        Seleccionar(botonJugar);
    }

    public void Salir()
    {
        if (!iniciandoJuego) StartCoroutine(SalirTrasSonido());
    }

    public void CambiarVolumenMusica(float valor)
    {
        AjustesAudio.GuardarMusica(valor);
        audioManager?.AplicarAjustesGuardados();
        ActualizarEtiquetas();
    }

    public void CambiarVolumenSfx(float valor)
    {
        AjustesAudio.GuardarSfx(valor);
        audioManager?.AplicarAjustesGuardados();
        ActualizarEtiquetas();
    }

    private void ReproducirClick() => audioManager?.ReproducirBoton();

    private IEnumerator IniciarGameplay()
    {
        iniciandoJuego = true;
        ReproducirClick();
        HabilitarBotones(false);

        const float duracion = 0.22f;
        float tiempo = 0f;
        while (grupoMenu != null && tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            grupoMenu.alpha = 1f - Mathf.Clamp01(tiempo / duracion);
            yield return null;
        }

        if (raizMenu != null) raizMenu.SetActive(false);
        if (gameManager != null)
        {
            if (gameManager.panelHUDIzquierdo != null) gameManager.panelHUDIzquierdo.SetActive(true);
            if (gameManager.panelHUDDerecho != null) gameManager.panelHUDDerecho.SetActive(true);
            if (gameManager.textoInteraccion != null) gameManager.textoInteraccion.SetActive(false);
        }
        if (playerMovement != null) playerMovement.enabled = true;

        // La exploración conserva su estado inicial; únicamente reiniciamos la
        // misma pista ya configurada en AudioManager.
        audioManager?.ReiniciarMusicaExploracion();
    }

    private IEnumerator SalirTrasSonido()
    {
        iniciandoJuego = true;
        ReproducirClick();
        HabilitarBotones(false);
        yield return new WaitForSecondsRealtime(0.18f);

#if UNITY_EDITOR
        Debug.Log("Salir: deteniendo Play Mode en el Editor.");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void HabilitarBotones(bool habilitados)
    {
        if (botonJugar != null) botonJugar.interactable = habilitados;
        if (botonOpciones != null) botonOpciones.interactable = habilitados;
        if (botonSalir != null) botonSalir.interactable = habilitados;
        if (botonVolver != null) botonVolver.interactable = habilitados;
    }

    private void ActualizarEtiquetas()
    {
        if (valorMusica != null) valorMusica.text = Mathf.RoundToInt(AjustesAudio.Musica * 100f) + "%";
        if (valorSfx != null) valorSfx.text = Mathf.RoundToInt(AjustesAudio.Sfx * 100f) + "%";
    }

    private static void Seleccionar(Button boton)
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(boton != null ? boton.gameObject : null);
    }
}
