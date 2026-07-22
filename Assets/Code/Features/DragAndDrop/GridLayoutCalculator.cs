using UnityEngine;

namespace Code.Features.DragAndDrop
{
    public static class GridLayoutCalculator
    {
        public static Vector3 GetLocalPosition(int index, int count, in GridContainerLayout layout)
        {
            var columns = Mathf.Max(1, layout.ColumnCount);
            var row = index / columns;
            var col = index % columns;

            var totalRows = count <= 0 ? 0 : (count - 1) / columns + 1;
            var itemsInRow = Mathf.Min(columns, count - row * columns);
            var colCenter = layout.Centered ? (itemsInRow - 1) * 0.5f : 0f;
            var rowCenter = layout.Centered ? (totalRows - 1) * 0.5f : 0f;

            var position = layout.LocalOffset + new Vector2(
                (col - colCenter) * layout.CellSpacing.x,
                -(row - rowCenter) * layout.CellSpacing.y);

            return new Vector3(position.x, position.y, 0f);
        }
    }
}
