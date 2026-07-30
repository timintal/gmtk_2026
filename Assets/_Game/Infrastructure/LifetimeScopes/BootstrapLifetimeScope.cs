using _Game.Infrastructure.Utils;
using _Game.Infrastructure.Persistence;
using Code.Common.Audio;
using Code.GameFlow;
using GameFlow.FSM;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Game.Infrastructure.LifetimeScopes
{
    public class BootstrapLifetimeScope : LifetimeScope
    {
        [SerializeField] private ScriptableObject[] _configs;
        [SerializeField] private GenericAudioSource[] _audioSources;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<AutoInjectFactory>(Lifetime.Scoped).AsSelf();
            builder.Register<PoolService>(Lifetime.Singleton).AsSelf();

            RegisterPersistentData(builder);
            RegisterScriptableObjectInstances(builder);
            RegisterFSM(builder);
            RegisterAudioSources(builder);
            
            builder.RegisterBuildCallback(OnBuildFinished);
        }
        
        
        private void RegisterAudioSources(IContainerBuilder builder)
        {
            foreach (var audioSource in _audioSources)
            {
                builder.RegisterInstance(audioSource).AsSelf();
            }
        }
        
        private void RegisterFSM(IContainerBuilder builder)
        {
            builder.Register<GameFSM>(Lifetime.Singleton).AsSelf();
            builder.Register<GameStateFactory>(Lifetime.Singleton).As<IGameStateFactory>();

        
            
            builder.Register<MainGameState>(Lifetime.Singleton).As<FSMStateBase>().AsSelf();
            builder.Register<GameOverState>(Lifetime.Singleton).As<FSMStateBase>().AsSelf();
            builder.Register<RunGameState>(Lifetime.Singleton).As<FSMStateBase>().AsSelf();
            builder.Register<SettingsState>(Lifetime.Singleton).As<FSMStateBase>().AsSelf();
        }

        void RegisterPersistentData(IContainerBuilder builder)
        {
            GeneratedPersistentDataRegistrar.Register(builder);
            builder.Register<PlayerPrefsDataHandler>(Lifetime.Singleton)
                .As<IPersistentDataHandler>();
            builder.RegisterEntryPoint<PersistentDataManager>();
        }

        private void RegisterScriptableObjectInstances(IContainerBuilder builder)
        {
            foreach (var so in _configs)
            {
                builder.RegisterInstance(so).AsSelf();
            }
        }
        private void OnBuildFinished(IObjectResolver resolver)
        {
            var gameFsm = resolver.Resolve<GameFSM>();
            gameFsm.Push<MainGameState>();
        }
    }
}
