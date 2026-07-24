using Code.Common;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Blessings
{
    public class CleanupActivatedBlessing : ISystem
    {
        public void Update()
        {
            foreach (var e in W.Query<All<Blessing, Activated, W.Links<Targets>>>().Entities())
            {
                e.Delete<Activated>();
                ref var links = ref e.Ref<W.Links<Targets>>();
                if (links.IsNotEmpty)
                {
                    e.DiscardBlessing();
                }
            }
        }
    }
}