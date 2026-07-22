using System;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public class TweenBackgroundColor : ColorTween<Camera>
    {
        protected override Color Property
        {
            get => target.backgroundColor;
            set => target.backgroundColor = value;
        }
    }
}