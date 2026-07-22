using System;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public class TweenAnchorMax : Vector2Tween<RectTransform>
    {
        [ExposeInEditor]
        public bool skipX;
        [ExposeInEditor]
        public bool skipY;

        protected override Vector2 Property
        {
            get => target.anchorMax;
            set
            {
                Vector2 newAnchor = value;

                if (skipX) newAnchor.x = target.anchorMax.x;
                if (skipY) newAnchor.y = target.anchorMax.y;

                target.anchorMax = newAnchor;
            }
        }
    }
}
