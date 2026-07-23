using System;
using Code.Common.View.UI;
using Code.Features.Tooltip;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Configs
{
    [CreateAssetMenu(fileName = "VisualConfig", menuName = "Proto/VisualConfig", order = 0)]
    public class VisualConfig : ScriptableObject, IResource
    {
        public TooltipPrefabEntry[] TooltipPrefabs = Array.Empty<TooltipPrefabEntry>();
        public WTEntityProvider DicePrefab;

        public TooltipView GetTooltipPrefab(TooltipType type)
        {
            for (var i = 0; i < TooltipPrefabs.Length; i++)
            {
                if (TooltipPrefabs[i].Type == type)
                {
                    return TooltipPrefabs[i].Prefab;
                }
            }

            return null;
        }

        [Serializable]
        public struct TooltipPrefabEntry
        {
            public TooltipType Type;
            public TooltipView Prefab;
        }
    }
}
