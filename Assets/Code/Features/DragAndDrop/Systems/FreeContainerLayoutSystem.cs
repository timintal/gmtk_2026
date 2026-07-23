using System.Collections.Generic;
using Code.Common;
using Code.Common.Hitbox;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Features.DragAndDrop
{
    public sealed class FreeContainerLayoutSystem : ISystem
    {
        private readonly List<W.Entity> _members = new();
        private readonly List<FreeLayoutItem> _items = new();

        public void Update()
        {
            foreach (var container in W.Query<All<DragContainer, FreeContainerLayout, Position, ContainerLayoutDirty>>().Entities())
            {
                ref readonly var position = ref container.Read<Position>();
                ref readonly var layout = ref container.Read<FreeContainerLayout>();
                ApplyLayout(container, position.Value, layout);
                container.Delete<ContainerLayoutDirty>();
            }
        }

        private void ApplyLayout(W.Entity container, Vector2 containerPosition, in FreeContainerLayout layout)
        {
            _members.Clear();
            _items.Clear();

            if (!container.Has<W.Links<DragContainerItems>>())
            {
                return;
            }

            var usesLocalLayoutSpace = ContainerLayoutUtility.UsesLocalLayoutSpace(container);

            ref readonly var items = ref container.Read<W.Links<DragContainerItems>>();
            foreach (var itemLink in items)
            {
                if (!itemLink.Value.TryUnpack<WT>(out var draggable) || !draggable.Has<Position>())
                {
                    continue;
                }

                if (draggable.Has<Dragging>() || draggable.Has<Destroyed>())
                {
                    continue;
                }

                _members.Add(draggable);
                _items.Add(new FreeLayoutItem
                {
                    Center = draggable.Read<Position>().Value,
                    HalfSize = ResolveHalfSize(draggable, layout)
                });
            }

            if (_members.Count == 0)
            {
                return;
            }

            var bounds = ResolveBounds(container, containerPosition, layout, usesLocalLayoutSpace);
            FreeLayoutCalculator.Resolve(bounds, _items, layout.OverlapTolerance, layout.RelaxIterations);

            for (var i = 0; i < _members.Count; i++)
            {
                _members[i].Ref<Position>().Value = _items[i].Center;
            }
        }

        private static Vector2 ResolveHalfSize(W.Entity draggable, in FreeContainerLayout layout)
        {
            if (draggable.Has<Hitbox2D>())
            {
                var collider = draggable.Read<Hitbox2D>().Value;
                if (collider != null)
                {
                    return collider.bounds.extents;
                }
            }

            if (draggable.Has<HitboxUi>())
            {
                var rectTransform = draggable.Read<HitboxUi>().Value;
                if (rectTransform != null)
                {
                    return rectTransform.rect.size * 0.5f;
                }
            }

            return layout.ItemSize * 0.5f;
        }

        private static Rect ResolveBounds(W.Entity container, Vector2 containerPosition, in FreeContainerLayout layout, bool usesLocalLayoutSpace)
        {
            if (usesLocalLayoutSpace)
            {
                var size = layout.Bounds;
                var center = layout.LocalOffset;

                if (container.Has<DragContainerHitboxUi>())
                {
                    var rectTransform = container.Read<DragContainerHitboxUi>().Value;
                    if (rectTransform != null)
                    {
                        var rect = rectTransform.rect;
                        if (size == Vector2.zero)
                        {
                            size = rect.size;
                        }

                        center += rect.center;
                    }
                }

                if (size == Vector2.zero)
                {
                    size = Vector2.one;
                }

                return new Rect(center - size * 0.5f, size);
            }

            if (container.Has<DragContainerHitbox2D>())
            {
                var collider = container.Read<DragContainerHitbox2D>().Value;
                if (collider != null)
                {
                    var worldBounds = collider.bounds;
                    var size = layout.Bounds == Vector2.zero ? (Vector2)worldBounds.size : layout.Bounds;
                    var center = (Vector2)worldBounds.center + layout.LocalOffset;
                    return new Rect(center - size * 0.5f, size);
                }
            }

            var fallbackSize = layout.Bounds == Vector2.zero ? Vector2.one : layout.Bounds;
            var fallbackCenter = containerPosition + layout.LocalOffset;
            return new Rect(fallbackCenter - fallbackSize * 0.5f, fallbackSize);
        }
    }
}
