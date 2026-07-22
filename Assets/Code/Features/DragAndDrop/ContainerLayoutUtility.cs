using Code.Common;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Features.DragAndDrop
{
    internal static class ContainerLayoutUtility
    {
        public static bool UsesLocalLayoutSpace(W.Entity container)
        {
            return container.Has<DragContainerHitboxUi>();
        }

        public static Vector2 ResolveMemberPosition(
            Vector2 containerPosition,
            Vector2 slotLocalPosition,
            bool usesLocalLayoutSpace)
        {
            return usesLocalLayoutSpace ? slotLocalPosition : containerPosition + slotLocalPosition;
        }
    }
}
