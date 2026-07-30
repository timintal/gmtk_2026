using _Game.UI;
using Code.Common;
using FFS.Libraries.StaticEcs;
using VContainer;

namespace _Game.Features.Run
{
    public class ShowRewardsSystem : ISystem
    {
        [Inject] internal RewardsSelection _rewardsSelection;
        public void Update()
        {
            var requestQuery = W.Query<All<ShowRewardsRequest>, None<Delay>>();
            if (requestQuery.EntitiesCount() == 0) 
                return;
            
            requestQuery.BatchDestroy();
            _rewardsSelection.ShowRewards(3);
        }
    }
}