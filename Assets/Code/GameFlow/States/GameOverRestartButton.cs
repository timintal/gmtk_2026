using Code.Ecs;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace Code.GameFlow
{
    [Preserve]
    public sealed class GameOverRestartButton : MonoBehaviour
    {
        [SerializeField] Button _button;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            if (!W.HasResource<FSM>())
                return;

            W.GetResource<FSM>().Value.ClearAndPush<MainGameState>();
        }
    }
}
