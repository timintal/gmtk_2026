using Code.Common;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Features.DragAndDrop
{
    public sealed class DragFollowSystem : ISystem
    {
        private const int PrimaryPointerId = 0;

        private EventReceiver<WT, DragPointerMoveEvent> _moveEvents;
        private bool _initialized;

        public void Init()
        {
            _moveEvents = W.RegisterEventReceiver<DragPointerMoveEvent>();
            _initialized = true;
        }

        public void Update()
        {
            foreach (var dragEvent in _moveEvents)
            {
                ref readonly var pointerEvent = ref dragEvent.Value;
                FollowPointer(pointerEvent);
            }
        }

        public void Destroy()
        {
            if (!_initialized || W.Status != WorldStatus.Initialized)
            {
                return;
            }

            W.DeleteEventReceiver(ref _moveEvents);
            _initialized = false;
        }

        private static void FollowPointer(in DragPointerMoveEvent pointerEvent)
        {
            if (pointerEvent.PointerId != PrimaryPointerId)
            {
                return;
            }

            foreach (var draggable in W.Query<All<Dragging, Position>>().Entities())
            {
                ref readonly var dragging = ref draggable.Read<Dragging>();
                if (dragging.PointerId != pointerEvent.PointerId)
                {
                    continue;
                }

                ref var position = ref draggable.Ref<Position>();
                if (dragging.UsesScreenSpace)
                {
                    

                    if (!draggable.Has<TransformLink>() ||
                        !DragUiUtility.TryScreenToLocalPosition(
                            draggable.Read<TransformLink>().Value.GetComponent<RectTransform>(),
                            pointerEvent.ScreenPosition,
                            out var localPosition))
                    {
                        continue;
                    }

                    position.Value = localPosition + dragging.PointerOffset;
                    continue;
                }

                if (!pointerEvent.HasWorldPosition)
                {
                    continue;
                }

                position.Value = pointerEvent.WorldPosition + dragging.PointerOffset;
            }
        }
    }
}
