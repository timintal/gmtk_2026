using System;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public class TweenWorldRotation : Vector3Tween<Transform>
    {
        [ExposeInEditor]
        public bool skipX;
        [ExposeInEditor]
        public bool skipY;
        [ExposeInEditor]
        public bool skipZ;

        protected override Vector3 Property
        {
            get => target.rotation.eulerAngles;
            set
            {
                Vector3 newAngles = value;

                if (skipX) newAngles.x = target.rotation.x;
                if (skipY) newAngles.y = target.rotation.y;
                if (skipZ) newAngles.z = target.rotation.z;

                target.rotation = Quaternion.Euler(newAngles);
            }
        }
    }
}