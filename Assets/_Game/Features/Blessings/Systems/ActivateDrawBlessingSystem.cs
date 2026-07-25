using _Game.Features.Run;
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
                
                W.NewEntity<Default>().Set(new DrawCardRequest()
                {
                    Value = Mathf.RoundToInt(e.Read<BlessingValue>().Value)
                });
            }
        }
    }
}