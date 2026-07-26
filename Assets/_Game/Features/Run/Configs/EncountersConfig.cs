using System;
using System.Linq;
using _Game.Features.Enemies;
using FFS.Libraries.StaticEcs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Features.Run.Configs
{
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
        
        [SerializeField] EncounterInfo bossEncounter;
        
        [SerializeField] int MediumEncountersLevel;
        [SerializeField] int AdvancedEncountersLevel;
        [SerializeField] public int BossLevel;
        

        public EncounterInfo GetRandomEncounter(int level)
        {
            if (level == 1) return firstEncounter;
            
            if (level < MediumEncountersLevel)
                return BasicEncounters[UnityEngine.Random.Range(0, BasicEncounters.Length)];
            
            if (level >= MediumEncountersLevel && level < AdvancedEncountersLevel)
                return MediumEncounters[UnityEngine.Random.Range(0, MediumEncounters.Length)];
            
            if (level >= AdvancedEncountersLevel && level < BossLevel)
                return AdvancedEncounters[UnityEngine.Random.Range(0, AdvancedEncounters.Length)];
            
            if (level >= BossLevel)
                return bossEncounter;
            
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
    [Serializable]
    public class CountdownModifierData
    {
        public CountdownModifierType Type;
        public int Value;
    }
    [Serializable]
    public class EnemySettings
    {
        public WTEntityProvider Prefab;
        public CountdownModifierData[] CountdownModifiers;
        
        public int CurrentCountdown;
        public int Attack;
    }

    [Serializable]
    public class EncounterInfo
    {
        public EnemySettings[] Enemies;

        public string Description()
        {
            int hp = 0;
            int attack = 0;

            if (Enemies == null) return "";
            string result = "";
            #if UNITY_EDITOR
            foreach (var p in Enemies)
            {
                if (p == null || p.Prefab == null) continue;
                var prefab = p.Prefab;
                result += prefab.name + "/";
                hp += p.CurrentCountdown;
                attack += p.Attack;
            }
            #endif

            return result + $" hp:{hp}, attack{attack}";
        }
    }

}