using _Game.Features.Blessings;
using _Game.Features.Enemies;
using Code.Common;
using Code.Features.GameLoop;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Run
{
    public class LevelFinishedSystem : ISystem
    {
        public void Update()
        {
            if (W.Query<All<LevelStarted>>().EntitiesCount() == 0 ||
                W.Query<All<ActiveTurn>>().EntitiesCount() == 0 ||
                W.Query<All<GameLost>>().EntitiesCount() > 0)
                return;
            
            if (W.Query<All<Enemy>>().EntitiesCount() == 0)
            {
                W.Query<All<LevelStarted>>().BatchDestroy();
                W.Query<All<ActiveTurn>>().BatchDestroy();
                W.Query<All<Dice.Dice>>().BatchSet(new Destroyed());
                
                BlessingUtils.DiscardHand();
                BlessingUtils.ShuffleDiscardPileToDrawPile();
                
                var newLevelRequest = W.NewEntity<Default>();
                newLevelRequest.Set<ShowRewardsRequest>();
                newLevelRequest.Set(new Delay() { Value = 1f });
                
            }
        }
    }
}