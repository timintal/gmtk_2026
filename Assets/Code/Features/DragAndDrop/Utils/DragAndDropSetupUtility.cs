#if UNITY_EDITOR
using System;
using Code.Common;
using Code.Common.Hitbox;
using FFS.Libraries.StaticEcs.Unity;
using UnityEditor;
using UnityEngine;

namespace Code.Features.DragAndDrop.Utils
{
    internal static class DragAndDropSetupUtility
    {
        public static DragSetupSpace ResolveSpace(GameObject gameObject, DragSetupSpace requested)
        {
            if (requested != DragSetupSpace.Auto)
            {
                return requested;
            }

            return gameObject.GetComponent<RectTransform>() != null ? DragSetupSpace.Ui : DragSetupSpace.World2D;
        }

        public static WTEntityProvider EnsureEntityProvider(GameObject gameObject)
        {
            var provider = gameObject.GetComponent<WTEntityProvider>();
            if (provider != null)
            {
                return provider;
            }

            return gameObject.AddComponent<WTEntityProvider>();
        }

        public static void ApplyDraggable(
            GameObject gameObject,
            DragSetupSpace space,
            int priority,
            bool addColliderIfMissing)
        {
            var resolvedSpace = ResolveSpace(gameObject, space);
            var provider = EnsureEntityProvider(gameObject);

            provider.OnChangeProvider(
                new ComponentProvider { value = new Draggable { Disabled = false, Priority = priority } },
                typeof(Draggable));
            provider.OnChangeProvider(
                new ComponentProvider { value = new Position() },
                typeof(Position));
            provider.OnChangeProvider(
                new ComponentProvider { value = new SyncViewPosition() },
                typeof(SyncViewPosition));
            provider.OnChangeProvider(
                new TagProvider { value = new InitPositionFromTransform() },
                typeof(InitPositionFromTransform));

            switch (resolvedSpace)
            {
                case DragSetupSpace.Ui:
                    ApplyUiDraggable(gameObject, provider);
                    break;
                case DragSetupSpace.World2D:
                    ApplyWorldDraggable(gameObject, provider, addColliderIfMissing);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(space), resolvedSpace, null);
            }
        }

        public static void ApplyContainer(
            GameObject gameObject,
            DragSetupSpace space,
            int priority,
            int capacity,
            DragContainerLayoutKind layoutKind,
            LineContainerLayout lineLayout,
            GridContainerLayout gridLayout,
            bool addColliderIfMissing)
        {
            var resolvedSpace = ResolveSpace(gameObject, space);
            var provider = EnsureEntityProvider(gameObject);

            provider.OnChangeProvider(
                new ComponentProvider
                {
                    value = new DragContainer { Disabled = false, Priority = priority, Capacity = capacity }
                },
                typeof(DragContainer));
            provider.OnChangeProvider(
                new ComponentProvider { value = new Position() },
                typeof(Position));
            provider.OnChangeProvider(
                new TagProvider { value = new InitPositionFromTransform() },
                typeof(InitPositionFromTransform));

            switch (resolvedSpace)
            {
                case DragSetupSpace.Ui:
                    ApplyUiContainer(gameObject, provider);
                    break;
                case DragSetupSpace.World2D:
                    ApplyWorldContainer(gameObject, provider, addColliderIfMissing);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(space), resolvedSpace, null);
            }

            provider.OnDeleteProvider(typeof(LineContainerLayout));
            provider.OnDeleteProvider(typeof(GridContainerLayout));

            switch (layoutKind)
            {
                case DragContainerLayoutKind.Line:
                    provider.OnChangeProvider(
                        new ComponentProvider { value = lineLayout },
                        typeof(LineContainerLayout));
                    break;
                case DragContainerLayoutKind.Grid:
                    provider.OnChangeProvider(
                        new ComponentProvider { value = gridLayout },
                        typeof(GridContainerLayout));
                    break;
            }
        }

        private static void ApplyUiDraggable(GameObject gameObject, WTEntityProvider provider)
        {
            var rectTransform = gameObject.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                throw new InvalidOperationException(
                    $"UI draggable setup requires a {nameof(RectTransform)} on '{gameObject.name}'.");
            }

            provider.OnChangeProvider(
                new ComponentProvider { value = new HitboxUi { Value = rectTransform } },
                typeof(HitboxUi));
            provider.OnChangeProvider(
                new ComponentProvider { value = new TransformLink() { Value = rectTransform } },
                typeof(TransformLink));

            provider.OnDeleteProvider(typeof(Hitbox2D));
            provider.OnDeleteProvider(typeof(TransformLink));
        }

        private static void ApplyWorldDraggable(
            GameObject gameObject,
            WTEntityProvider provider,
            bool addColliderIfMissing)
        {
            var collider = FindOrCreateCollider2D(gameObject, addColliderIfMissing);

            provider.OnChangeProvider(
                new ComponentProvider { value = new Hitbox2D { Value = collider } },
                typeof(Hitbox2D));
            provider.OnChangeProvider(
                new ComponentProvider { value = new TransformLink { Value = gameObject.transform } },
                typeof(TransformLink));

            provider.OnDeleteProvider(typeof(HitboxUi));
        }

        private static void ApplyUiContainer(GameObject gameObject, WTEntityProvider provider)
        {
            var rectTransform = gameObject.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                throw new InvalidOperationException(
                    $"UI container setup requires a {nameof(RectTransform)} on '{gameObject.name}'.");
            }

            provider.OnChangeProvider(
                new ComponentProvider { value = new DragContainerHitboxUi { Value = rectTransform } },
                typeof(DragContainerHitboxUi));
            provider.OnChangeProvider(
                new ComponentProvider { value = new TransformLink() { Value = rectTransform } },
                typeof(TransformLink));

            provider.OnDeleteProvider(typeof(DragContainerHitbox2D));
            provider.OnDeleteProvider(typeof(TransformLink));
        }

        private static void ApplyWorldContainer(
            GameObject gameObject,
            WTEntityProvider provider,
            bool addColliderIfMissing)
        {
            var collider = FindOrCreateCollider2D(gameObject, addColliderIfMissing);

            provider.OnChangeProvider(
                new ComponentProvider { value = new DragContainerHitbox2D { Value = collider } },
                typeof(DragContainerHitbox2D));
            provider.OnChangeProvider(
                new ComponentProvider { value = new TransformLink { Value = gameObject.transform } },
                typeof(TransformLink));

            provider.OnDeleteProvider(typeof(DragContainerHitboxUi));
        }

        private static Collider2D FindOrCreateCollider2D(GameObject gameObject, bool addIfMissing)
        {
            var collider = gameObject.GetComponent<Collider2D>();
            if (collider == null)
            {
                collider = gameObject.GetComponentInChildren<Collider2D>();
            }

            if (collider != null)
            {
                return collider;
            }

            if (!addIfMissing)
            {
                throw new InvalidOperationException(
                    $"World drag setup requires a {nameof(Collider2D)} on '{gameObject.name}' or its children.");
            }

            var box = Undo.AddComponent<BoxCollider2D>(gameObject);
            if (gameObject.TryGetComponent<SpriteRenderer>(out var spriteRenderer) && spriteRenderer.sprite != null)
            {
                box.size = spriteRenderer.sprite.bounds.size;
                EditorUtility.SetDirty(box);
            }

            return box;
        }
    }
}
#endif
