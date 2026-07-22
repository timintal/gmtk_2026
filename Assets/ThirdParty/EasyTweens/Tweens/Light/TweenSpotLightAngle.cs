using System;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public class TweenSpotLightAngle : FloatTween<Light>
    {
        protected override float Property
        {
            get => target.spotAngle;
            set => target.spotAngle = value;
        }
    }
}