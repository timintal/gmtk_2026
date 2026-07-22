using Cysharp.Threading.Tasks;
using Libraries.GameFlow.FSM;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

namespace Code.GameFlow
{
    [Preserve]
    public class MainGameState : FSMState
    {
        public override async UniTask OnEnter()
        {
            await SceneManager.LoadSceneAsync("main", LoadSceneMode.Additive);
        }
        
        public override async UniTask OnExit()
        {
            await SceneManager.UnloadSceneAsync("main");
        }
    }
}