using Code.Common;

namespace _Game.Features.Run
{
    public class RunFeature
    {
        public static void AddToWorld()
        {
            GameSys.Add(new InitRunSystem(), Order.Init);
            GameSys.Add(new StartNewLevelSystem(), Order.Init + 1);
            GameSys.Add(new StartNewTurnSystem(), Order.Init + 2);
            GameSys.Add(new LevelFinishedSystem(), Order.LateUpdate);
            GameSys.Add(new EndTurnSystem(), Order.LateUpdate);
            GameSys.Add(new ShowRewardsSystem(), Order.LateUpdate);
            GameSys.Add(new DrawCardSystem(), Order.LateUpdate);
            
        }
    }
}