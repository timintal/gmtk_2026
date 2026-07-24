using System;
using _Game.Features.Enemies;
using Code.Common.View.UI;
using Code.Features.Tooltip;
using FFS.Libraries.StaticEcs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Code.Configs
{
    [CreateAssetMenu(fileName = "VisualConfig", menuName = "Proto/VisualConfig", order = 0)]
    public class VisualConfig : ScriptableObject, IResource
    {
        public TooltipPrefabEntry[] TooltipPrefabs = Array.Empty<TooltipPrefabEntry>();
        [ListDrawerSettings(ShowFoldout = true, ShowIndexLabels = true, ListElementLabelName = "Type")]
        public CountdownModifierInfo[] CountdownModifiers = Array.Empty<CountdownModifierInfo>();
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

        public CountdownModifierInfo GetModifierInfo(CountdownModifierType type)
        {
            for (int i = 0; i < CountdownModifiers.Length; i++)
            {
                if (CountdownModifiers[i].Type == type)
                {
                    return CountdownModifiers[i];
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

        [Serializable]
        public class CountdownModifierInfo
        {
            public CountdownModifierType Type;
            public Sprite Icon;
            public string Name;
            public string Description;
            public string ErrorMessage;
        }
    }
}
