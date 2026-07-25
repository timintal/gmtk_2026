using System;
using System.Linq;
using _Game.Features.Enemies;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticEcs.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Features.Run.Configs
{
    [Serializable]
    public class EncounterInfo
    {
        public WTEntityProvider[] Enemies;

        public string Description()
        {
            int hp = 0;
            int attack = 0;

            if (Enemies == null) return "";
            #if UNITY_EDITOR
            foreach (var p in Enemies)
            {
                if (p == null || p.SerializedProviders == null) continue;
                p.SerializedProviders.ForEach(sp =>
                {
                    if (sp.ComponentType == typeof(Attack))
                    {
                        var component = ((ComponentProvider)sp).value;
                        attack += (int)((Attack)component).BaseValue;
                    }
                    if (sp.ComponentType == typeof(EnemyCountdown))
                    {
                        var component = ((ComponentProvider)sp).value;
                        hp += (int)((EnemyCountdown)component).Value;
                    }
                });
            }
            #endif

            return $"hp:{hp}, attack{attack}";
        }
    }

    [CreateAssetMenu(fileName = "EncountersConfig", menuName = "Run/EncountersConfig", order = 0)]
    public class EncountersConfig : ScriptableObject, IResource
    {
        [SerializeField] EncounterInfo firstEncounter;
        
        [ListDrawerSettings(ShowFoldout = true, ShowIndexLabels = true, ListElementLabelName = "Description")]
        [SerializeField] EncounterInfo[] BasicEncounters;
        [ListDrawerSettings(ShowFoldout = true, ShowIndexLabels = true, ListElementLabelName = "Description")]
        [SerializeField] EncounterInfo[] MediumEncounters;
        
        [ListDrawerSettings(ShowFoldout = true, ShowIndexLabels = true, ListElementLabelName = "Description")]
        [SerializeField] EncounterInfo[] AdvancedEncounters;
        [SerializeField] int MediumEncountersLevel;
        [SerializeField] int AdvancedEncountersLevel;
        

        public EncounterInfo GetRandomEncounter(int level)
        {
            if (level == 1) return firstEncounter;
            
            if (level < MediumEncountersLevel)
                return BasicEncounters[UnityEngine.Random.Range(0, BasicEncounters.Length)];
            
            if (level >= MediumEncountersLevel && level < AdvancedEncountersLevel)
                return MediumEncounters[UnityEngine.Random.Range(0, MediumEncounters.Length)];
            
            if (level >= AdvancedEncountersLevel)
                return AdvancedEncounters[UnityEngine.Random.Range(0, AdvancedEncounters.Length)];
            
            return AdvancedEncounters[UnityEngine.Random.Range(0, AdvancedEncounters.Length)];
        }

        [Button]
        void CheckForMissingRefs()
        {
            if (firstEncounter == null || firstEncounter.Enemies.Any(e => e == null))
            {
                Debug.LogError("First encounter is missing");
            }
            if (BasicEncounters == null || BasicEncounters.Length == 0 || BasicEncounters.Any(e => e == null))
            {
                Debug.LogError("Basic encounters are missing");
            }
            if (MediumEncounters == null || MediumEncounters.Length == 0 || MediumEncounters.Any(e => e == null))
            {
                Debug.LogError("Medium encounters are missing");
            }
            if (AdvancedEncounters == null || AdvancedEncounters.Length == 0 || AdvancedEncounters.Any(e => e == null))
            {
                Debug.LogError("Advanced encounters are missing");
            }
        }
    }
}