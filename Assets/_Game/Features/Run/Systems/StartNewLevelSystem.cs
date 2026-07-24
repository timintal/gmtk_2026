using _Game.Features.Blessings;
using _Game.Features.Dice;
using _Game.Features.Run.Configs;
using _Game.Features.Run.View;
using Code.Common;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace _Game.Features.Run
{
    public class StartNewLevelSystem : ISystem
    {
        public void Update()
        {
            var requestQuery = W.Query<All<StartNewLevelRequest>>();
            if (requestQuery.EntitiesCount() == 0) return;
            
            requestQuery.BatchDestroy();

            foreach (var e in W.Query<All<Blessing>>().Entities())
            {
                e.PutBlessingInDrawPile();
            }
            foreach (var e in W.Query<All<Dice.Dice>>().Entities())
            {
                e.Set<Destroyed>();
            }
            
            PlayerState playerState = W.GetResource<PlayerState>();
            playerState.CurrentLevel++;
            W.NewEntity<Default>().Set<StartNewTurnRequest>();

            var encountersConfig = W.GetResource<EncountersConfig>();
            var randomEncounter = encountersConfig.GetRandomEncounter(playerState.CurrentLevel);
            var container = W.GetResource<EnemiesContainer>().Container;
            foreach (var enemyPrefab in randomEncounter.Enemies)
            {
                Object.Instantiate(enemyPrefab, container);
            }
            
            W.NewEntity<Default>().Set<LevelStarted>();
        }
    }
}