using System.Collections.Generic;
using Code.Common.Hitbox;
using Code.Common.View;
using Code.Features.DragAndDrop;
using FFS.Libraries.StaticEcs;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Features.Tooltip
{
    internal static class TooltipTargetUtility
    {
        private const int WorldOverlapBufferSize = 64;

        private static readonly List<(Collider2D Collider, int SortKey)> WorldColliderBuffer = new();
        private static readonly Collider2D[] WorldOverlapBuffer = new Collider2D[WorldOverlapBufferSize];

        public static bool TryFindTopTooltipTarget(in TooltipPointerState pointer, out W.Entity hit)
        {
            if (TryFindTopUiTooltip(in pointer, out hit))
            {
                return true;
            }

            if (pointer.IsBlockedByUi || !pointer.HasWorldPosition)
            {
                hit = default;
                return false;
            }

            return TryFindTopWorldTooltip(pointer.WorldPosition, out hit);
        }

        private static bool TryFindTopUiTooltip(in TooltipPointerState pointer, out W.Entity hit)
        {
            hit = default;
            if (!W.HasResource<TooltipUiRaycastCache>())
            {
                return false;
            }

            var results = W.GetResource<TooltipUiRaycastCache>().Results;
            for (var i = 0; i < results.Count; i++)
            {
                if (results[i].module is not GraphicRaycaster)
                {
                    continue;
                }

                if (TryFindEntityForUiRaycastHit(pointer.ScreenPosition, results[i].gameObject, out hit))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryFindTopWorldTooltip(Vector2 world, out W.Entity hit)
        {
            hit = default;
            WorldColliderBuffer.Clear();

            var count = Physics2D.defaultPhysicsScene.OverlapPoint(world, WorldOverlapBuffer);
            if (count > WorldOverlapBuffer.Length)
            {
                count = WorldOverlapBuffer.Length;
            }

            for (var i = 0; i < count; i++)
            {
                AddWorldCollider(WorldOverlapBuffer[i]);
            }

            WorldColliderBuffer.Sort(static (a, b) => b.SortKey.CompareTo(a.SortKey));

            for (var i = 0; i < WorldColliderBuffer.Count; i++)
            {
                if (TryFindEntityForWorldCollider(WorldColliderBuffer[i].Collider, out hit))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AddWorldCollider(Collider2D collider)
        {
            if (collider == null)
            {
                return;
            }

            WorldColliderBuffer.Add((collider, GetWorldSortKey(collider)));
        }

        private static bool TryFindEntityForUiRaycastHit(Vector2 screen, GameObject hitObject, out W.Entity hit)
        {
            hit = default;
            if (hitObject == null)
            {
                return false;
            }

            var bestDepth = int.MinValue;
            var found = false;

            foreach (var entity in W.Query<All<Tooltip, HitboxUi>>().Entities())
            {
                ref readonly var hitbox = ref entity.Read<HitboxUi>();
                if (hitbox.Value == null
                    || !DragUiUtility.ContainsScreenPoint(hitbox.Value, screen)
                    || !IsTransformUnderHitbox(hitObject.transform, hitbox.Value))
                {
                    continue;
                }

                var depth = GetDepthFromHitbox(hitbox.Value, hitObject.transform);
                if (depth <= bestDepth)
                {
                    continue;
                }

                bestDepth = depth;
                hit = entity;
                found = true;
            }

            if (found)
            {
                return true;
            }

            return TryResolveViaViewOrProvider(hitObject, requireUiHitbox: true, out hit);
        }

        private static bool TryFindEntityForWorldCollider(Collider2D collider, out W.Entity hit)
        {
            hit = default;
            if (collider == null)
            {
                return false;
            }

            foreach (var entity in W.Query<All<Tooltip, Hitbox2D>>().Entities())
            {
                ref readonly var hitbox = ref entity.Read<Hitbox2D>();
                if (hitbox.Value == collider)
                {
                    hit = entity;
                    return true;
                }
            }

            return TryResolveViaViewOrProvider(collider.gameObject, requireUiHitbox: false, out hit);
        }

        private static bool TryResolveViaViewOrProvider(GameObject gameObject, bool requireUiHitbox, out W.Entity hit)
        {
            hit = default;

            if (TryResolveViaBoundView(gameObject, out hit) && HasRequiredHitbox(hit, requireUiHitbox))
            {
                return hit.Has<Tooltip>();
            }

            hit = default;
            var provider = gameObject.GetComponentInParent<WTEntityProvider>();
            if (provider == null
                || !provider.EntityGid.TryUnpack<WT>(out hit)
                || !hit.Has<Tooltip>()
                || !HasRequiredHitbox(hit, requireUiHitbox))
            {
                hit = default;
                return false;
            }

            return true;
        }

        private static bool TryResolveViaBoundView(GameObject gameObject, out W.Entity hit)
        {
            hit = default;

            var entityView = gameObject.GetComponentInParent<EntityView>();
            if (entityView != null && entityView.TryGetBoundEntity(out hit))
            {
                return true;
            }

            var childView = gameObject.GetComponentInParent<EntityChildView>();
            return childView != null && childView.TryGetBoundEntity(out hit);
        }

        private static bool HasRequiredHitbox(W.Entity entity, bool requireUiHitbox)
        {
            return requireUiHitbox ? entity.Has<HitboxUi>() : entity.Has<Hitbox2D>();
        }

        private static bool IsTransformUnderHitbox(Transform transform, RectTransform hitbox)
        {
            return transform == hitbox || transform.IsChildOf(hitbox);
        }

        private static int GetDepthFromHitbox(RectTransform hitbox, Transform hit)
        {
            var depth = 0;
            var current = hit;
            while (current != null && current != hitbox)
            {
                depth++;
                current = current.parent;
            }

            return current == hitbox ? depth : -1;
        }

        private static int GetWorldSortKey(Collider2D collider)
        {
            var renderer = collider.GetComponent<SpriteRenderer>() ?? collider.GetComponentInChildren<SpriteRenderer>();
            if (renderer == null)
            {
                return collider.GetHashCode();
            }

            return renderer.sortingLayerID * 100000 + renderer.sortingOrder;
        }
    }
}
