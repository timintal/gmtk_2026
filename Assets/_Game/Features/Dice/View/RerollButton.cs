using _Game.Features.Run;
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
            if (W.Query<All<ActiveTurn>>().EntitiesCount() > 0)
            {
                W.NewEntity<Default>().Set<EndTurnRequest>();
            }
        }
    }
}