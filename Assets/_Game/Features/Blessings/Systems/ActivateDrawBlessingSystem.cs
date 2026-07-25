using _Game.Features.Dice;
using Code.Common;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace _Game.Features.Blessings
{
    public class ActivateDrawBlessingSystem : ISystem
    {
        public void Update()
        {
            foreach (var e in W.Query<All<Blessing, Activated, DrawBlessing, BlessingValue>>().Entities())
            {
                e.Set<UsedBlessing>();

                PlayerState playerState = W.GetResource<PlayerState>();
                playerState.NextDrawCount += Mathf.RoundToInt(e.Read<BlessingValue>().Value);
            }
        }
    }
}