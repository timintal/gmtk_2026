#if UNITY_EDITOR
using Code.Common.Hitbox;
using Code.Features.DragAndDrop;
using Code.Features.DragAndDrop.Utils;
using Code.Features.Tooltip;
using FFS.Libraries.StaticEcs.Unity;
using UnityEngine;

namespace Code.Features.Tooltip.Utils
{
    public static class TooltipSetupUtility
    {
        public static WTEntityProvider EnsureEntityProvider(GameObject gameObject)
        {
            return DragAndDropSetupUtility.EnsureEntityProvider(gameObject);
        }

        public static void ApplyTooltip(
            GameObject gameObject,
            WTEntityProvider provider,
            TooltipType type,
            string text)
        {
            provider.OnChangeProvider(
                new ComponentProvider { value = new Tooltip { Type = type, Text = text } },
                typeof(Tooltip));

            var resolvedSpace = DragAndDropSetupUtility.ResolveSpace(gameObject, DragSetupSpace.Auto);
            switch (resolvedSpace)
            {
                case DragSetupSpace.Ui:
                    ApplyUiHitbox(gameObject, provider);
                    break;
                case DragSetupSpace.World2D:
                    ApplyWorldHitbox(gameObject, provider);
                    break;
            }
        }

        private static void ApplyUiHitbox(GameObject gameObject, WTEntityProvider provider)
        {
            var rectTransform = gameObject.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                throw new System.InvalidOperationException(
                    $"UI tooltip setup requires a {nameof(RectTransform)} on '{gameObject.name}'.");
            }

            provider.OnChangeProvider(
                new ComponentProvider { value = new HitboxUi { Value = rectTransform } },
                typeof(HitboxUi));
            provider.OnDeleteProvider(typeof(Hitbox2D));
        }

        private static void ApplyWorldHitbox(GameObject gameObject, WTEntityProvider provider)
        {
            var collider = gameObject.GetComponent<Collider2D>() ?? gameObject.GetComponentInChildren<Collider2D>(true);
            if (collider == null)
            {
                throw new System.InvalidOperationException(
                    $"World tooltip setup requires a {nameof(Collider2D)} on '{gameObject.name}' or its children.");
            }

            provider.OnChangeProvider(
                new ComponentProvider { value = new Hitbox2D { Value = collider } },
                typeof(Hitbox2D));
            provider.OnDeleteProvider(typeof(HitboxUi));
        }
    }
}
#endif
