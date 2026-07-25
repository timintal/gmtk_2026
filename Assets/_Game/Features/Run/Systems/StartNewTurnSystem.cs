using _Game.Features.Dice;
using Code.Common;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Run
{
    public class StartNewTurnSystem : ISystem
    {
        public void Update()
        {
            var requestQuery = W.Query<All<StartNewTurnRequest>, None<Delay>>();
            if (requestQuery.EntitiesCount() == 0) return;
            
            requestQuery.BatchDestroy();
            
            PlayerState playerState = W.GetResource<PlayerState>();
            W.NewEntity<Default>().Set(new DrawCardRequest(){Value = playerState.NextDrawCount});
            playerState.NextDrawCount = playerState.BaseDrawPerTurn;
            
            W.NewEntity<Default>().Set<ActiveTurn>();
        }
    }
}