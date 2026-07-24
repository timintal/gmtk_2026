using System.Collections.Generic;
using UnityEngine;

namespace _Game.Features.Visuals
{
    [ExecuteAlways]
    public class SimpleLineLayout : MonoBehaviour
    {
        public Vector3 axis = Vector3.right;
        public float spacing = 1f;
        public bool includeInactive = false;

        private readonly List<Transform> _children = new();

        private bool _dirty = true;
        private Vector3 _lastAxis;
        private float _lastSpacing;
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
            return axis != _lastAxis
                   || !Mathf.Approximately(spacing, _lastSpacing)
                   || includeInactive != _lastIncludeInactive;
        }

        [ContextMenu("Refresh")]
        public void Refresh()
        {
            _lastAxis = axis;
            _lastSpacing = spacing;
            _lastIncludeInactive = includeInactive;

            var dir = axis.sqrMagnitude > Mathf.Epsilon ? axis.normalized : Vector3.right;

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

            float start = -spacing * (count - 1) * 0.5f;

            for (int i = 0; i < count; i++)
            {
                _children[i].localPosition = dir * (start + spacing * i);
            }
        }
    }
}
