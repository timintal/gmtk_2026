using System;
using Code.Ecs;
using FFS.Libraries.StaticEcs;
using VContainer;

namespace _Game.Infrastructure.ECS
{
    public interface ISystemFactory
    {
        T Create<T>() where T : ISystem;
        T CreateFeature<T>() where T : IFeature;
        ISystem CreateSystem(Type systemType);
    }
    
    public class SystemFactory : ISystemFactory
    {
        private readonly IObjectResolver _container;

        public SystemFactory(IObjectResolver container) =>
            _container = container;

        public T Create<T>() where T : ISystem
        {
            return _container.Resolve<T>();
        }

        public ISystem CreateSystem(Type systemType)
        {
            return _container.Resolve(systemType) as ISystem;
        }
        
        public T CreateFeature<T>() where T : IFeature
        {
            return _container.Resolve<T>();
        }
    }}