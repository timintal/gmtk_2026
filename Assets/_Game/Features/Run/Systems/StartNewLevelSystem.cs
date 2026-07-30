using _Game.Features.Blessings;
using _Game.Features.Dice;
using _Game.Features.Enemies;
using _Game.Features.Run.Configs;
using _Game.Features.Run.View;
using _Game.Infrastructure.Factories;
using Code.Common;
using FFS.Libraries.StaticEcs;
using UnityEngine;
using VContainer;

namespace _Game.Features.Run
{
    public class StartNewLevelSystem : ISystem
    {
        [Inject] internal EncountersConfig _encountersConfig;
        [Inject] internal EnemiesFactory _enemiesFactory;
        
        public void Update()
        {
            var requestQuery = W.Query<All<StartNewLevelRequest>, None<Delay>>();
            if (requestQuery.EntitiesCount() == 0) return;

            requestQuery.BatchDestroy();

            foreach (var e in W.Query<All<Blessing>, None<RewardScreen>>().Entities())
            {
                e.PutBlessingInDrawPile();
            }
            foreach (var e in W.Query<All<Dice.Dice>>().Entities())
            {
                e.Set<Destroyed>();
            }

            PlayerState playerState = W.GetResource<PlayerState>();
            playerState.CurrentLevel++;

            var newLevelRequest = W.NewEntity<Default>();
            newLevelRequest.Set<StartNewTurnRequest>();

            var randomEncounter = _encountersConfig.GetRandomEncounter(playerState.CurrentLevel);
            var container = W.GetResource<EnemiesContainer>().Container;

            foreach (var enemyData in randomEncounter.Enemies)
            {
                var enemy = _enemiesFactory.CreateEnemy(enemyData, container);
                var enemyEntity = enemy.Entity;
                if (enemyEntity.Has<W.Links<CountdownModifiers>>())
                {
                    AddModifiers(enemyEntity, enemyData);
                    enemyEntity.Set(new Attack()
                    {
                        BaseValue = enemyData.Attack,
                        CurrentValue = enemyData.Attack
                    });
                    enemyEntity.Set(new EnemyCountdown
                    {
                        Value = enemyData.CurrentCountdown
                    });
                }
            }

            W.NewEntity<Default>().Set<LevelStarted>();
        }
        private static void AddModifiers(World<WT>.Entity enemyEntity, EnemySettings enemyData)
        {
            ref var modifiers = ref enemyEntity.Ref<W.Links<CountdownModifiers>>()!;
            modifiers.Clear();
            foreach (var modifier in enemyData.CountdownModifiers)
            {
                var modifierEntity = W.NewEntity<Default>();
                modifierEntity.Set(new CountdownModifier()
                {
                    Type = modifier.Type,
                });
                if (modifier.Type == CountdownModifierType.Even)
                {
                    modifierEntity.Set<AcceptOnlyEven>();
                    modifiers.Add(modifierEntity);
                }
                else if (modifier.Type == CountdownModifierType.Odd)
                {
                    modifierEntity.Set<AcceptOnlyOdd>();
                }
                else if (modifier.Type == CountdownModifierType.Smaller)
                {
                    modifierEntity.Set(new AcceptSmaller()
                    {
                        Value = modifier.Value
                    });
                    modifiers.Add(modifierEntity);
                }
                else if (modifier.Type == CountdownModifierType.Bigger)
                {
                    modifierEntity.Set(new AcceptBigger()
                    {
                        Value = modifier.Value
                    });
                    modifiers.Add(modifierEntity);
                }
            }
        }
    }
}