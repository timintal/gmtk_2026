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
                
                if (e.Has<UsedBlessing>())
                {
                    e.DiscardBlessing();
                    e.Delete<UsedBlessing>();
                }
                else if (e.Read<W.Links<Targets>>().Length == 0)
                {
                    W.GetResource<ErrorMessage>().Show("No Targets");
                }
            }
        }
    }
}