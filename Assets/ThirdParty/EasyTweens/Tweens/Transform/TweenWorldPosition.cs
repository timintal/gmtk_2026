using System;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public class TweenWorldPosition : Vector3Tween<Transform>
    {
        [ExposeInEditor]
        public bool skipX;
        [ExposeInEditor]
        public bool skipY;
        [ExposeInEditor]
        public bool skipZ;

        protected override Vector3 Property
        {
            get => target.position;
            set
            {
                Vector3 newPosition = value;

                if (skipX) newPosition.x = target.position.x;
                if (skipY) newPosition.y = target.position.y;
                if (skipZ) newPosition.z = target.position.z;

                target.position = newPosition;
            }
        }
    }
}