using Code.Ecs;
using Code.Features.GameLoop;
using Code.GameFlow;
using FFS.Libraries.StaticEcs;

namespace Code.Features.EnergyFeature
{
    public class CheckEnergySystem : ISystem
    {
        public void Update()
        {
            if (W.Query<Any<RoundWon, GameLost>>().EntitiesCount() > 0)
                return;
            
            W.Query<All<Energy>>().For(e =>
            {
                var energy = e.Read<Energy>();
                if (energy.Value <= 0)
                {
                    W.NewEntity<Default>().Set<GameLost>();
                    W.GetResource<FSM>().Value.Push<GameOverState>();
                }
            });
        }
    }
}