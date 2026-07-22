using System;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public class TweenPivot : Vector2Tween<RectTransform>
    {
        [ExposeInEditor]
        public bool skipX;
        [ExposeInEditor]
        public bool skipY;

        protected override Vector2 Property
        {
            get => target.pivot;
            set
            {
                Vector2 newPosition = value;

                if (skipX) newPosition.x = target.pivot.x;
                if (skipY) newPosition.y = target.pivot.y;

                target.pivot = newPosition;
            }
        }
    }
}
