using _Game.Features.Dice;
using Code.Common;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace _Game.Features.Blessings
{
    public class ActivateAddBlessingSystem : ISystem
    {
        public void Update()
        {
            foreach (var e in W.Query<All<Blessing, Activated, AddValueBlessing, BlessingValue, W.Links<Targets>>>().Entities())
            {
                ref var targets = ref e.Ref<W.Links<Targets>>();
                foreach (var targetLink in targets)
                {
                    if (targetLink.Value.TryUnpack<WT>(out var entity) && 
                        entity.Has<DiceValue>())
                    {
                        ref var diceValue = ref entity.Mut<DiceValue>();
                        diceValue.Value += Mathf.RoundToInt(e.Read<BlessingValue>().Value);
                    }
                }
            }
        }
    }
}