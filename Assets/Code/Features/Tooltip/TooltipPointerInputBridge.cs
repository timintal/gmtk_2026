using System.Collections.Generic;
using Code.Common;
using FFS.Libraries.StaticEcs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Code.Features.Tooltip
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-10050)]
    public sealed class TooltipPointerInputBridge : MonoBehaviour
    {
        private const int PrimaryPointerId = 0;

        private EventSystem _eventSystem;
        private InputAction _positionAction;
        private InputAction _pressAction;
        private PointerEventData _pointerEventData;
        private bool _touchWasActive;
        private Vector2 _lastScreenPosition;
        private bool _hasLastScreenPosition;

        private void OnEnable()
        {
            _positionAction = new InputAction("TooltipPointerPosition", InputActionType.PassThrough, "<Pointer>/position");
            _pressAction = new InputAction("TooltipPointerPress", InputActionType.Button, "<Pointer>/press");
            _positionAction.Enable();
            _pressAction.Enable();
        }

        private void OnDisable()
        {
            _touchWasActive = false;
            _hasLastScreenPosition = false;

            _positionAction?.Disable();
            _pressAction?.Disable();
            _positionAction?.Dispose();
            _pressAction?.Dispose();
            _positionAction = null;
            _pressAction = null;
        }

        private void Update()
        {
            if (W.Status != WorldStatus.Initialized || !W.HasResource<TooltipPointerState>())
            {
                return;
            }

            ref var state = ref W.GetResource<TooltipPointerState>();
            var previousScreenPosition = state.ScreenPosition;
            var touchActive = IsTouchActive();

            state.TouchBeganThisFrame = touchActive && !_touchWasActive;
            state.TouchEndedThisFrame = !touchActive && _touchWasActive;
            state.IsTouchActive = touchActive;
            state.ScreenPosition = ReadScreenPosition(touchActive);
            state.ScreenDelta = _hasLastScreenPosition ? state.ScreenPosition - previousScreenPosition : Vector2.zero;
            PopulateUiRaycast(state.ScreenPosition, out state.IsBlockedByUi);
            state.HasWorldPosition = TryScreenToWorld(state.ScreenPosition, out state.WorldPosition);

            _touchWasActive = touchActive;
            _lastScreenPosition = state.ScreenPosition;
            _hasLastScreenPosition = true;
        }

        private Vector2 ReadScreenPosition(bool touchActive)
        {
            if (touchActive && Touchscreen.current != null)
            {
                return Touchscreen.current.primaryTouch.position.ReadValue();
            }

            if (_positionAction != null)
            {
                return _positionAction.ReadValue<Vector2>();
            }

            return _hasLastScreenPosition ? _lastScreenPosition : Vector2.zero;
        }

        private static bool IsTouchActive()
        {
            return Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;
        }

        private void PopulateUiRaycast(Vector2 screen, out bool isBlockedByUi)
        {
            isBlockedByUi = false;
            if (!W.HasResource<TooltipUiRaycastCache>())
            {
                return;
            }

            var results = W.GetResource<TooltipUiRaycastCache>().Results;
            results.Clear();

            var currentEventSystem = EventSystem.current;
            if (currentEventSystem == null)
            {
                return;
            }

            if (_pointerEventData == null || _eventSystem != currentEventSystem)
            {
                _eventSystem = currentEventSystem;
                _pointerEventData = new PointerEventData(currentEventSystem);
            }

            _pointerEventData.Reset();
            _pointerEventData.position = screen;
            _pointerEventData.pointerId = PrimaryPointerId;
            currentEventSystem.RaycastAll(_pointerEventData, results);

            for (var i = 0; i < results.Count; i++)
            {
                if (results[i].module is GraphicRaycaster)
                {
                    isBlockedByUi = true;
                    return;
                }
            }
        }

        private static bool TryScreenToWorld(Vector2 screen, out Vector2 world)
        {
            world = default;
            if (!W.HasResource<MainCamera>())
            {
                return false;
            }

            var camera = W.GetResource<MainCamera>().Value;
            if (camera == null)
            {
                return false;
            }

            world = ScreenToWorld2D(camera, screen);
            return true;
        }

        private static Vector2 ScreenToWorld2D(Camera camera, Vector2 screen)
        {
            if (camera.orthographic)
            {
                var world = camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -camera.transform.position.z));
                return new Vector2(world.x, world.y);
            }

            var ray = camera.ScreenPointToRay(screen);
            var distance = Mathf.Abs(ray.direction.z) > 0.0001f ? -ray.origin.z / ray.direction.z : 0f;
            var point = ray.GetPoint(distance);
            return new Vector2(point.x, point.y);
        }
    }
}
