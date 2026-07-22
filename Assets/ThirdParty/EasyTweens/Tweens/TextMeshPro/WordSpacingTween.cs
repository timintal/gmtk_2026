using System;
using TMPro;

namespace EasyTweens
{
    [Serializable, TweenCategoryOverride("TMP")]
    public class WordSpacingTween : FloatTween<TMP_Text>
    {
        protected override float Property
        {
            get => target.wordSpacing;
            set => target.wordSpacing = value;
        }
    }
}