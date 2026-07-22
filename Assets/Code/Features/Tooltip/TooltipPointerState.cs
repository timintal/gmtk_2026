using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Features.Tooltip
{
    public struct TooltipPointerState : IResource
    {
        public Vector2 ScreenPosition;
        public Vector2 ScreenDelta;
        public bool IsBlockedByUi;
        public bool HasWorldPosition;
        public Vector2 WorldPosition;
        public bool IsTouchActive;
        public bool TouchBeganThisFrame;
        public bool TouchEndedThisFrame;
    }
}
