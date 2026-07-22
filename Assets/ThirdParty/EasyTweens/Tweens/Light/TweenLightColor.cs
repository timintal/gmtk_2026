using System;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public class TweenLightColor : ColorTween<Light>
    {
        protected override Color Property
        {
            get => target.color;
            set => target.color = value;
        }
    }
}