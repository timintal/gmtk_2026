using System;
using UnityEngine;

namespace EasyTweens
{
    [TweenCategoryOverride("UI")]
    [Serializable]
    public class TweenCanvasGroupFade : FloatTween<CanvasGroup>
    {
        protected override float Property
        {
            get => target.alpha;
            set => target.alpha = value;
        }
    }
}
