using System;
using UnityEngine.UI;

namespace EasyTweens
{
    [Serializable]
    public class TweenFillAmount : FloatTween<Image>
    {
        protected override float Property
        {
            get => target.fillAmount;
            set => target.fillAmount = value;
        }
    }
}