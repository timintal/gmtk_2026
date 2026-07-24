using Code.Common;
using Code.Common.View;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace _Game.Features.Blessings.Views
{
    public class BlessingsContainerView : ResourceMonoBehaviour<BlessingsContainerView>
    {
        [SerializeField] Transform _root;

        [Header("Fan Layout")]
        [Tooltip("Horizontal distance between neighbouring card centers. Smaller than a card's width = overlap.")]
        [SerializeField] float _cardSpacing = 3f;
        [SerializeField] float _hoverSpacing = 1f;
        [Tooltip("Extra tilt (degrees) applied per step away from the middle card.")]
        [SerializeField] float _anglePerCard = 5f;
        [Tooltip("How much lower a card sits per step away from the middle (arc depth).")]
        [SerializeField] float _sideDrop = 0.2f;
        [Tooltip("Clamp so large hands don't over-rotate / over-drop.")]
        [SerializeField] float _maxAngle = 10f;
        [Tooltip("Base sorting order; each card adds its index on top of this.")]
        [SerializeField] int _baseOrderInLayer = 0;
        
        BlessingView _hovered;
        private bool _dirty;

        public Transform Root => _root;

        void OnEnable() => _dirty = true;

        void Update()
        {
            UpdateHover();
            if (_dirty)
            {
                Layout();
                _dirty = false;
            }
        }

        private void OnTransformChildrenChanged() => _dirty = true;

        // Central hover resolution: when several cards overlap under the cursor, only the
        // top-most one (highest sibling index == drawn last == on top) is hovered.
        void UpdateHover()
        {
            var mouse = Mouse.current;
            var cam = W.GetResource<MainCamera>().Value;
            if (mouse == null)
                return;

            if (mouse.leftButton.isPressed)
            {
                if (_hovered != null && mouse.leftButton.wasPressedThisFrame)
                {
                    _hovered.OnClick();
                }
                return;
            }

            if (mouse.leftButton.wasReleasedThisFrame && _hovered != null)
            {
                _hovered.OnRelease();
            }
            
            var point = ScreenToWorld2D(cam, mouse.position.ReadValue());

            BlessingView best = null;
            var count = _root.childCount;
            for (var i = 0; i < count; i++)
            {
                var child = _root.GetChild(i);
                if (!child.TryGetComponent<BlessingView>(out var view) || view.Collider == null)
                    continue;

                if (view.Collider.OverlapPoint(point))
                    best = view; // ascending scan => last overlapping child (top-most) wins
            }

            if (best == _hovered)
                return;

            bool needRelayout = false;
            
            if (_hovered != null)
            {
                needRelayout = true;
                _hovered.SetHovered(false);
            }

            _hovered = best;

            if (_hovered != null)
            {
                needRelayout = true;
                _hovered.SetHovered(true);
            }
            
            _dirty |= needRelayout;
        }

        static Vector2 ScreenToWorld2D(Camera camera, Vector2 screen)
        {
            if (camera.orthographic)
            {
                var world = camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -camera.transform.position.z));
                return new Vector2(world.x, world.y);
            }

            var ray = camera.ScreenPointToRay(screen);
            var distance = Mathf.Abs(ray.direction.z) > 0.0001f ? -ray.origin.z / ray.direction.z : 0f;
            var pointOnPlane = ray.GetPoint(distance);
            return new Vector2(pointOnPlane.x, pointOnPlane.y);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (_root != null)
                Layout();
        }
#endif

        [Button]
        // Call this whenever cards are added/removed from _root.
        public void Layout()
        {
            var count = _root.childCount;
            if (count == 0)
                return;

            // Middle of the hand. For an odd count this is a real card, for an even count it sits between two.
            var center = (count - 1) * 0.5f;

            float hoverOffset = _hovered != null ? -_hoverSpacing : 0f;
            int currentOrder = _baseOrderInLayer;
            int curreentOrderMultiplier = 1;
            
            for (var i = 0; i < count; i++)
            {
                var card = _root.GetChild(i);

                // negative = left of middle, positive = right, 0 = middle.
                var offset = i - center;
                currentOrder += curreentOrderMultiplier;
                if (_hovered != null)
                {
                    if (card == _hovered.transform)
                    {
                        curreentOrderMultiplier = -1;
                        hoverOffset *= -1;
                    }
                    else
                    {
                        offset += hoverOffset;
                    }
                }

                var x = offset * _cardSpacing;
                var y = -(offset * offset) * _sideDrop;
                var angle = Mathf.Clamp(-offset * _anglePerCard, -_maxAngle, _maxAngle);

                card.DOKill();
                card.DOLocalMove(new Vector3(x, y, 0f), 0.2f);
                card.DOLocalRotateQuaternion(Quaternion.Euler(0f, 0f, angle), 0.2f);

                if (card.TryGetComponent<SortingGroup>(out var sortingGroup))
                    sortingGroup.sortingOrder = currentOrder;
            }
        }
    }
}
