using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Microinteracción visual y sonora para los botones del menú.
/// </summary>
public class MenuButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    public MainMenuController menu;
    public float escalaHover = 1.035f;
    public float escalaPresionada = 0.975f;
    public float velocidad = 12f;

    private Vector3 escalaBase;
    private Vector3 escalaObjetivo;

    private void Awake()
    {
        escalaBase = transform.localScale;
        escalaObjetivo = escalaBase;
    }

    private void OnEnable()
    {
        if (escalaBase == Vector3.zero) escalaBase = Vector3.one;
        escalaObjetivo = escalaBase;
        transform.localScale = escalaBase;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            escalaObjetivo,
            1f - Mathf.Exp(-velocidad * Time.unscaledDeltaTime));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        escalaObjetivo = escalaBase * escalaHover;
    }

    public void OnPointerExit(PointerEventData eventData) => escalaObjetivo = escalaBase;
    public void OnPointerDown(PointerEventData eventData) => escalaObjetivo = escalaBase * escalaPresionada;
    public void OnPointerUp(PointerEventData eventData) => escalaObjetivo = escalaBase * escalaHover;
    public void OnSelect(BaseEventData eventData) => escalaObjetivo = escalaBase * escalaHover;
    public void OnDeselect(BaseEventData eventData) => escalaObjetivo = escalaBase;
}
