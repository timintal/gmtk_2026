using _Game.Infrastructure.ECS;
using Code.Common;
using Code.Ecs;
using Code.Features.DragAndDrop;
using FFS.Libraries.StaticEcs;

namespace Code.Features.Tooltip
{
    public class TooltipFeature : IFeature
    {
        public TooltipFeature(ISystemFactory systems)
        {
            GameSys.Add(systems.Create<TooltipHoverSystem>(), (short)(Order.Input + 10));
        }
        

        public static bool IsEnabled()
        {
            return W.Status == WorldStatus.Initialized &&
                   W.Query<All<Dragging>>().EntitiesCount() == 0;
        }
    }
}
