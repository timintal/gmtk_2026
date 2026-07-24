using System;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace _Game.Features.Run.Configs
{
    [Serializable]
    public class EncounterInfo
    {
        public WTEntityProvider[] Enemies;
    }

    [CreateAssetMenu(fileName = "EncountersConfig", menuName = "Run/EncountersConfig", order = 0)]
    public class EncountersConfig : ScriptableObject, IResource
    {
        [SerializeField] EncounterInfo[] BasicEncounters;
        [SerializeField] EncounterInfo[] AdvancedEncounters;
        [SerializeField] int AdvancedEncountersLevel;
        

        public EncounterInfo GetRandomEncounter(int level)
        {
            if (level >= AdvancedEncountersLevel)
                return AdvancedEncounters[UnityEngine.Random.Range(0, AdvancedEncounters.Length)];
            
            return BasicEncounters[UnityEngine.Random.Range(0, BasicEncounters.Length)];
        }
    }
}