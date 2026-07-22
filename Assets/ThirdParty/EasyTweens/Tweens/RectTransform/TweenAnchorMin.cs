using System;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public class TweenAnchorMin : Vector2Tween<RectTransform>
    {

        [ExposeInEditor]
        public bool skipX;
        [ExposeInEditor]
        public bool skipY;

        protected override Vector2 Property
        {
            get => target.anchorMin;
            set
            {
                Vector2 newAnchor = value;

                if (skipX) newAnchor.x = target.anchorMin.x;
                if (skipY) newAnchor.y = target.anchorMin.y;

                target.anchorMin = newAnchor;
            }
        }
    }
}
