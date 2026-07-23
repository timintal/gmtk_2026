using Code.Common;
using FFS.Libraries.StaticEcs;

namespace Code.Features.DragAndDrop
{
    /// <summary>
    /// Releases destroyed draggables from their container before the drag pipeline runs.
    /// Deleting the <see cref="InDragContainer"/> link fires its OnDelete hook, which removes the
    /// item from the container's <c>DragContainerItems</c> and marks the container layout dirty.
    /// This frees capacity and reflows survivors in the same frame the item is marked
    /// <see cref="Destroyed"/>, instead of waiting for <c>CleanupDestroyedEntitiesSystem</c>.
    /// </summary>
    public sealed class ReleaseDestroyedContainerItemsSystem : ISystem
    {
        public void Update()
        {
            foreach (var draggable in W.Query<All<Destroyed>>().Entities())
            {
                if (draggable.Has<W.Link<InDragContainer>>())
                {
                    draggable.Delete<W.Link<InDragContainer>>();
                }
            }
        }
    }
}
