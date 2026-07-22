using Code.Common;
using FFS.Libraries.StaticEcs;

namespace Code.Features.HealthFeature.Systems
{
    public class KillZeroHealthEntities : ISystem
    {
        public void Update()
        {
            W.Query().For(static (W.Entity e, in Health h) =>
            {
                if (h.Value <= 0)
                    e.Set<Destroyed>();
            });
        }
    }
}