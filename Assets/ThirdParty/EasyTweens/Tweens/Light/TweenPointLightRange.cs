using System;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public class TweenLightRange : FloatTween<Light>
    {
        protected override float Property
        {
            get => target.range;
            set => target.range = value;
        }
    }
}