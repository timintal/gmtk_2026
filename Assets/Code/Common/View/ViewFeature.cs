using _Game.Infrastructure.ECS;
using Code.Ecs;

namespace Code.Common.View
{
    public class ViewFeature : IFeature
    {
        public ViewFeature(ISystemFactory systems)
        {
            GameSys.Add(systems.Create<SyncPositionFromTransformSystem>(), Order.Init);
            
            GameSys.Add(systems.Create<CreateViewSystem>(), Order.LateUpdate);
            GameSys.Add(systems.Create<BindViewSystem>(), Order.LateUpdate + 1);
            GameSys.Add(systems.Create<SyncTransformFromPositionSystem>(), Order.LateUpdate);
            GameSys.Add(systems.Create<WorldSpaceUiFollowSystem>(), Order.LateUpdate);
            GameSys.Add(systems.Create<InitRigidbodySystem>(), Order.LateUpdate);
            GameSys.Add(systems.Create<InitializeViewSystem>(), Order.LateUpdate);
            GameSys.Add(systems.Create<UpdateDraggingSortingSystem>(), Order.LateUpdate);
            GameSys.Add(systems.Create<CleanupViewSystem>(), Order.Cleanup - 10);
        }
    }
}