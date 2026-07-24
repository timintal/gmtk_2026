using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public sealed class UIButtonLabelPress : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler
{
    [SerializeField] private RectTransform label;
    [SerializeField] private float pressOffset = 4f;

    private Selectable _selectable;
    private Vector2 _restPosition;
    private bool _pressed;

    private void Awake()
    {
        _selectable = GetComponent<Selectable>();
        if (label != null)
            _restPosition = label.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (!_selectable.IsInteractable())
            return;

        SetPressed(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        SetPressed(false);
    }

    private void OnDisable()
    {
        SetPressed(false);
    }

    private void SetPressed(bool pressed)
    {
        if (label == null || _pressed == pressed)
            return;

        _pressed = pressed;
        label.anchoredPosition = pressed
            ? _restPosition + Vector2.down * pressOffset
            : _restPosition;
    }
}
