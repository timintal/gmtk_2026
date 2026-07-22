using System;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public class TweenCameraOrtographicSize : FloatTween<Camera>
    {
        protected override float Property
        {
            get
            {
                return target.orthographicSize;
            }
            set
            {
                target.orthographicSize = value;
            }
        }
    }
}
