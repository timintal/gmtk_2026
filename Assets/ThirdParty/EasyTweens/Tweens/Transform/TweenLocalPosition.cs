using System;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public class TweenLocalPosition : Vector3Tween<Transform>
    {
        [ExposeInEditor]
        public bool skipX;
        [ExposeInEditor]
        public bool skipY;
        [ExposeInEditor]
        public bool skipZ;

        protected override Vector3 Property
        {
            get => target.localPosition;
            set
            {
                Vector3 newPosition = value;

                if (skipX) newPosition.x = target.localPosition.x;
                if (skipY) newPosition.y = target.localPosition.y;
                if (skipZ) newPosition.z = target.localPosition.z;

                target.localPosition = newPosition;
            }
        }
    }
}
