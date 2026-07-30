using _Game.Features.Dice.Systems;
using _Game.Infrastructure.ECS;
using Code.Ecs;

namespace _Game.Features.Dice
{
    public  class DiceFeature : IFeature
    {
        public DiceFeature(ISystemFactory systems)
        {
            GameSys.Add(systems.Create<RefreshDieViewSystem>());
            GameSys.Add(systems.Create<PerformRerollSystem>());
        }
    }
}