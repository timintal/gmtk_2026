using System;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public class TweenAnchoredPosition : Vector2Tween<RectTransform>
    {
        [ExposeInEditor]
        public bool skipX;
        [ExposeInEditor]
        public bool skipY;

        protected override Vector2 Property
        {
            get => target.anchoredPosition;
            set
            {
                Vector2 newPosition = value;

                if (skipX) newPosition.x = target.anchoredPosition.x;
                if (skipY) newPosition.y = target.anchoredPosition.y;

                target.anchoredPosition = newPosition;
            }
        }
    }
}
