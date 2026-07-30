using _Game.Features.Blessings.Views;
using _Game.Infrastructure.ECS;
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
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(new MainCamera { Value = _mainCamera }).As<MainCamera>();
            builder.RegisterEntryPoint<EcsBootstrap>();
            builder.Register<ISystemFactory, SystemFactory>(Lifetime.Singleton);
            builder.RegisterComponent(_blessingsContainerView).AsSelf();
            builder.RegisterComponent(_tooltipCanvas).AsSelf();
            
            GeneratedSystemRegistrar.BindAll(builder);
            
        }
    }
}