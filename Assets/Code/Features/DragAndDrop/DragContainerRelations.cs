using Code.Common;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Features.DragAndDrop
{
    public static class DragContainerRelations
    {
        public static void PlaceInContainer(W.Entity draggable, W.Entity container)
        {
            draggable.Set(new W.Link<InDragContainer>(container));
            if (draggable.Has<SetAsContainerChild>())
            {
                TryReparentAsContainerChild(draggable, container);
            }
            
            W.SendEvent(new DragAccepted
            {
                Draggable = draggable.GID,
                TargetContainer = container
            });
        }

        public static void PlaceInContainer(W.Entity draggable, EntityGID container)
        {
            if (container.TryUnpack<WT>(out var entity))
            {
                PlaceInContainer(draggable, entity);
            }
        }

        public static bool TryGetContainer(W.Entity draggable, out EntityGID container)
        {
            if (!draggable.Has<W.Link<InDragContainer>>())
            {
                container = default;
                return false;
            }

            container = draggable.Read<W.Link<InDragContainer>>().Value;
            return container.Raw != 0UL;
        }

        public static bool HasContainer(W.Entity draggable) => draggable.Has<W.Link<InDragContainer>>();

        public static int CountItems(EntityGID container, EntityGID exclude = default)
        {
            if (!container.TryUnpack<WT>(out var containerEntity) || !containerEntity.Has<W.Links<DragContainerItems>>())
            {
                return 0;
            }

            ref readonly var items = ref containerEntity.Read<W.Links<DragContainerItems>>();
            if (exclude.Raw == 0UL)
            {
                return items.Length;
            }

            var count = 0;
            foreach (var item in items)
            {
                if (item.Value != exclude)
                {
                    count++;
                }
            }

            return count;
        }

        public static void MarkLayoutDirty(EntityGID container)
        {
            if (container.Raw == 0UL)
            {
                return;
            }

            if (container.TryUnpack<WT>(out var containerEntity))
            {
                containerEntity.Set<ContainerLayoutDirty>();
            }
        }

        private static void TryReparentAsContainerChild(W.Entity draggable, W.Entity container)
        {
            if (draggable.Has<TransformLink>() && container.Has<TransformLink>())
            {
                draggable.Read<TransformLink>().Value.SetParent(container.Read<TransformLink>().Value);
            }
        }
    }
}
