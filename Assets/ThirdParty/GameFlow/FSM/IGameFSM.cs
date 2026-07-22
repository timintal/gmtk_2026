using Cysharp.Threading.Tasks;

namespace Libraries.GameFlow.FSM
{
    public interface IGameFSM
    {
        void Push<T>(IStateProperties properties = null) where T : FSMStateBase;
        void ClearAndPush<T>(IStateProperties properties = null) where T : FSMStateBase;
        void PopState();
        void PopUntil<T>();
        void ClearStateStack();
        UniTask PushAndWaitForExit<T>(IStateProperties properties = null) where T : FSMStateBase;
        void SwitchToState<T>(IStateProperties properties = null) where T : FSMStateBase;
    }
}