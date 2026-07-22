using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Common
{
    public class LerpToTargetSystem : ISystem
    {
        private const float LerpSpeed = 10;
        private const float LerpThreshold = 0.1f;
        
        public void Update()
        {
            var dt = W.GetResource<DeltaTime>().Value;

            foreach (var e in W.Query<All<Position, LerpTarget>>().Entities())
            {
                ref var pos = ref e.Ref<Position>();
                ref readonly var target = ref e.Read<LerpTarget>();
                
                pos.Value = Vector2.Lerp(pos.Value, target.Value, dt * LerpSpeed);

                var distance = (pos.Value - target.Value).sqrMagnitude;
                
                if (distance < LerpThreshold)
                {
                    pos.Value = target.Value;
                    e.Delete<LerpTarget>();
                }
            }
        }
    }
}