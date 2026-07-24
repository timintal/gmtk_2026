using _Game.Features.Enemies;
using Code.Configs;
using Code.Features.Tooltip;
using FFS.Libraries.StaticEcs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.UI
{
    public class ModifierView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _modifierText;
        [SerializeField] private Image _modifierIcon;
        [SerializeField] WTEntityProvider _tooltipEntityProvider;

        public void SetModifier(CountdownModifierType type, W.Entity entity)
        {
            var visualConfig = W.GetResource<VisualConfig>();
            var info = visualConfig.GetModifierInfo(type);
            UpdateModifierText(info, entity);
            _modifierIcon.sprite = info.Icon;

            if (info.Icon == null)
            {
                _modifierIcon.gameObject.SetActive(false);
                _modifierText.gameObject.SetActive(true);
            }
            else
            {
                _modifierIcon.gameObject.SetActive(true);
                _modifierText.gameObject.SetActive(false);
            }
            
            SetTooltipDescription(info, entity);
        }
        private void SetTooltipDescription(VisualConfig.CountdownModifierInfo info, W.Entity modifierEntity)
        {
            if (_tooltipEntityProvider.Entity.Has<Tooltip>())
            {
                string value = string.Empty;
                if (info.Type == CountdownModifierType.Bigger && modifierEntity.Has<AcceptBigger>())
                {
                    var biggerThan = modifierEntity.Read<AcceptBigger>();
                    value = biggerThan.Value.ToString();
                }
                else if (info.Type == CountdownModifierType.Smaller && modifierEntity.Has<AcceptSmaller>())
                {
                    var smallerThan = modifierEntity.Read<AcceptSmaller>();
                    value = smallerThan.Value.ToString();
                }
                
                ref var tooltip = ref _tooltipEntityProvider.Entity.Ref<Tooltip>();
                tooltip.Text = string.Format(info.Description, value);
            }
        }
        private void UpdateModifierText(VisualConfig.CountdownModifierInfo info, World<WT>.Entity entity)
        {
            string value = string.Empty;
            if (info.Type == CountdownModifierType.Bigger && entity.Has<AcceptBigger>())
            {
                var biggerThan = entity.Read<AcceptBigger>();
                value = biggerThan.Value.ToString();
            }
            else if (info.Type == CountdownModifierType.Smaller && entity.Has<AcceptSmaller>())
            {
                var smallerThan = entity.Read<AcceptSmaller>();
                value = smallerThan.Value.ToString();
            }
            
            _modifierText.text = string.Format(info.Name, value);
        }
    }
}