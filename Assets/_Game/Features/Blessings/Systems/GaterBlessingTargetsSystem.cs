using _Game.Features.Dice;
using Code.Common;
using Code.Common.Hitbox;
using Code.Features.DragAndDrop;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Blessings
{
    public class GaterBlessingTargetsSystem : ISystem
    {
        public void Update()
        {
            foreach (var e in W.Query<All<Blessing, Activated, Position>>().Entities())
            {
                var position = e.Read<Position>();
                e.Set(new W.Links<Targets>());
                ref var links = ref e.Ref<W.Links<Targets>>();
                
                if (e.Has<AffectAllBlessing>())
                {
                    foreach (var container in W.Query<All<RolledDicesContainer, DragContainerHitbox2D>>().Entities())
                    {
                        var collider2D = container.Read<DragContainerHitbox2D>().Value;
                        if (collider2D.OverlapPoint(position.Value))
                        {
                            foreach (var diEntity in W.Query<All<Dice.Dice, DiceValue>>().Entities())
                            {
                                links.TryAdd(diEntity);
                            }
                            break;
                        }
                    }
                }

                foreach (var targetEntity in W.Query<All<BlessingTarget, Hitbox2D>>().Entities())
                {
                    var collider2D = targetEntity.Read<Hitbox2D>().Value;
                    if (collider2D.OverlapPoint(position.Value))
                    {
                        if (e.Has<DiceBlessing>() && targetEntity.Has<DiceValue>())
                        {
                            bool isValidTarget = true;
                            var diceValue = targetEntity.Read<DiceValue>().Value;

                            if (isValidTarget)
                            {
                                isValidTarget = !e.Has<EvenBlessing>() || diceValue % 2 == 0;
                            }
                            if (isValidTarget)
                            {
                                isValidTarget = !e.Has<OddBlessing>() || diceValue % 2 == 1;
                            }

                            if (isValidTarget)
                            {
                                links.TryAdd(targetEntity);

                                if (e.Has<AffectSameValueDicesBlessing>())
                                {
                                    foreach (var diEntity in W.Query<All<Dice.Dice, DiceValue>>().Entities())
                                    {
                                        if (diEntity.Read<DiceValue>().Value == diceValue)
                                            links.TryAdd(diEntity);
                                    }
                                }
                            }
                        }

                        break;
                    }
                }
            }
        }
    }
}