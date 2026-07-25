using _Game.Features.Blessings;
using _Game.Features.Enemies;
using _Game.Features.PlayerControls;
using Code.Common;
using Code.Configs;
using Code.Features.EnergyFeature;
using Code.Features.EnergyFeature.View;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace _Game.Features.Run
{
    public class EndTurnSystem : ISystem
    {
        public void Update()
        {
            if (W.Query<All<EndTurnRequest>>().EntitiesCount() == 0)
                return;
            
            W.Query<All<EndTurnRequest>>().BatchDestroy();
            W.Query<All<ActiveTurn>>().BatchDestroy();
            
            BlessingUtils.DiscardHand();

            var visualConfig = W.GetResource<VisualConfig>();

            Vector2 barPosition = Vector2.zero;
            foreach (var bar in W.Query<All<EnergyProgressViewLink, Player>>().Entities())
            {
                barPosition = bar.Read<EnergyProgressViewLink>().Value.transform.position;
                break;
            }
            
            
            float delay = 0f;
            foreach (var enemy in W.Query<All<Enemy, Attack>>().Entities())
            {
                if (enemy.Has<AttackViewLink>())
                {
                    var lightning = Object.Instantiate(visualConfig.LightningPrefab);
                    lightning.PlayLightning(barPosition, enemy.Read<AttackViewLink>().Value.transform.position, 0.2f, delay + 0.3f);
                    
                    enemy.Read<AttackViewLink>().Value.PlayAttackAnimation(delay);
                    delay += 0.3f;
                }
                foreach (var player in W.Query<All<Player, Energy>>().Entities())
                {
                    player.Mut<Energy>().Value -= Mathf.RoundToInt(enemy.Mut<Attack>().CurrentValue);
                    if (player.Mut<Energy>().Value < 0)
                        player.Mut<Energy>().Value = 0;
                }
            }

            var newTurnRequest = W.NewEntity<Default>();
            newTurnRequest.Set<StartNewTurnRequest>();
            newTurnRequest.Set(new Delay() { Value = 1f });
        }
    }
}