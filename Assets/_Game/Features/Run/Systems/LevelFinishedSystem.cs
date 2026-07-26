using _Game.Features.Blessings;
using _Game.Features.Dice;
using _Game.Features.Enemies;
using _Game.Features.Run.Configs;
using Code.Common;
using Code.Ecs;
using Code.Features.GameLoop;
using Code.GameFlow;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Run
{
    public class LevelFinishedSystem : ISystem
    {
        public void Update()
        {
            if (W.Query<All<LevelStarted>>().EntitiesCount() == 0 ||
                W.Query<All<ActiveTurn>>().EntitiesCount() == 0 ||
                W.Query<All<GameLost>>().EntitiesCount() > 0 ||
                W.Query<All<GameWon>>().EntitiesCount() > 0)
                return;
            
            if (W.Query<All<Enemy>>().EntitiesCount() == 0)
            {
                var playerState = W.GetResource<PlayerState>();
                if (playerState.CurrentLevel >= W.GetResource<EncountersConfig>().BossLevel)
                {
                    W.GetResource<FSM>().Value.Push<GameWonState>();
                    W.NewEntity<Default>().Set<GameWon>();
                    return;
                }
                
                W.Query<All<LevelStarted>>().BatchDestroy();
                W.Query<All<ActiveTurn>>().BatchDestroy();
                W.Query<All<Dice.Dice>>().BatchSet(new Destroyed());
                
                BlessingUtils.DiscardHand();
                BlessingUtils.ShuffleDiscardPileToDrawPile();
                
                playerState.NextDrawCount = playerState.BaseDrawPerTurn;
                
                var newLevelRequest = W.NewEntity<Default>();
                newLevelRequest.Set<ShowRewardsRequest>();
                newLevelRequest.Set(new Delay() { Value = 0.4f });
                
            }
        }
    }
}