using System;
using Code.Configs;
using Code.Features.Tooltip;
using FFS.Libraries.StaticEcs;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Common.View.UI
{
    public sealed class TooltipCanvas : ResourceMonoBehaviour<TooltipCanvas>
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private Vector2 _cursorOffset = new(16f, -16f);
        [SerializeField] private int _sortingOrder = 5000;

        private Canvas _canvas;

        public RectTransform RootRect => _root != null ? _root : _root = transform as RectTransform;

        public Vector2 CursorOffset => _cursorOffset;

        public Camera EventCamera
        {
            get
            {
                if (_canvas == null)
                {
                    _canvas = GetComponent<Canvas>();
                }

                return _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? _canvas.worldCamera
                    : null;
            }
        }

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            if (_canvas != null)
            {
                _canvas.overrideSorting = true;
                _canvas.sortingOrder = _sortingOrder;
            }

            if (_root == null)
            {
                _root = transform as RectTransform;
            }

            var raycaster = GetComponent<GraphicRaycaster>();
            if (raycaster != null)
            {
                raycaster.enabled = false;
            }
        }

        public TooltipView SpawnTooltip(W.Entity target, TooltipType type, Vector2 screenPosition)
        {
            if (W.Status != WorldStatus.Initialized || !W.HasResource<VisualConfig>())
            {
                return null;
            }

            var prefab = W.GetResource<VisualConfig>().GetTooltipPrefab(type);
            if (prefab == null)
            {
                Debug.LogWarning($"Tooltip prefab is not configured for type '{type}'.", this);
                return null;
            }

            var instance = Instantiate(prefab, RootRect);
            instance.Bind(target);
            instance.ShowAtScreenPosition(screenPosition, this);
            return instance;
        }
    }
}
