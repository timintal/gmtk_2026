using UnityEngine;
using UnityEngine.EventSystems;

namespace EasyTweens
{
    public class ClickableUIElement : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private TweenAnimation _pointerDownAnimation;
        [SerializeField] private TweenAnimation _pointerUpAnimation;

        private bool _clicked;
    
        public void OnPointerDown(PointerEventData eventData)
        {
            _pointerUpAnimation.enabled = false;
            _pointerDownAnimation.Play();
            _clicked = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_clicked)
            {
                _pointerDownAnimation.enabled = false;
                _pointerUpAnimation.Play();
                _clicked = false;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_clicked)
            {
                _pointerDownAnimation.enabled = false;
                _pointerUpAnimation.Play();
                _clicked = false;
            }
        }
    }
}