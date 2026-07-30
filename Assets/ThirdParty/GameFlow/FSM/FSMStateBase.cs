using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameFlow.FSM
{
    public class FSMState : FSMState<IStateProperties> {}
    public class FSMState<TProperties> : FSMStateBase where TProperties : IStateProperties
    {
        public virtual bool PropertiesRequired { get; } = false;
        public TProperties Properties { get; private set; }

        public override void SetProperties(IStateProperties properties)
        {
            if (properties != null)
            {
                if (properties is TProperties p)
                {
                    Properties = p;
                }
                else throw new ArgumentException($"Properties must be of type {typeof(TProperties).Name}");
            }
            else if (PropertiesRequired)
            {
                throw new ArgumentException($"Properties are required for state {GetType().Name}");
            }
        }
    }
    
    public abstract class FSMStateBase : IDisposable
    {
        protected internal CancellationTokenSource cancellationSource;

        
        public CancellationTokenSource CancellationSource => cancellationSource;
        
        public virtual UniTask OnEnter() => UniTask.CompletedTask;
        public virtual UniTask OnExit() => UniTask.CompletedTask;
        public virtual UniTask OnEnable() => UniTask.CompletedTask;
        public virtual UniTask OnDisable() => UniTask.CompletedTask;
        
        public virtual void OnUpdate() { }

        public virtual UniTask LoadResources() => UniTask.CompletedTask;
        public virtual UniTask UnloadResources() => UniTask.CompletedTask;
        
        public abstract void SetProperties(IStateProperties properties);
        
        public virtual void Dispose()
        {
            if (cancellationSource != null && !cancellationSource.IsCancellationRequested)
            {
                cancellationSource.Cancel();
                cancellationSource.Dispose();
            }
        }

        public override string ToString() => GetType().Name;
    }
}
