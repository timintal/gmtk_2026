using System;
using UnityEngine.UI;

namespace EasyTweens
{
    [Serializable, TweenCategoryOverride("UI")]
    public class SliderTween : FloatTween<Slider>
    {

        protected override float Property
        {
            get => target.value;
            set => target.value = value;
        }
    }
}