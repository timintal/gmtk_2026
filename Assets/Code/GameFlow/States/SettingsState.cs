using Cysharp.Threading.Tasks;
using GameFlow.FSM;
using UnityEngine.Scripting;

namespace Code.GameFlow
{
    [Preserve]
    public class SettingsState : FSMState
    {
        public override UniTask OnEnter()
        {
            W.GetResource<SettingsPopup>().gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }
        
        public override UniTask OnExit()
        {
            W.GetResource<SettingsPopup>().gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }
    }
}