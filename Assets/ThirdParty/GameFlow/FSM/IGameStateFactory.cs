namespace Libraries.GameFlow.FSM
{
    public interface IGameStateFactory
    {
        T GetState<T>() where T : FSMStateBase;
    }
}