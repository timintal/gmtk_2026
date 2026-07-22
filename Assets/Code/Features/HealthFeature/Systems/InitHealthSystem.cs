using FFS.Libraries.StaticEcs;

namespace Code.Features.HealthFeature.Systems
{
    public class InitHealthSystem : ISystem
    {
        public void Update()
        {
            W.Query<All<InitHealthRequest>>().For(static (W.Entity e, ref Health h, in MaxHealth mh) =>
            {
                h.Value = mh.Value;
                e.Delete<InitHealthRequest>();
            });
        }
    }
}