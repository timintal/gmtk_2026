using FFS.Libraries.StaticEcs;

namespace Code.Features.Stats
{
    public abstract class ResetStatSystem<T> : ISystem where T : struct, IStat
    {
        public void Update()
        {
            // Remember each stat's current (last frame's final) value before overwriting it, so the
            // apply system can tell afterwards whether the recomputed value actually changed.
            // Ref<> is used deliberately: resetting must NOT mark the stat as Changed on its own.
            var previous = StatChangeBuffer<T>.Previous;
            previous.Clear();
            foreach (var e in W.Query<All<T>>().Entities())
            {
                ref var stat = ref e.Ref<T>();
                previous[e.ID] = stat.CurrentValue;
                stat.CurrentValue = stat.BaseValue;
            }
        }
    }
}
