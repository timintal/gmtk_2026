using FFS.Libraries.StaticEcs;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Features.Dice.View
{
    public class RerollButton : MonoBehaviour
    {
        [SerializeField] Button _button;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnRerollClicked);
        }
        private void OnRerollClicked()
        {
            var playerState = W.GetResource<PlayerState>();
            if (playerState.RerollsCount > 0)
            {
                W.NewEntity<Default>().Set(new RerollRequest { ClearCurrent = true, DiceCount = playerState.DicePerRoll });
            }
        }
    }
}