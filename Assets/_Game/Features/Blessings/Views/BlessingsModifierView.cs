using TMPro;
using UnityEngine;

namespace _Game.Features.Blessings.Views
{
    public class BlessingsModifierView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _modifierText;
        
        public void SetModifier(string modifier)
        {
            _modifierText.text = modifier;
        }
    }
}