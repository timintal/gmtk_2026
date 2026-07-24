using _Game.Features.Dice;
using Code.Common;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace _Game.Features.Blessings
{
    public class ActivateRerollBlessingSystem : ISystem
    {
        public void Update()
        {
            foreach (var e in W.Query<All<Blessing, Activated, RerollBlessing, BlessingValue, Position, W.Links<Targets>>>().Entities())
            {
                if (e.Read<Position>().Value.y < 0)
                {
                    continue; //skip activation
                }

                if (e.Has<AffectAllBlessing>())
                {
                    foreach (var diceEntity in W.Query<All<Dice.Dice, DiceValue>>().Entities())
                    {
                        diceEntity.Mut<DiceValue>().Value = Random.Range(1, 7);
                    }
                }
                else
                {
                    W.NewEntity<Default>().Set(new RerollRequest
                    {
                        ClearCurrent = false,
                        DiceCount = Mathf.RoundToInt(e.Read<BlessingValue>().Value),
                    });
                }

                e.DiscardBlessing();
            }
        }
    }
}