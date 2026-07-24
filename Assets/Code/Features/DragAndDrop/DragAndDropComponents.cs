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
            if (link.TryUnpack<TW>(out var container))
            {
                container.Set<ContainerLayoutDirty>();
            }
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

    /// <summary>
    /// Free-form layout: draggables keep whatever position they were dropped at (clamped inside
    /// the container). When the dropped item overlaps existing members beyond the allowed
    /// tolerance, those members are pushed aside without leaving the container bounds.
    /// </summary>
    [Serializable]
    public struct FreeContainerLayout : IComponent
    {
        /// <summary>Center offset of the layout area relative to the container hitbox center.</summary>
        public Vector2 LocalOffset;

        /// <summary>Explicit layout area size. Zero derives it from the container hitbox.</summary>
        public Vector2 Bounds;

        /// <summary>Explicit item size used when a draggable has no hitbox. Zero derives it per item.</summary>
        public Vector2 ItemSize;

        /// <summary>How much two items may overlap (in layout units) before they get pushed apart.</summary>
        public float OverlapTolerance;

        /// <summary>Number of separation passes used to resolve overlaps when animation is disabled.</summary>
        public int RelaxIterations;

        /// <summary>
        /// Max distance (layout units per second) an item travels while spreading apart. When greater
        /// than zero the layout animates over several frames instead of snapping in one; zero keeps the
        /// original instant resolve.
        /// </summary>
        public float AnimationSpeed;

        /// <summary>
        /// Fraction (0..1) of each overlap resolved per frame while animating. Lower values ease the
        /// spread out; values &lt;= 0 default to 1 (resolve fully, still capped by <see cref="AnimationSpeed"/>).
        /// </summary>
        public float AnimationSmoothing;

        /// <summary>Per-frame movement below this threshold counts as settled and stops the animation.</summary>
        public float SettleEpsilon;
    }
}
