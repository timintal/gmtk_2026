using _Game.Infrastructure.ECS;
using Code.Common.Cleanup;
using Code.Common.Physics;
using Code.Ecs;

namespace Code.Common
{
    public class CommonSystems : IFeature
    {
        public CommonSystems(ISystemFactory systems)
        {
            GameSys.Add(systems.Create<InitPositionFromTransformSystem>(), Order.Init);
            GameSys.Add(systems.Create<PreProcessCollisionEventSystem>(), Order.Init);
            
            GameSys.Add(systems.Create<TickDelaySystem>());
            
            GameSys.Add(systems.Create<AutoDestroyTickSystem>(), Order.PreCleanup);
            GameSys.Add(systems.Create<CleanUpPhysicsEventsSystem>(), Order.PreCleanup);
            
            GameSys.Add(systems.Create<CleanupChildrenForDestroyedEntitySystem>(), Order.Cleanup - 100);
            GameSys.Add(systems.Create<CleanupDestroyedEntitiesSystem>(), Order.Cleanup);
            
        }   
    }
}