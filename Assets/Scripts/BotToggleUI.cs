using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BotToggleUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Toggle toggle;                      // asignar el Toggle
    public TextMeshProUGUI statusText;         // texto que mostrará el estado debajo del control

    [Header("Textos y colores")]
    public string onText = "BOT ACTIVADO";
    public string offText = "BOT DESACTIVADO";
    public Color onColor = Color.green;
    public Color offColor = Color.red;

    void Reset()
    {
        // autoconfigura si colocas el script directamente en el Toggle
        toggle = GetComponent<Toggle>();
        if (statusText == null)
            statusText = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Start()
    {
        if (toggle == null)
        {
            Debug.LogError("BotToggleUI: asigna el Toggle en el inspector.");
            enabled = false;
            return;
        }

        // Estado inicial
        UpdateVisual(toggle.isOn);

        // Suscribimos al evento del Toggle
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnDestroy()
    {
        if (toggle != null)
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    void OnToggleChanged(bool value)
    {
        // Llama al controlador del bot
        var ai = FindObjectOfType<AIController>();
        if (ai != null)
            ai.SetBotActive(value);

        // Actualiza el texto/ color
        UpdateVisual(value);
    }

    void UpdateVisual(bool active)
    {
        if (statusText != null)
        {
            statusText.text = active ? onText : offText;
            statusText.color = active ? onColor : offColor;
        }
    }
}
