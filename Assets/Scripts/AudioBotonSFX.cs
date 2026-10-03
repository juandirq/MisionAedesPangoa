using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AudioBotonSFX : MonoBehaviour
{
    private Button boton;
    private void OnEnable()
    {
        if (boton == null) boton = GetComponent<Button>();
        boton.onClick.RemoveListener(Reproducir);
        boton.onClick.AddListener(Reproducir);
    }
    private void OnDisable()
    {
        if (boton != null) boton.onClick.RemoveListener(Reproducir);
    }
    private static void Reproducir() => AudioManager.Instancia?.ReproducirBoton();
}
