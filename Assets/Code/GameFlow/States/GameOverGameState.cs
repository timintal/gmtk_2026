using Cysharp.Threading.Tasks;
using Libraries.GameFlow.FSM;
using UnityEngine.Scripting;

namespace Code.GameFlow
{
    [Preserve]
    public class GameOverProperties : IStateProperties
    {
        public string Reason;
    }
    
    public class GameOverGameState : FSMState<GameOverProperties>
    {
        public override UniTask OnEnter()
        {
            W.GetResource<GameOverScreen>().gameObject.SetActive(true);
            W.DestroyAllLoadedEntities();
            return UniTask.CompletedTask;
        }

        public override UniTask OnExit()
        {
            return UniTask.CompletedTask;
        }
    }
}
