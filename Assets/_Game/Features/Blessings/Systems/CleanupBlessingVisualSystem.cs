using Code.Common.View;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Blessings
{
    public class CleanupBlessingVisualSystem : ISystem
    {
        public void Update()
        {
            foreach (var e in W.Query<All<Blessing>, Any<ViewLink, ViewPrefab>, None<Hand, RewardScreen>>().Entities())
            {
                e.Set<NeedCleanupView>();
            }
        }
    }
}