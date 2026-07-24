using _Game.Features.Blessings.Views;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Blessings
{
    public class RefreshBlessingsLayoutSystem : ISystem
    {
        public void Update()
        {
            var added = W.Query<AllAdded<Blessing>>().EntitiesCount();
            var removed = W.Query<AllDeleted<Blessing>>().EntitiesCount();

            if (added + removed > 0 && W.GetResource<BlessingsContainerView>() != null)
            {
                W.GetResource<BlessingsContainerView>().Layout();
            }
        }
    }
}