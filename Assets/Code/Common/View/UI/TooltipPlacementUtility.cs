using UnityEngine;

namespace Code.Common.View.UI
{
    public static class TooltipPlacementUtility
    {
        private const float ScreenPadding = 8f;

        public static void PlaceAtScreenPoint(
            RectTransform tooltip,
            RectTransform canvasRect,
            Vector2 screenPosition,
            Vector2 cursorOffset,
            Camera eventCamera)
        {
            if (tooltip == null || canvasRect == null)
            {
                return;
            }

            var targetScreenPoint = screenPosition + cursorOffset;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    targetScreenPoint,
                    eventCamera,
                    out var localPoint))
            {
                return;
            }

            tooltip.localPosition = localPoint;
            ClampToScreen(tooltip, canvasRect, eventCamera);
        }

        private static void ClampToScreen(RectTransform tooltip, RectTransform canvasRect, Camera eventCamera)
        {
            var corners = new Vector3[4];
            tooltip.GetWorldCorners(corners);

            var minX = corners[0].x;
            var minY = corners[0].y;
            var maxX = corners[2].x;
            var maxY = corners[2].y;

            var deltaX = 0f;
            var deltaY = 0f;

            if (minX < ScreenPadding)
            {
                deltaX += ScreenPadding - minX;
            }

            if (maxX > Screen.width - ScreenPadding)
            {
                deltaX -= maxX - (Screen.width - ScreenPadding);
            }

            if (minY < ScreenPadding)
            {
                deltaY += ScreenPadding - minY;
            }

            if (maxY > Screen.height - ScreenPadding)
            {
                deltaY -= maxY - (Screen.height - ScreenPadding);
            }

            if (Mathf.Approximately(deltaX, 0f) && Mathf.Approximately(deltaY, 0f))
            {
                return;
            }

            var currentScreenPoint = RectTransformUtility.WorldToScreenPoint(eventCamera, tooltip.position);
            var adjustedScreenPoint = currentScreenPoint + new Vector2(deltaX, deltaY);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    adjustedScreenPoint,
                    eventCamera,
                    out var adjustedLocalPoint))
            {
                tooltip.localPosition = adjustedLocalPoint;
            }
        }
    }
}
