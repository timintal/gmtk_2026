using _Game.Features.Blessings.Views;
using Code.Common;
using Code.Common.View;
using Code.Configs;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Blessings
{
    public class AddBlessingsVisualSystem : ISystem
    {
        public void Update()
        {
            if (!W.HasResource<BlessingsContainerView>())
                return;
            
            foreach (var e in W.Query<All<Blessing, Hand>, None<ViewPrefab>>().Entities())
            {
                e.Set(new ViewPrefab
                {
                    Prefab = W.GetResource<VisualConfig>().BlessingCardPrefab
                });
                e.Set(new ParentTransform()
                {
                    Value = W.GetResource<BlessingsContainerView>().Root
                });
            }
        }
    }
}