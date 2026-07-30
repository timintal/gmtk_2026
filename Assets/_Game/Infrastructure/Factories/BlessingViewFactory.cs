using Code.Common.View;
using Code.Configs;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Game.Infrastructure.Factories
{
    public class BlessingViewFactory
    {
        private readonly IObjectResolver _resolver;
        private readonly VisualConfig _visualConfig;

        public BlessingViewFactory(IObjectResolver resolver, VisualConfig visualConfig)
        {
            _resolver = resolver;
            _visualConfig = visualConfig;
        }
        
        public EntityView Create(Transform parent)
        {
            return _resolver.Instantiate(_visualConfig.BlessingCardPrefab, parent);
        }
    }
}