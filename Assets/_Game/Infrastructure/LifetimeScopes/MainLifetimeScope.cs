using _Game.Features.Blessings.Views;
using _Game.Infrastructure.ECS;
using _Game.Infrastructure.Factories;
using _Game.Infrastructure.Utils;
using _Game.UI;
using Code.Common;
using Code.Common.View.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Game.Infrastructure.LifetimeScopes
{
    public class MainLifetimeScope : LifetimeScope
    {
        [SerializeField] Camera _mainCamera;
        [SerializeField] BlessingsContainerView _blessingsContainerView;
        [SerializeField] TooltipCanvas _tooltipCanvas;
        [SerializeField] RewardsSelection _rewardsSelection;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<AutoInjectFactory>(Lifetime.Scoped).AsSelf();
            
            builder.RegisterInstance(new MainCamera { Value = _mainCamera }).As<MainCamera>();
            builder.RegisterEntryPoint<EcsBootstrap>();
            builder.Register<ISystemFactory, SystemFactory>(Lifetime.Singleton);
            builder.RegisterComponent(_blessingsContainerView).AsSelf();
            builder.RegisterComponent(_tooltipCanvas).AsSelf();
            builder.RegisterComponent(_rewardsSelection).AsSelf();
            
            RegisterFactories(builder);
            
            GeneratedSystemRegistrar.BindAll(builder);
        }
        
        void RegisterFactories(IContainerBuilder builder)
        {
            builder.Register<BlessingViewFactory>(Lifetime.Singleton);
            builder.Register<EnemiesFactory>(Lifetime.Singleton);
        }
    }
}