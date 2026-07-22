using System;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public class TweenLightIntensity : FloatTween<Light>
    {
        protected override float Property
        {
            get => target.intensity;
            set => target.intensity = value;
        }
    }
}