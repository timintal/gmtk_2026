using System;
using DG.Tweening;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Common.View.UI
{
    public abstract class TooltipView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _hideDuration = 0.15f;

        private RectTransform _rectTransform;
        private Tween _hideTween;

        protected RectTransform RectTransform => _rectTransform != null
            ? _rectTransform
            : _rectTransform = transform as RectTransform;

        public abstract void Bind(W.Entity entity);

        public void ShowAtScreenPosition(Vector2 screenPosition, TooltipCanvas canvas)
        {
            gameObject.SetActive(true);

            if (_canvasGroup != null)
            {
                _hideTween?.Kill();
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }

            TooltipPlacementUtility.PlaceAtScreenPoint(
                RectTransform,
                canvas.RootRect,
                screenPosition,
                canvas.CursorOffset,
                canvas.EventCamera);
        }

        public virtual void Hide()
        {
            if (_canvasGroup == null)
            {
                Destroy(gameObject);
                return;
            }

            _hideTween?.Kill();
            _hideTween = _canvasGroup
                .DOFade(0f, _hideDuration)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (this != null)
                    {
                        Destroy(gameObject);
                    }
                });
        }

        private void OnDestroy()
        {
            _hideTween?.Kill();
        }
    }
}
