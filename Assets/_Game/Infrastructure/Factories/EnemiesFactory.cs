using _Game.Features.Run.Configs;
using Code.Configs;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Game.Infrastructure.Factories
{
    public class EnemiesFactory
    {
        private readonly IObjectResolver _resolver;
        private readonly VisualConfig _visualConfig;
        public EnemiesFactory(IObjectResolver resolver, VisualConfig visualConfig)
        {
            _resolver = resolver;
            _visualConfig = visualConfig;
        }
        
        public WTEntityProvider CreateEnemy(EnemySettings settings, Transform parent)
        {
            return _resolver.Instantiate(settings.Prefab, parent);
        }
    }
}