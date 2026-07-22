using FFS.Libraries.StaticEcs;

namespace Code.Common
{
    public class AutoDestroyTickSystem : ISystem
    {
        public void Update()
        {
            var dt = W.GetResource<DeltaTime>().Value;
            foreach (var e in W.Query<All<AutoDestroy>>().Entities())
            {
                ref var autoDestroy = ref e.Ref<AutoDestroy>();
                autoDestroy.Delay -= dt;
                if (autoDestroy.Delay <= 0)
                {
                    e.Set<Destroyed>();
                }
            }
        }
    }
}