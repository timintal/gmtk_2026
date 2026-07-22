using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Libraries.GameFlow.FSM
{
    public abstract class FSMCommand
    {
        protected readonly GameFSM parentFSM;

        public FSMCommand(GameFSM fsm)
        {
            parentFSM = fsm;
        }

        public abstract UniTask Execute();
        
        protected async UniTask ExitState(FSMStateBase stateToPop)
        {
            await stateToPop.OnDisable();
            await stateToPop.OnExit();
            stateToPop.cancellationSource.Cancel();
            stateToPop.cancellationSource.Dispose();
            stateToPop.cancellationSource = null;
            stateToPop.UnloadResources().Forget();
        }
        
        public override string ToString() => GetType().Name;
    }

    public class PushStateCommand : FSMCommand
    {
        private readonly FSMStateBase stateToPush;
        private readonly IStateProperties properties;

        public PushStateCommand(GameFSM fsm, FSMStateBase stateToPush, IStateProperties properties) : base(fsm)
        {
            this.stateToPush = stateToPush;
            this.properties = properties;
        }

        public override async UniTask Execute()
        {
            stateToPush.SetProperties(properties);
            var hasCurrentState = parentFSM.statesStack.TryPeek(out var currentState);

            if (hasCurrentState && currentState == stateToPush)
            {
                Debug.LogError("Trying to push the same state " + stateToPush.GetType().Name);
                return;
            }

            stateToPush.cancellationSource = new CancellationTokenSource();

            var loadResourcesTask = stateToPush.LoadResources();

            
            if (hasCurrentState)
            {
                await currentState.OnDisable();
            }
            
            await loadResourcesTask;

            await stateToPush.OnEnter();
            parentFSM.statesStack.Push(stateToPush);
            await stateToPush.OnEnable();
            parentFSM.currentState = stateToPush;
        }

        public override string ToString() => $"{GetType().Name} ({stateToPush})";
    }

    public class PopStateCommand : FSMCommand
    {
        public PopStateCommand(GameFSM fsm) : base(fsm) { }

        public override async UniTask Execute()
        {
            if (parentFSM.statesStack.TryPop(out var stateToPop))
            {
                await ExitState(stateToPop);
            }

            if (parentFSM.statesStack.TryPeek(out var currentState))
            {
                await currentState.OnEnable();
            }
            
            parentFSM.currentState = currentState;
        }
    }

    public class ClearStackCommand : FSMCommand
    {
        public ClearStackCommand(GameFSM fsm) : base(fsm) { }

        public override async UniTask Execute()
        {
            while (parentFSM.statesStack.TryPop(out var stateToPop))
            {
                await ExitState(stateToPop);
            }
            parentFSM.currentState = null;
        }
    }
}