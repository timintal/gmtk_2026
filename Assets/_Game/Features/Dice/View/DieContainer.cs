using Code.Common.View;
using FFS.Libraries.StaticEcs;
using TMPro;
using UnityEngine;

namespace _Game.Features.Dice.View
{
    public class DieContainer : ResourceMonoBehaviour<DieContainer>
    {
        [SerializeField] TMP_Text _text;

        private void Update()
        {
            int totalValue = 0;
            foreach (var e in W.Query<All<Dice, DiceValue>>().Entities())
            {
                totalValue += e.Read<DiceValue>().Value;
            }
            _text.text = $"{totalValue}";
        }
    }
}