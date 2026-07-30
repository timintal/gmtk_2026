using _Game.Infrastructure.ECS;
using Code.Common;
using Code.Ecs;
using Code.Features.HealthFeature.Systems;

namespace Code.Features.HealthFeature
{
    public class HealthFeature : IFeature
    {
        public HealthFeature(ISystemFactory systems)
        {
            GameSys.Add(systems.Create<InitHealthSystem>(), Order.PostInit + 1);
            
            GameSys.Add(systems.Create<ApplyDamageToHealthSystem>(), Order.PreCleanup - 1);
            GameSys.Add(systems.Create<KillZeroHealthEntities>(), Order.PreCleanup);
        }
    }
}