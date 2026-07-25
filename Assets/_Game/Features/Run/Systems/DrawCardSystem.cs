using _Game.Features.Blessings;
using Code.Common;
using Code.Common.Utils;
using FFS.Libraries.StaticEcs;
using UnityEngine.Pool;

namespace _Game.Features.Run
{
    public class DrawCardSystem : ISystem
    {
        public void Update()
        {
            foreach (var e in W.Query<All<DrawCardRequest>>().Entities())
            {
                e.Set<Destroyed>();
                var drawRequest = e.Read<DrawCardRequest>();
                for (int i = 0; i < drawRequest.Value; i++)
                {
                    DrawCard();
                }
            }
        }

        bool DrawCard()
        {
            ListPool<W.Entity>.Get(out var cards);
            foreach (var e in W.Query<All<Blessing, DrawPile>, None<UsedBlessing>>().Entities())
            {
                cards.Add(e);
            }

            if (cards.Count > 0)
            {
                cards.Shuffle();
            }
            else
            {
                BlessingUtils.ShuffleDiscardPileToDrawPile();
                foreach (var e in W.Query<All<Blessing, DrawPile>, None<UsedBlessing>>().Entities())
                {
                    cards.Add(e);
                }
            }

            if (cards.Count > 0)
            {
                cards[0].DrawBlessing();
            }

            ListPool<W.Entity>.Release(cards);
            return cards.Count > 0;
        }
    }
}