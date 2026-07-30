using _Game.Infrastructure.ECS;
using Code.Ecs;

namespace Code.Common.Fx
{
    public  class FxFeature : IFeature
    {
        public  FxFeature(ISystemFactory systems)
        {
            GameSys.Add(systems.Create<PlayOneShotParticlesSystem>(), Order.LateUpdate);
        }
        
    }
}