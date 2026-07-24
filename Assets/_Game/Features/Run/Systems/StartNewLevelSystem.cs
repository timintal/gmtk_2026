using _Game.Features.Dice;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Run
{
    public class StartNewLevelSystem : ISystem
    {
        public void Update()
        {
            var requestQuery = W.Query<All<StartNewLevelRequest>>();
            if (requestQuery.EntitiesCount() == 0) return;
            
            requestQuery.BatchDestroy();
            
            PlayerState playerState = W.GetResource<PlayerState>();
            playerState.RestoreBlessings();
            playerState.CurrentLevel++;
            W.NewEntity<Default>().Set<StartNewTurnRequest>();
        }
    }
}