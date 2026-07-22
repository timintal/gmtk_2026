using System;
using TMPro;

namespace EasyTweens
{
    [Serializable, TweenCategoryOverride("TMP")]
    public class CharacterSpacingTween : FloatTween<TMP_Text>
    {
        protected override float Property
        {
            get => target.characterSpacing;
            set => target.characterSpacing = value;
        }
    }
}