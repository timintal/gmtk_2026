using Code.Common;
using Code.Common.Hitbox;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Features.DragAndDrop
{
    public sealed class DragInputSystem : ISystem
    {
        private const int PrimaryPointerId = 0;

        private EventReceiver<WT, DragPointerDownEvent> _downEvents;
        private EventReceiver<WT, DragPointerUpEvent> _upEvents;
        private bool _initialized;

        public void Init()
        {
            _downEvents = W.RegisterEventReceiver<DragPointerDownEvent>();
            _upEvents = W.RegisterEventReceiver<DragPointerUpEvent>();
            _initialized = true;
        }

        public void Update()
        {
            foreach (var dragEvent in _downEvents)
            {
                ref readonly var pointerEvent = ref dragEvent.Value;
                TryBeginDrag(pointerEvent);
            }

            foreach (var dragEvent in _upEvents)
            {
                ref readonly var pointerEvent = ref dragEvent.Value;
                TryEndDrag(pointerEvent);
            }
        }

        public void Destroy()
        {
            if (!_initialized || W.Status != WorldStatus.Initialized)
            {
                return;
            }

            W.DeleteEventReceiver(ref _downEvents);
            W.DeleteEventReceiver(ref _upEvents);
            _initialized = false;
        }

        private static void TryBeginDrag(in DragPointerDownEvent pointerEvent)
        {
            if (pointerEvent.PointerId != PrimaryPointerId || HasActiveDrag())
            {
                return;
            }

            if (!TryFindTopDraggable(pointerEvent, out var draggable, out var usesScreenSpace))
            {
                return;
            }

            ref readonly var originPosition = ref draggable.Read<Position>();
            if (!TryGetPointerPosition(draggable, pointerEvent, usesScreenSpace, out var pointerPosition))
            {
                return;
            }

            var sourceContainer = default(EntityGID);
            var hasSourceContainer = false;
            if (DragContainerRelations.TryGetContainer(draggable, out sourceContainer))
            {
                hasSourceContainer = true;
            }

            draggable.Set(new Dragging
            {
                PointerId = PrimaryPointerId,
                SourceContainer = sourceContainer,
                HasSourceContainer = hasSourceContainer,
                OriginPosition = originPosition.Value,
                PointerOffset = originPosition.Value - pointerPosition,
                UsesScreenSpace = usesScreenSpace
            });
            draggable.Set<SkipSyncViewPositionDamping>();
        }

        private static void TryEndDrag(in DragPointerUpEvent pointerEvent)
        {
            if (pointerEvent.PointerId != PrimaryPointerId)
            {
                return;
            }

            foreach (var draggable in W.Query<All<Draggable, Dragging>>().Entities())
            {
                ref readonly var dragging = ref draggable.Read<Dragging>();
                if (dragging.PointerId != pointerEvent.PointerId)
                {
                    continue;
                }

                var targetContainer = default(EntityGID);
                var hasTarget = TryFindTopContainer(pointerEvent, dragging.UsesScreenSpace, out targetContainer);

                draggable.Delete<SkipSyncViewPositionDamping>();

                W.NewEntity<Default>().Set(new DragTransferRequest
                {
                    Draggable = draggable.GID,
                    SourceContainer = dragging.SourceContainer,
                    TargetContainer = targetContainer,
                    HasSourceContainer = dragging.HasSourceContainer,
                    HasTargetContainer = hasTarget,
                });
                return;
            }
        }

        private static bool HasActiveDrag()
        {
            foreach (var draggable in W.Query<All<Dragging>>().Entities())
            {
                if (draggable.Read<Dragging>().PointerId == PrimaryPointerId)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryFindTopDraggable(in DragPointerDownEvent pointerEvent, out W.Entity hit, out bool usesScreenSpace)
        {
            if (TryFindTopUiDraggable(pointerEvent.ScreenPosition, out hit))
            {
                usesScreenSpace = true;
                return true;
            }

            usesScreenSpace = false;
            if (pointerEvent.IsBlockedByUi || !pointerEvent.HasWorldPosition)
            {
                hit = default;
                return false;
            }

            return TryFindTopWorldDraggable(pointerEvent.WorldPosition, out hit);
        }

        private static bool TryFindTopWorldDraggable(Vector2 world, out W.Entity hit)
        {
            hit = default;
            var bestPriority = int.MinValue;
            var bestEntityId = uint.MinValue;

            foreach (var entity in W.Query<All<Draggable, Hitbox2D, Position>>().Entities())
            {
                ref readonly var draggable = ref entity.Read<Draggable>();
                ref readonly var hitbox = ref entity.Read<Hitbox2D>();
                if (draggable.Disabled || hitbox.Value == null || entity.Has<Dragging>())
                {
                    continue;
                }

                if (!hitbox.Value.OverlapPoint(world))
                {
                    continue;
                }

                if (draggable.Priority < bestPriority)
                {
                    continue;
                }

                if (draggable.Priority == bestPriority && entity.GID.Id <= bestEntityId)
                {
                    continue;
                }

                hit = entity;
                bestPriority = draggable.Priority;
                bestEntityId = entity.GID.Id;
            }

            return bestPriority != int.MinValue;
        }

        private static bool TryFindTopUiDraggable(Vector2 screen, out W.Entity hit)
        {
            hit = default;
            var bestPriority = int.MinValue;
            var bestEntityId = uint.MinValue;

            foreach (var entity in W.Query<All<Draggable, HitboxUi, Position>>().Entities())
            {
                ref readonly var draggable = ref entity.Read<Draggable>();
                ref readonly var hitbox = ref entity.Read<HitboxUi>();
                if (draggable.Disabled || hitbox.Value == null || entity.Has<Dragging>())
                {
                    continue;
                }

                if (!DragUiUtility.ContainsScreenPoint(hitbox.Value, screen))
                {
                    continue;
                }

                if (draggable.Priority < bestPriority)
                {
                    continue;
                }

                if (draggable.Priority == bestPriority && entity.GID.Id <= bestEntityId)
                {
                    continue;
                }

                hit = entity;
                bestPriority = draggable.Priority;
                bestEntityId = entity.GID.Id;
            }

            return bestPriority != int.MinValue;
        }

        private static bool TryFindTopContainer(in DragPointerUpEvent pointerEvent, bool usesScreenSpace, out EntityGID hit)
        {
            if (usesScreenSpace)
            {
                return TryFindTopUiContainer(pointerEvent.ScreenPosition, out hit);
            }

            if (pointerEvent.IsBlockedByUi || !pointerEvent.HasWorldPosition)
            {
                hit = default;
                return false;
            }

            return TryFindTopWorldContainer(pointerEvent.WorldPosition, out hit);
        }

        private static bool TryFindTopWorldContainer(Vector2 world, out EntityGID hit)
        {
            hit = default;
            var bestPriority = int.MinValue;
            var bestEntityId = uint.MinValue;

            foreach (var entity in W.Query<All<DragContainer, DragContainerHitbox2D>>().Entities())
            {
                ref readonly var container = ref entity.Read<DragContainer>();
                ref readonly var hitbox = ref entity.Read<DragContainerHitbox2D>();
                if (container.Disabled || hitbox.Value == null)
                {
                    continue;
                }

                if (!hitbox.Value.OverlapPoint(world))
                {
                    continue;
                }

                if (container.Priority < bestPriority)
                {
                    continue;
                }

                if (container.Priority == bestPriority && entity.GID.Id <= bestEntityId)
                {
                    continue;
                }

                hit = entity.GID;
                bestPriority = container.Priority;
                bestEntityId = entity.GID.Id;
            }

            return bestPriority != int.MinValue;
        }

        private static bool TryFindTopUiContainer(Vector2 screen, out EntityGID hit)
        {
            hit = default;
            var bestPriority = int.MinValue;
            var bestEntityId = uint.MinValue;

            foreach (var entity in W.Query<All<DragContainer, DragContainerHitboxUi>>().Entities())
            {
                ref readonly var container = ref entity.Read<DragContainer>();
                ref readonly var hitbox = ref entity.Read<DragContainerHitboxUi>();
                if (container.Disabled || hitbox.Value == null)
                {
                    continue;
                }

                if (!DragUiUtility.ContainsScreenPoint(hitbox.Value, screen))
                {
                    continue;
                }

                if (container.Priority < bestPriority)
                {
                    continue;
                }

                if (container.Priority == bestPriority && entity.GID.Id <= bestEntityId)
                {
                    continue;
                }

                hit = entity.GID;
                bestPriority = container.Priority;
                bestEntityId = entity.GID.Id;
            }

            return bestPriority != int.MinValue;
        }

        private static bool TryGetPointerPosition(W.Entity draggable, in DragPointerDownEvent pointerEvent, bool usesScreenSpace, out Vector2 pointerPosition)
        {
            if (!usesScreenSpace)
            {
                pointerPosition = pointerEvent.WorldPosition;
                return pointerEvent.HasWorldPosition;
            }

            if (!draggable.Has<HitboxUi>())
            {
                pointerPosition = default;
                return false;
            }

            return DragUiUtility.TryScreenToLocalPosition(
                draggable.Read<HitboxUi>().Value,
                pointerEvent.ScreenPosition,
                out pointerPosition);
        }
    }
}
