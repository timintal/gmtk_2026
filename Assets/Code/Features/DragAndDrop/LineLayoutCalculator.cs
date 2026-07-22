using UnityEngine;

namespace Code.Features.DragAndDrop
{
    public static class LineLayoutCalculator
    {
        public static Vector3 GetLocalPosition(int index, int count, in LineContainerLayout layout)
        {
            var direction = layout.Direction.sqrMagnitude > 0f ? layout.Direction.normalized : Vector2.right;
            var centeredOffset = layout.Centered ? (count - 1) * 0.5f : 0f;
            var distance = (index - centeredOffset) * layout.Spacing;
            var position = layout.LocalOffset + direction * distance;

            return new Vector3(position.x, position.y, 0f);
        }
    }
}
