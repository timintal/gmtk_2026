using Code.Common.View.ChildViews;
using Code.Features.DragAndDrop;
using FFS.Libraries.StaticEcs;

namespace Code.Common.View
{
    public class UpdateDraggingSortingSystem : ISystem
    {
        public void Update()
        {
            foreach (var e in W.Query<All<SortingGroupViewLink>>().Entities())
            {
                e.Read<SortingGroupViewLink>().Value.SetDragged(e.Has<Dragging>());
            }
        }
    }
}