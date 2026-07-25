using _Game.Features.Blessings;
using _Game.Features.Enemies;
using _Game.Features.PlayerControls;
using Code.Common;
using Code.Features.EnergyFeature;
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
            
            foreach (var enemy in W.Query<All<Enemy, Attack>>().Entities())
            {
                foreach (var player in W.Query<All<Player, Energy>>().Entities())
                {
                    player.Mut<Energy>().Value -= Mathf.RoundToInt(enemy.Mut<Attack>().CurrentValue);
                }
            }

            var newTurnRequest = W.NewEntity<Default>();
            newTurnRequest.Set<StartNewTurnRequest>();
            newTurnRequest.Set(new Delay() { Value = 1f });
        }
    }
}