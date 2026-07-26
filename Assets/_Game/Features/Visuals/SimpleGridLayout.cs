using System.Collections.Generic;
using UnityEngine;

namespace _Game.Features.Visuals
{
    [ExecuteAlways]
    public class SimpleGridLayout : MonoBehaviour
    {
        public Vector3 rowAxis = Vector3.right;
        public Vector3 columnAxis = Vector3.down;
        public Vector2 spacing = Vector2.one;
        public int columns = 3;
        public bool includeInactive = false;

        private readonly List<Transform> _children = new();

        private bool _dirty = true;
        private Vector3 _lastRowAxis;
        private Vector3 _lastColumnAxis;
        private Vector2 _lastSpacing;
        private int _lastColumns;
        private bool _lastIncludeInactive;

        private void OnEnable() => _dirty = true;

        private void OnTransformChildrenChanged() => _dirty = true;

#if UNITY_EDITOR
        private void OnValidate() => _dirty = true;
#endif

        private void Update()
        {
            if (ParamsChanged())
            {
                _dirty = true;
            }

            if (!_dirty)
            {
                return;
            }

            Refresh();
            _dirty = false;
        }

        private bool ParamsChanged()
        {
            return rowAxis != _lastRowAxis
                   || columnAxis != _lastColumnAxis
                   || spacing != _lastSpacing
                   || columns != _lastColumns
                   || includeInactive != _lastIncludeInactive;
        }

        [ContextMenu("Refresh")]
        public void Refresh()
        {
            _lastRowAxis = rowAxis;
            _lastColumnAxis = columnAxis;
            _lastSpacing = spacing;
            _lastColumns = columns;
            _lastIncludeInactive = includeInactive;

            var rowDir = rowAxis.sqrMagnitude > Mathf.Epsilon ? rowAxis.normalized : Vector3.right;
            var colDir = columnAxis.sqrMagnitude > Mathf.Epsilon ? columnAxis.normalized : Vector3.down;

            _children.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (includeInactive || child.gameObject.activeSelf)
                {
                    _children.Add(child);
                }
            }

            int count = _children.Count;
            if (count == 0)
            {
                return;
            }

            int cols = Mathf.Max(1, columns);
            int rows = Mathf.CeilToInt(count / (float)cols);

            float startY = -spacing.y * (rows - 1) * 0.5f;

            for (int i = 0; i < count; i++)
            {
                int col = i % cols;
                int row = i / cols;

                // Number of items actually in this row (last row may be partial).
                int itemsInRow = Mathf.Min(cols, count - row * cols);
                float startX = -spacing.x * (itemsInRow - 1) * 0.5f;

                _children[i].localPosition = rowDir * (startX + spacing.x * col)
                                             + colDir * (startY + spacing.y * row);
            }
        }
    }
}
