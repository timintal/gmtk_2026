using System;
using TMPro;

namespace EasyTweens
{
    [Serializable, TweenCategoryOverride("TMP")]
    public class ParagraphSpacingTween : FloatTween<TMP_Text>
    {
        protected override float Property
        {
            get => target.paragraphSpacing;
            set => target.paragraphSpacing = value;
        }
    }
}