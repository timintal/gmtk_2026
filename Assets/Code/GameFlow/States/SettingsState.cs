using Cysharp.Threading.Tasks;
using Libraries.GameFlow.FSM;
using UnityEngine.Scripting;

namespace Code.GameFlow
{
    [Preserve]
    public class SettingsState : FSMState
    {
        public override async UniTask OnEnter()
        {
            W.GetResource<SettingsPopup>().gameObject.SetActive(true);
        }
        
        public override async UniTask OnExit()
        {
            W.GetResource<SettingsPopup>().gameObject.SetActive(false);
        }
    }
}