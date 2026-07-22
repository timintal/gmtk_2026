using System;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Common.Hitbox
{
    [Serializable]
    public struct Hitbox2D : IComponent
    {
        public Collider2D Value;
    }

    [Serializable]
    public struct HitboxUi : IComponent
    {
        public RectTransform Value;
    }
}
