using System;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public class TweenTransform : FloatTween<Transform>
    {
        [ExposeInEditor]
        public Transform from;
        [ExposeInEditor]
        public Transform to;
        protected override float Property { get; set; }

        public override void SetFactor(float f)
        {
            base.SetFactor(f);
            if (from != null && to != null)
            {
                target.position = Vector3.LerpUnclamped(from.position, to.position, Property);
                target.rotation = Quaternion.SlerpUnclamped(from.rotation, to.rotation, Property);
                target.localScale = Vector3.LerpUnclamped(from.localScale, to.localScale, Property);
            }
        }
    }
}