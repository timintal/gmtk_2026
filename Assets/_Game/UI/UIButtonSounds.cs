using Code.Common.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public sealed class UIButtonSounds : MonoBehaviour,
    IPointerEnterHandler,
    ISelectHandler,
    IPointerClickHandler
{
    [SerializeField] private AudioClip highlightClip;
    [SerializeField] private AudioClip clickClip;

    private Selectable _selectable;
    private bool _pointerInside;

    private void Awake()
    {
        _selectable = GetComponent<Selectable>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _pointerInside = true;
        PlayHighlight();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            Play(clickClip);
    }

    public void OnSelect(BaseEventData eventData)
    {
        // Prevent playing twice when mouse hover also causes selection.
        if (!_pointerInside)
            PlayHighlight();
    }

    private void OnDisable()
    {
        _pointerInside = false;
    }

    private void PlayHighlight()
    {
        if (_selectable.IsInteractable())
            Play(highlightClip);
    }

    private void Play(AudioClip clip)
    {
        if (clip != null)
        {
            W.GetResource<SFXAudioSource>().PlayOneShot(clip);
        }
    }
}