#if UI_EFFECT

using System;
using Coffee.UIEffects;

namespace EasyTweens
{
    [Serializable, TweenCategoryOverride("Image")]
    public class TweenUIEffect : FloatTween<UIEffect>
    {
        protected override float Property
        {
            get => target.transitionRate;
            set => target.transitionRate = value;
        }
    }
}
#endif