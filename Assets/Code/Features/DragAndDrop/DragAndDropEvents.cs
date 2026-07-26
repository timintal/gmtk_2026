using System;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Features.DragAndDrop
{
    [Serializable]
    public struct DragPointerDownEvent : IEvent
    {
        public int PointerId;
        public Vector2 ScreenPosition;
        public Vector2 WorldPosition;
        public bool HasWorldPosition;
        public bool IsBlockedByUi;
    }

    [Serializable]
    public struct DragPointerMoveEvent : IEvent
    {
        public int PointerId;
        public Vector2 ScreenPosition;
        public Vector2 ScreenDelta;
        public Vector2 WorldPosition;
        public bool HasWorldPosition;
        public bool IsBlockedByUi;
    }

    [Serializable]
    public struct DragPointerUpEvent : IEvent
    {
        public int PointerId;
        public Vector2 ScreenPosition;
        public Vector2 WorldPosition;
        public bool HasWorldPosition;
        public bool IsBlockedByUi;
    }

    [Serializable]
    public struct DragAccepted : IEvent
    {
        public EntityGID Draggable;
        public EntityGID TargetContainer;
    }

    [Serializable]
    public struct DragStarted : IEvent
    {
        public EntityGID Draggable;
    }

    [Serializable]
    public struct DragEnded : IEvent
    {
        public EntityGID Draggable;
        public EntityGID TargetContainer;
        public bool Accepted;
        public string RejectReason;
    }
}
