using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Common
{
    public class MoveToTargetSystem : ISystem
    {
        public void Update()
        {
            var dt = W.GetResource<DeltaTime>().Value;

            foreach (var e in W.Query<All<Position, Speed, MoveTarget>>().Entities())
            {
                ref var pos = ref e.Ref<Position>();
                ref readonly var speed = ref e.Read<Speed>();
                ref readonly var target = ref e.Read<MoveTarget>();

                var delta = target.Value - pos.Value;
                var distance = delta.magnitude;
                var step = speed.Value * dt;

                if (distance <= step || distance < 1e-4f)
                {
                    pos.Value = target.Value;
                    e.Delete<MoveTarget>();
                    if (e.Has<Direction>()) e.Ref<Direction>().Value = Vector2.zero;
                    continue;
                }

                var dir = delta / distance;
                pos.Value += dir * step;
                if (e.Has<Direction>()) e.Ref<Direction>().Value = dir;
            }
        }
    }
}
