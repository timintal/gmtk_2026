using System;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public class TweenLocalScale : Vector3Tween<Transform>
    {
        [ExposeInEditor]
        public bool skipX;
        [ExposeInEditor]
        public bool skipY;
        [ExposeInEditor]
        public bool skipZ;

        protected override Vector3 Property
        {
            get => target.localScale;
            set
            {
                Vector3 newScale = value;
                if (skipX) newScale.x = target.localScale.x;
                if (skipY) newScale.y = target.localScale.y;
                if (skipZ) newScale.z = target.localScale.z;

                target.localScale = newScale;
            }
        }
    }
}
