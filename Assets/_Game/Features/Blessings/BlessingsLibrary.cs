using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Features.Blessings
{
    [CreateAssetMenu(fileName = "BlessingsLibrary", menuName = "Game/Blessings Library", order = 0)]
    public class BlessingsLibrary : ScriptableObject
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

        public BlessingsConfig GetRandomBlessingConfig(int level, List<string> excludedBlessingIds = null)
        {
            float totalProbability = 0;
            foreach (var config in BlessingsConfigs)
            {
                if (config.LevelLock <= level && 
                    (excludedBlessingIds == null || !excludedBlessingIds.Contains(config.BlessingId)))
                {
                    totalProbability += config.Probability;
                }
            }
            
            float random = UnityEngine.Random.Range(0, totalProbability);
            foreach (var config in BlessingsConfigs)
            {
                if (config.LevelLock <= level && 
                    (excludedBlessingIds == null || !excludedBlessingIds.Contains(config.BlessingId)))
                {
                    if (random < config.Probability)
                    {
                        return config;
                    }
                    random -= config.Probability;
                }
            }
            
            return BlessingsConfigs[0];
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