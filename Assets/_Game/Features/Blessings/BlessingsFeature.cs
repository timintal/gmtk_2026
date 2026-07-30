using _Game.Infrastructure.ECS;
using Code.Common;
using Code.Ecs;

namespace _Game.Features.Blessings
{
    public class BlessingsFeature : IFeature
    {
        public BlessingsFeature(ISystemFactory systems)
        {
            GameSys.Add(systems.Create<RefreshBlessingsLayoutSystem>(), Order.PreUpdate);
            
            GameSys.Add(systems.Create<GaterBlessingTargetsSystem>());
            
            GameSys.Add(systems.Create<ActivateAddBlessingSystem>(), Order.Update + 1);
            GameSys.Add(systems.Create<ActivateMultiplyBlessingSystem>(), Order.Update + 1);
            GameSys.Add(systems.Create<ActivateRerollBlessingSystem>(), Order.Update + 1);
            GameSys.Add(systems.Create<ActivateDrawBlessingSystem>(), Order.Update + 1);
            
            GameSys.Add(systems.Create<AddBlessingsVisualSystem>(), Order.Cleanup);
            GameSys.Add(systems.Create<CleanupBlessingVisualSystem>(), Order.Cleanup);
            
            GameSys.Add(systems.Create<CleanupActivatedBlessing>(), Order.Cleanup);
        }
    }
}