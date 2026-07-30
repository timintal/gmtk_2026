using _Game.Infrastructure.ECS;
using Code.Common;
using Code.Ecs;

namespace _Game.Features.Run
{
    public class RunFeature : IFeature
    {
        public RunFeature(ISystemFactory systems)
        {
            GameSys.Add(systems.Create<InitRunSystem>(), Order.Init);
            GameSys.Add(systems.Create<StartNewLevelSystem>(), Order.Init + 1);
            GameSys.Add(systems.Create<StartNewTurnSystem>(), Order.Init + 2);
            GameSys.Add(systems.Create<LevelFinishedSystem>(), Order.LateUpdate);
            GameSys.Add(systems.Create<EndTurnSystem>(), Order.LateUpdate);
            GameSys.Add(systems.Create<ShowRewardsSystem>(), Order.LateUpdate);
            GameSys.Add(systems.Create<DrawCardSystem>(), Order.LateUpdate);
            
        }
    }
}