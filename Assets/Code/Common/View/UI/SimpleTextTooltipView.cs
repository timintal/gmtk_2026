using Code.Features.Tooltip;
using FFS.Libraries.StaticEcs;
using TMPro;
using UnityEngine;

namespace Code.Common.View.UI
{
    public sealed class SimpleTextTooltipView : TooltipView
    {
        [SerializeField] private TMP_Text _text;

        public override void Bind(W.Entity entity)
        {
            if (!entity.Has<Tooltip>())
            {
                _text.text = string.Empty;
                return;
            }

            _text.text = entity.Read<Tooltip>().Text ?? string.Empty;
        }
    }
}
