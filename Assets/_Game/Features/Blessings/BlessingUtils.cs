using _Game.Features.Dice;
using Code.Common;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace _Game.Features.Blessings
{
    public static class BlessingUtils
    {
        public static void DiscardHand()
        {
            foreach (var e in W.Query<All<Blessing, Hand>>().Entities())
            {
                e.DiscardBlessing();
            }
        }
        
        public static void ShuffleDiscardPileToDrawPile()
        {
            foreach (var e in W.Query<All<Blessing, DiscardPile>>().Entities())
            {
                e.PutBlessingInDrawPile();
            }
        }
        
        public static void DiscardBlessing(this W.Entity e)
        {
            if (!e.Has<Blessing>()) return;

            e.Delete<Hand>();
            e.Delete<DrawPile>();
            e.Delete<RewardScreen>();
            e.Set<DiscardPile>();
        }
        
        public static void DrawBlessing(this W.Entity e)
        {
            if (!e.Has<Blessing>()) return;

            e.Delete<DiscardPile>();
            e.Delete<DrawPile>();
            e.Delete<RewardScreen>();
            e.Set<Hand>();

            if (e.Has<Position>())
            {
                e.Ref<Position>().Value = new Vector2(5, -10);
            }
        }
        
        public static void PutBlessingInDrawPile(this W.Entity e)
        {
            if (!e.Has<Blessing>()) return;

            e.Delete<Hand>();
            e.Delete<DiscardPile>();
            e.Delete<RewardScreen>();
            e.Set<DrawPile>();
        }

        public static void IncreaseUpgradesCount(this W.Entity entity)
        {
            if (entity.Has<UpgradesCount>())
            {
                ref var upgradesCount = ref entity.Mut<UpgradesCount>();
                upgradesCount.Value += 1;
            }
            else
            {
                entity.Set(new UpgradesCount {Value = 1});
            }
        }
    }
}