using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonTextColorChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private TextMeshProUGUI text;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color clickColor = Color.green;

    void Awake()
    {
        // Automatically find the TextMeshProUGUI in child
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        text.color = clickColor;
    }

    void Start()
    {
        text.color = normalColor;
    }
}