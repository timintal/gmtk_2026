using System;
using FFS.Libraries.StaticEcs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Features.Blessings
{
    [CreateAssetMenu(fileName = "BlessingsLibrary", menuName = "Game/Blessings Library", order = 0)]
    public class BlessingsLibrary : ScriptableObject, IResource
    {
        [ListDrawerSettings(ShowFoldout = true, ShowIndexLabels = true, ListElementLabelName = "BlessingId")]
        public BlessingsConfig[] BlessingsConfigs = Array.Empty<BlessingsConfig>();

        private void OnEnable()
        {
            foreach (var config in BlessingsConfigs)
            {
                config.OnDebugCreateBlessing += blessingsConfig => CreateBlessing(blessingsConfig);
            }
        }
        
        private void OnDisable()
        {
            foreach (var config in BlessingsConfigs)
            {
                config.OnDebugCreateBlessing=null;
            }
        }

        public W.Entity CreateBlessing(BlessingsConfig config)
        {
            if (!Application.isPlaying)
                return default;

            return config.GetDiceBlessingEntity();
        }
        
        [Button]
        public W.Entity CreateBlessing(string blessingId)
        {
            foreach (var config in BlessingsConfigs)
            {
                if (config.BlessingId == blessingId)
                {
                    return CreateBlessing(config);
                }
            }

            return default;
        }
        public BlessingsConfig GetBlessingConfig(string blessingId)
        {
            foreach (var config in BlessingsConfigs)
            {
                if (config.BlessingId == blessingId)
                {
                    return config;
                }
            }
            return null;
        }
    }
}