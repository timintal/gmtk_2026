using _Game.Features.Blessings;
using _Game.Features.Dice;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Run
{
    public class StartNewTurnSystem : ISystem
    {
        public void Update()
        {
            var requestQuery = W.Query<All<StartNewTurnRequest>>();
            if (requestQuery.EntitiesCount() == 0) return;
            
            requestQuery.BatchDestroy();

            foreach (var e in W.Query<All<Blessing, Hand>>().Entities())
            {
                e.DiscardBlessing();
            }
            PlayerState playerState = W.GetResource<PlayerState>();
            W.NewEntity<Default>().Set(new DrawCardRequest(){Value = playerState.DrawPerTurn});
        }
    }
}