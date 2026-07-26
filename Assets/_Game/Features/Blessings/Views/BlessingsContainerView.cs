using _Game.Features.Run;
using Code.Common;
using Code.Common.Utils;
using Code.Common.View;
using DG.Tweening;
using FFS.Libraries.StaticEcs;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
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
        private int _lastUpdateItemsCount;

        public Transform Root => _root;

        void OnEnable() => _dirty = true;

        void Update()
        {
            if (W.Query<All<ActiveTurn>>().EntitiesCount() > 0)
            {
                UpdateHover();
            }
            else if (_hovered != null)
            {
                _hovered.SetHovered(false);
                _hovered = null;
                _dirty = true;
            }
            
            var currentItemsCount = W.Query<All<Blessing, Hand>>().EntitiesCount();
            if (currentItemsCount != _lastUpdateItemsCount)
            {
                _dirty = true;
            }

            if (_dirty)
            {
                Layout();
                _dirty = false;
            }
        }

        private void OnTransformChildrenChanged() => _dirty = true;

        public void SetDirty()
        {
            _dirty = true;
        }
        
        void UpdateHover()
        {
            // Pointer unifies mouse and touch: press == left button / primary touch.
            var pointer = Pointer.current;
            var cam = W.GetResource<MainCamera>().Value;
            if (pointer == null)
                return;

            if (pointer.press.isPressed)
            {
                if (_hovered != null)
                {
                    if (pointer.press.wasPressedThisFrame)
                    {
                        _hovered.OnClick();
                    }
                    // Cancel drag: right-click on desktop, or a second finger on touch.
                    if (WasCancelPressedThisFrame())
                    {
                        _hovered.SetHovered(false);
                        _hovered.ResetDrag();
                        _hovered = null;
                        _dirty = true;
                    }
                    return;
                }
            }

            if (pointer.press.wasReleasedThisFrame && _hovered != null)
            {
                _hovered.OnRelease();
                _hovered = null;
                _dirty = true;
            }
            
            var point = ScreenUtils.ScreenToWorld2D(cam, pointer.position.ReadValue());

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

        // Desktop: right mouse button. Touch: a second finger touching the screen.
        static bool WasCancelPressedThisFrame()
        {
            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
                return true;

            var touch = Touchscreen.current;
            if (touch != null)
            {
                var touches = touch.touches;
                for (var i = 0; i < touches.Count; i++)
                {
                    if (touches[i].press.wasPressedThisFrame && ActiveTouchCount(touches) >= 2)
                        return true;
                }
            }

            return false;
        }

        static int ActiveTouchCount(UnityEngine.InputSystem.Utilities.ReadOnlyArray<TouchControl> touches)
        {
            var active = 0;
            for (var i = 0; i < touches.Count; i++)
            {
                if (touches[i].press.isPressed)
                    active++;
            }
            return active;
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
            _lastUpdateItemsCount = count;
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
