using System.Collections.Generic;
using Code.Common;
using FFS.Libraries.StaticEcs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Code.Features.DragAndDrop
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-8500)]
    public sealed class DragPointerInputBridge : MonoBehaviour
    {
        private const int PrimaryPointerId = 0;

        private readonly List<RaycastResult> _uiRaycastResults = new();

        private EventSystem _eventSystem;
        private InputAction _positionAction;
        private InputAction _pressAction;
        private PointerEventData _pointerEventData;
        private bool _pressed;
        private Vector2 _lastScreenPosition;

        private void OnEnable()
        {
            _positionAction = new InputAction("DragPointerPosition", InputActionType.PassThrough, "<Pointer>/position");
            _pressAction = new InputAction("DragPointerPress", InputActionType.Button, "<Pointer>/press");

            _positionAction.performed += OnPositionPerformed;
            _pressAction.started += OnPressStarted;
            _pressAction.canceled += OnPressCanceled;

            _positionAction.Enable();
            _pressAction.Enable();
        }

        private void OnDisable()
        {
            _pressed = false;

            if (_positionAction != null)
            {
                _positionAction.performed -= OnPositionPerformed;
                _positionAction.Dispose();
                _positionAction = null;
            }

            if (_pressAction != null)
            {
                _pressAction.started -= OnPressStarted;
                _pressAction.canceled -= OnPressCanceled;
                _pressAction.Dispose();
                _pressAction = null;
            }
        }

        private void OnPressStarted(InputAction.CallbackContext context)
        {
            ReadPointer(out var screen, out var world, out var hasWorld);

            _pressed = true;
            _lastScreenPosition = screen;
            W.SendEvent(new DragPointerDownEvent
            {
                PointerId = PrimaryPointerId,
                ScreenPosition = screen,
                WorldPosition = world,
                HasWorldPosition = hasWorld,
                IsBlockedByUi = IsBlockedByUi(screen)
            });
        }

        private void OnPressCanceled(InputAction.CallbackContext context)
        {
            ReadPointer(out var screen, out var world, out var hasWorld);

            _lastScreenPosition = screen;
            W.SendEvent(new DragPointerUpEvent
            {
                PointerId = PrimaryPointerId,
                ScreenPosition = screen,
                WorldPosition = world,
                HasWorldPosition = hasWorld,
                IsBlockedByUi = IsBlockedByUi(screen)
            });
            _pressed = false;
        }

        private void OnPositionPerformed(InputAction.CallbackContext context)
        {
            var screen = context.ReadValue<Vector2>();
            var delta = screen - _lastScreenPosition;
            _lastScreenPosition = screen;

            if (!_pressed)
            {
                return;
            }

            var hasWorld = TryScreenToWorld(screen, out var world);

            W.SendEvent(new DragPointerMoveEvent
            {
                PointerId = PrimaryPointerId,
                ScreenPosition = screen,
                ScreenDelta = delta,
                WorldPosition = world,
                HasWorldPosition = hasWorld,
                IsBlockedByUi = IsBlockedByUi(screen)
            });
        }

        private void ReadPointer(out Vector2 screen, out Vector2 world, out bool hasWorld)
        {
            screen = _positionAction != null ? _positionAction.ReadValue<Vector2>() : default;
            hasWorld = TryScreenToWorld(screen, out world);
        }

        private static bool TryScreenToWorld(Vector2 screen, out Vector2 world)
        {
            world = default;
            if (W.Status != WorldStatus.Initialized || !W.HasResource<MainCamera>())
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

        private bool IsBlockedByUi(Vector2 screen)
        {
            var currentEventSystem = EventSystem.current;
            if (currentEventSystem == null)
            {
                return false;
            }

            if (_pointerEventData == null || _eventSystem != currentEventSystem)
            {
                _eventSystem = currentEventSystem;
                _pointerEventData = new PointerEventData(currentEventSystem);
            }

            _pointerEventData.Reset();
            _pointerEventData.position = screen;
            _pointerEventData.pointerId = PrimaryPointerId;
            _uiRaycastResults.Clear();
            currentEventSystem.RaycastAll(_pointerEventData, _uiRaycastResults);

            for (var i = 0; i < _uiRaycastResults.Count; i++)
            {
                if (_uiRaycastResults[i].module is GraphicRaycaster)
                {
                    return true;
                }
            }

            return false;
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
