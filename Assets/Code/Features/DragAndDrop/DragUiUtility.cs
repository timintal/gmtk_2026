using UnityEngine;

namespace Code.Features.DragAndDrop
{
    internal static class DragUiUtility
    {
        public static bool ContainsScreenPoint(RectTransform rectTransform, Vector2 screenPosition)
        {
            if (rectTransform == null)
            {
                return false;
            }

            return RectTransformUtility.RectangleContainsScreenPoint(
                rectTransform,
                screenPosition,
                GetEventCamera(rectTransform));
        }

        public static bool TryScreenToLocalPosition(RectTransform rectTransform, Vector2 screenPosition, out Vector2 localPosition)
        {
            localPosition = default;
            if (rectTransform == null || rectTransform.parent is not RectTransform parentRect)
            {
                return false;
            }

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                screenPosition,
                GetEventCamera(rectTransform),
                out localPosition);
        }

        private static Camera GetEventCamera(Component component)
        {
            var canvas = component.GetComponentInParent<Canvas>();
            if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return null;
            }

            return canvas.worldCamera;
        }
    }
}
