using FFS.Libraries.StaticEcs;

namespace Code.Features.Stats
{
    public abstract class ResetStatSystem<T> : ISystem where T : struct, IStat
    {
        public void Update()
        {
            foreach (var e in W.Query<All<T>>().Entities())
            {
                ref var stat = ref e.Ref<T>();
                stat.CurrentValue = stat.BaseValue;
            }
        }
    }
}