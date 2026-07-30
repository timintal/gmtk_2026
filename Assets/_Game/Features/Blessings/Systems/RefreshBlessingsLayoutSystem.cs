using _Game.Features.Blessings.Views;
using FFS.Libraries.StaticEcs;
using VContainer;

namespace _Game.Features.Blessings
{
    public class RefreshBlessingsLayoutSystem : ISystem
    {
        [Inject] internal BlessingsContainerView _blessingsContainerView;
        
        public void Update()
        {
            var added = W.Query<AllAdded<Blessing>>().EntitiesCount();
            var removed = W.Query<AllDeleted<Blessing>>().EntitiesCount();
            
            if (added + removed > 0 && _blessingsContainerView != null)
            {
                _blessingsContainerView.Layout();
            }
        }
    }
}