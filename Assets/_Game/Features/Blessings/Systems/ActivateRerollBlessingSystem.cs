using _Game.Features.Dice;
using Code.Common;
using Code.Features.DragAndDrop;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace _Game.Features.Blessings
{
    public class ActivateRerollBlessingSystem : ISystem
    {
        public void Update()
        {
            // foreach (var e in W.Query<All<Blessing, Activated, RerollBlessing, W.Links<Targets>>>().Entities())
            // {
            //     foreach (var diceEntity in W.Query<All<Dice.Dice, DiceValue>, None<UpgradesCount>>().Entities())
            //     {
            //         diceEntity.Mut<DiceValue>().Value = Random.Range(1, 7);
            //     }
            // }

            foreach (var e in W.Query<All<Blessing, Activated, RerollBlessing, BlessingValue, Position, W.Links<Targets>>>().Entities())
            {
                var pos = e.Read<Position>().Value;
                bool canRoll = false;
                foreach (var diceContainer in W.Query<All<RolledDicesContainer, DragContainerHitbox2D>>().Entities())
                {
                    if (diceContainer.Read<DragContainerHitbox2D>().Value.OverlapPoint(pos))
                    {
                        canRoll = true;
                        break;
                    }
                }

                if (!canRoll && pos.y > 4)
                {
                    canRoll = true;
                }

                if (!canRoll)
                    continue;


                bool used = false;
                if (e.Has<AffectAllBlessing>())
                {
                    foreach (var diceEntity in W.Query<All<Dice.Dice, DiceValue>, None<UpgradesCount>>().Entities())
                    {
                        used = true;
                        diceEntity.Mut<DiceValue>().Value = Random.Range(1, 7);
                    }
                }
                else
                {
                    used = true;
                    W.NewEntity<Default>().Set(new RerollRequest
                    {
                        ClearCurrent = false,
                        DiceCount = Mathf.RoundToInt(e.Read<BlessingValue>().Value),
                    });
                }

                if (used)
                {
                    e.Set<UsedBlessing>();
                }
            }
        }
    }
}