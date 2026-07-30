using Code.Common.View.UI;
using Code.Features.DragAndDrop;
using FFS.Libraries.StaticEcs;
using UnityEngine;
using VContainer;

namespace Code.Features.Tooltip
{
    public sealed class TooltipHoverSystem : ISystem
    {
        public const float ShowDelaySeconds = 0.2f;
        public const float MovementResetThresholdPixels = 8f;

        private EntityGID _hoverTarget;
        private float _hoverTimer;
        private TooltipView _hoverTooltip;

        private EntityGID _touchTarget;
        private float _touchTimer;
        private TooltipView _touchTooltip;
        
        [Inject] internal TooltipCanvas _tooltipCanvas;

        public void Update()
        {
            if (!W.HasResource<TooltipPointerState>())
            {
                return;
            }

            if (!TooltipFeature.IsEnabled())
            {
                CancelHoverTracking();
                return;
            }

            if (HasActiveDrag())
            {
                CancelHoverTracking();
                return;
            }

            ref readonly var pointer = ref W.GetResource<TooltipPointerState>();
            if (pointer.IsTouchActive)
            {
                UpdateTouchHover(in pointer);
                return;
            }

            UpdateMouseHover(in pointer);
        }

        private void UpdateMouseHover(in TooltipPointerState pointer)
        {
            CancelTouchTracking();

            var movementThresholdSq = MovementResetThresholdPixels * MovementResetThresholdPixels;
            if (_hoverTooltip != null && pointer.ScreenDelta.sqrMagnitude <= movementThresholdSq)
            {
                return;
            }

            if (!TooltipTargetUtility.TryFindTopTooltipTarget(pointer, out var target))
            {
                CancelHoverTracking();
                return;
            }

            if (_hoverTooltip != null)
            {
                if (_hoverTarget == target.GID)
                {
                    return;
                }

                HideHoverTooltip();
                _hoverTarget = default;
                _hoverTimer = 0f;
            }

            if (_hoverTarget != target.GID)
            {
                _hoverTarget = target.GID;
                _hoverTimer = 0f;
            }

            if (pointer.ScreenDelta.sqrMagnitude > movementThresholdSq)
            {
                _hoverTimer = 0f;
            }
            else
            {
                _hoverTimer += W.GetResource<DeltaTime>().Value;
            }

            if (_hoverTimer < ShowDelaySeconds)
            {
                return;
            }

            _hoverTooltip = SpawnTooltip(target, pointer.ScreenPosition);
        }

        private void UpdateTouchHover(in TooltipPointerState pointer)
        {
            CancelHoverTracking();

            if (pointer.TouchBeganThisFrame)
            {
                _touchTarget = default;
                _touchTimer = 0f;
                HideTouchTooltip();

                if (TooltipTargetUtility.TryFindTopTooltipTarget(pointer, out var target))
                {
                    _touchTarget = target.GID;
                }

                return;
            }

            if (pointer.TouchEndedThisFrame)
            {
                CancelTouchTracking();
                return;
            }

            if (!_touchTarget.TryUnpack<WT>(out var touchTarget) || !touchTarget.Has<Tooltip>())
            {
                CancelTouchTracking();
                return;
            }

            if (_touchTooltip != null
                && pointer.ScreenDelta.sqrMagnitude <= MovementResetThresholdPixels * MovementResetThresholdPixels)
            {
                return;
            }

            if (!TooltipTargetUtility.TryFindTopTooltipTarget(pointer, out var currentTarget)
                || currentTarget.GID != _touchTarget)
            {
                CancelTouchTracking();
                return;
            }

            if (_touchTooltip != null)
            {
                return;
            }

            if (pointer.ScreenDelta.sqrMagnitude
                > MovementResetThresholdPixels * MovementResetThresholdPixels)
            {
                _touchTimer = 0f;
            }
            else
            {
                _touchTimer += W.GetResource<DeltaTime>().Value;
            }

            if (_touchTimer < ShowDelaySeconds)
            {
                return;
            }

            _touchTooltip = SpawnTooltip(touchTarget, pointer.ScreenPosition);
        }

        private TooltipView SpawnTooltip(W.Entity target, Vector2 screenPosition)
        {
            if (!target.Has<Tooltip>() || _tooltipCanvas == null)
            {
                return null;
            }

            ref readonly var tooltip = ref target.Read<Tooltip>();
            return _tooltipCanvas.SpawnTooltip(target, tooltip.Type, screenPosition);
        }

        private void CancelHoverTracking()
        {
            _hoverTarget = default;
            _hoverTimer = 0f;
            HideHoverTooltip();
        }

        private void CancelTouchTracking()
        {
            _touchTarget = default;
            _touchTimer = 0f;
            HideTouchTooltip();
        }

        private void HideHoverTooltip()
        {
            if (_hoverTooltip == null)
            {
                return;
            }

            _hoverTooltip.Hide();
            _hoverTooltip = null;
        }

        private void HideTouchTooltip()
        {
            if (_touchTooltip == null)
            {
                return;
            }

            _touchTooltip.Hide();
            _touchTooltip = null;
        }

        private static bool HasActiveDrag()
        {
            return W.Query<All<Dragging>>().EntitiesCount() > 0;
        }
    }
}
