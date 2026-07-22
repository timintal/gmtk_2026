using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Common.View
{
    public struct WorldSpaceUiFollowLink : IComponent
    {
        public RectTransform UiElement;
        public Transform Target;
        public Vector3 Offset;
    }
}
