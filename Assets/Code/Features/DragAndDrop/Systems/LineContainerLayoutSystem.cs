using System.Collections.Generic;
using Code.Common;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Features.DragAndDrop
{
    public sealed class LineContainerLayoutSystem : ISystem
    {
        private readonly List<LayoutMember> _members = new();

        public void Update()
        {
            foreach (var container in W.Query<All<DragContainer, LineContainerLayout, Position, ContainerLayoutDirty>>().Entities())
            {
                ref readonly var position = ref container.Read<Position>();
                ref readonly var layout = ref container.Read<LineContainerLayout>();
                ApplyLayout(container, position.Value, layout);
                container.Delete<ContainerLayoutDirty>();
            }
        }

        private void ApplyLayout(W.Entity container, Vector2 containerPosition, in LineContainerLayout layout)
        {
            _members.Clear();

            if (!container.Has<W.Links<DragContainerItems>>())
            {
                return;
            }

            ref readonly var items = ref container.Read<W.Links<DragContainerItems>>();
            foreach (var itemLink in items)
            {
                if (!itemLink.Value.TryUnpack<WT>(out var draggable) || !draggable.Has<Position>())
                {
                    continue;
                }

                if (draggable.Has<Dragging>() || draggable.Has<Destroyed>())
                {
                    continue;
                }

                var order = draggable.Has<ContainerSlotIndex>()
                    ? draggable.Read<ContainerSlotIndex>().Order
                    : 0;
                _members.Add(new LayoutMember(draggable.GID.Id, order, draggable));
            }

            _members.Sort(static (a, b) =>
            {
                var order = a.Order.CompareTo(b.Order);
                return order != 0 ? order : a.EntityId.CompareTo(b.EntityId);
            });

            var usesLocalLayoutSpace = ContainerLayoutUtility.UsesLocalLayoutSpace(container);
            for (var i = 0; i < _members.Count; i++)
            {
                var localPosition = LineLayoutCalculator.GetLocalPosition(i, _members.Count, layout);
                _members[i].Entity.Ref<Position>()!.Value = ContainerLayoutUtility.ResolveMemberPosition(
                    containerPosition,
                    new Vector2(localPosition.x, localPosition.y),
                    usesLocalLayoutSpace);
            }
        }

        private readonly struct LayoutMember
        {
            public readonly uint EntityId;
            public readonly int Order;
            public readonly W.Entity Entity;

            public LayoutMember(uint entityId, int order, W.Entity entity)
            {
                EntityId = entityId;
                Order = order;
                Entity = entity;
            }
        }
    }
}
