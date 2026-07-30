using _Game.Features.Blessings.Views;
using Code.Common;
using Code.Common.View;
using Code.Configs;
using FFS.Libraries.StaticEcs;
using VContainer;

namespace _Game.Features.Blessings
{
    public class AddBlessingsVisualSystem : ISystem
    {
        [Inject] internal VisualConfig _visualConfig;
        [Inject] internal BlessingsContainerView _blessingsContainerView;
        
        public void Update()
        {
            foreach (var e in W.Query<All<Blessing, Hand>, None<ViewPrefab>>().Entities())
            {
                e.Set(new ViewPrefab
                {
                    Prefab = _visualConfig.BlessingCardPrefab
                });
                e.Set(new ParentTransform()
                {
                    Value = _blessingsContainerView.Root
                });
            }
        }
    }
}