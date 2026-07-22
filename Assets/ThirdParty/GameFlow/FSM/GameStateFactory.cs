// using VContainer;

using System;
using System.Collections.Generic;

namespace Libraries.GameFlow.FSM
{
    // public class GameStateFactory : IGameStateFactory
    // {
    //     private readonly IObjectResolver _resolver;
    //     public GameStateFactory(IObjectResolver resolver)
    //     {
    //         _resolver = resolver;
    //     }
    //
    //     public T GetState<T>() where T : FSMStateBase
    //     {
    //         return _resolver.Resolve<T>();
    //     }
    // }
    
    public class CustomGameStateFactory : IGameStateFactory
    {
        public Dictionary<Type, FSMStateBase> states = new();
        public T GetState<T>() where T : FSMStateBase
        {
            if (states.TryGetValue(typeof(T), out var state))
            {
                return (T)state;
            }
            var newState = Activator.CreateInstance<T>();
            states[typeof(T)] = newState;
            return newState;
        }
    }
}