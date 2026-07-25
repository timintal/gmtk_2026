using System;
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
        public async override UniTask OnEnter()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2));
            W.GetResource<GameOverScreen>().gameObject.SetActive(true);
            W.DestroyAllLoadedEntities();
        }

        public override UniTask OnExit()
        {
            return UniTask.CompletedTask;
        }
    }
}
