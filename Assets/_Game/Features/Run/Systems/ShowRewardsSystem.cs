using _Game.UI;
using Code.Common;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Run
{
    public class ShowRewardsSystem : ISystem
    {
        public void Update()
        {
            var requestQuery = W.Query<All<ShowRewardsRequest>, None<Delay>>();
            if (requestQuery.EntitiesCount() == 0) 
                return;
            
            requestQuery.BatchDestroy();
            W.GetResource<RewardsSelection>().ShowRewards(3);
        }
    }
}