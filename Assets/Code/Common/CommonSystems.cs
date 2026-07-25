using Code.Common.Cleanup;
using Code.Common.Physics;
using Code.Common.View;

namespace Code.Common
{
    public static class CommonSystems
    {
        public static void AddToWorld()
        {
            GameSys.Add(new InitPositionFromTransformSystem(), Order.Init);
            GameSys.Add(new PreProcessCollisionEventSystem(), Order.Init);
            
            GameSys.Add(new TickDelaySystem());
            
            GameSys.Add(new AutoDestroyTickSystem(), Order.PreCleanup);
            GameSys.Add(new CleanUpPhysicsEventsSystem(), Order.PreCleanup);
            
            GameSys.Add(new CleanupChildrenForDestroyedEntitySystem(), Order.Cleanup - 100);
            GameSys.Add(new CleanupDestroyedEntitiesSystem(), Order.Cleanup);
            ViewFeature.AddToWorld();
        }   
    }
}