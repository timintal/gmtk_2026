using System;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Features.DragAndDrop
{
    public enum DragTransferRejectReason : byte
    {
        None,
        NoTargetContainer,
        InvalidDraggable,
        InvalidSourceContainer,
        InvalidTargetContainer,
        ContainerFull,
        Custom
    }

    /// <summary>Draggable → container (single parent link).</summary>
    public struct InDragContainer : ILinkType
    {
        public void OnAdd<TW>(World<TW>.Entity self, EntityGID link) where TW : struct, IWorldType
        {
            link.TryAddLinkItem<TW, DragContainerItems>(self);
        }

        public void OnDelete<TW>(World<TW>.Entity self, EntityGID link, HookReason reason) where TW : struct, IWorldType
        {
            link.TryDeleteLinkItem<TW, DragContainerItems>(self);
        }
    }

    /// <summary>Container → draggables (membership list).</summary>
    public struct DragContainerItems : ILinksType
    {
        public void OnAdd<TW>(World<TW>.Entity self, EntityGID link) where TW : struct, IWorldType
        {
            link.TryAddLink<TW, InDragContainer>(self);
        }

        public void OnDelete<TW>(World<TW>.Entity self, EntityGID link, HookReason reason) where TW : struct, IWorldType
        {
            link.TryDeleteLink<TW, InDragContainer>(self);
        }
    }

    [Serializable]
    public struct Draggable : IComponent
    {
        public bool Disabled;
        public int Priority;
    }
    
    public struct SetAsContainerChild : ITag{}

    [Serializable]
    public struct DragContainer : IComponent
    {
        public bool Disabled;
        public int Priority;
        public int Capacity;
    }

    [Serializable]
    public struct DragContainerHitbox2D : IComponent
    {
        public Collider2D Value;
    }

    public struct DragContainerHitboxUi : IComponent
    {
        public RectTransform Value;
    }

    [Serializable]
    public struct Dragging : IComponent
    {
        public int PointerId;
        public EntityGID SourceContainer;
        public bool HasSourceContainer;
        public Vector2 OriginPosition;
        public Vector2 PointerOffset;
        public bool UsesScreenSpace;
    }
    
    [Serializable]
    public struct DragTransferRequest : IComponent
    {
        public EntityGID Draggable;
        public EntityGID SourceContainer;
        public EntityGID TargetContainer;
        public bool HasSourceContainer;
        public bool HasTargetContainer;
        public int PointerId;
        public Vector2 DropScreenPosition;
        public Vector2 DropWorldPosition;
    }

    [Serializable]
    public struct DragTransferRejected : IComponent
    {
        public DragTransferRejectReason Reason;
    }

    public struct ContainerLayoutDirty : ITag
    {
    }

    [Serializable]
    public struct ContainerSlotIndex : IComponent
    {
        public int Order;
    }

    [Serializable]
    public struct LineContainerLayout : IComponent
    {
        public float Spacing;
        public Vector2 LocalOffset;
        public Vector2 Direction;
        public bool Centered;
    }

    [Serializable]
    public struct GridContainerLayout : IComponent
    {
        public int ColumnCount;
        public Vector2 CellSpacing;
        public Vector2 LocalOffset;
        public bool Centered;
    }
}
