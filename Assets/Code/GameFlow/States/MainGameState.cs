using Cysharp.Threading.Tasks;
using GameFlow.FSM;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Code.GameFlow
{
    [UnityEngine.Scripting.Preserve]
    public class MainGameState : FSMState
    {
        [Inject] internal GameFSM _fsm;
        
        public override async UniTask OnEnter()
        {
            await SceneManager.LoadSceneAsync("main", LoadSceneMode.Additive);
            
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("main"));
            
            _fsm.Push<RunGameState>();
        }
        
        public override async UniTask OnExit()
        {
            await SceneManager.UnloadSceneAsync("main");
        }
    }
}