using System;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public class TweenCameraFov : FloatTween<Camera>
    {
        protected override float Property
        {
            get
            {
                return target.fieldOfView;
            }
            set
            {
                target.fieldOfView = value;
            }
        }
    }
}
