using System;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public class TweenSizeDelta : Vector2Tween<RectTransform>
    {
        [ExposeInEditor]
        public bool skipX;
        [ExposeInEditor]
        public bool skipY;

        protected override Vector2 Property
        {
            get => target.sizeDelta;
            set
            {
                Vector2 newPosition = value;

                if (skipX) newPosition.x = target.sizeDelta.x;
                if (skipY) newPosition.y = target.sizeDelta.y;

                target.sizeDelta = newPosition;
            }
        }
    }
}
