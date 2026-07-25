using _Game.Features.Dice;
using Code.Common;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace _Game.Features.Blessings
{
    public class ActivateMultiplyBlessingSystem : ISystem
    {
        public void Update()
        {
            foreach (var e in W.Query<All<Blessing, Activated, MultiplyValueBlessing, BlessingValue, W.Links<Targets>>>().Entities())
            {
                ref var targets = ref e.Ref<W.Links<Targets>>();
                if (targets.IsNotEmpty)
                {
                    e.Set<UsedBlessing>();
                }
                
                foreach (var targetLink in targets)
                {
                    if (targetLink.Value.TryUnpack<WT>(out var entity) && 
                        entity.Has<DiceValue>())
                    {
                        ref var diceValue = ref entity.Mut<DiceValue>();
                        diceValue.Value = Mathf.RoundToInt(diceValue.Value * e.Read<BlessingValue>().Value);
                        entity.IncreaseUpgradesCount();
                    }
                }
            }
        }
    }
}