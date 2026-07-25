using Code.Features.Stats;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace _Game.Features.Enemies
{
    public partial struct Enemy : ITag {  }
    public partial struct EnemyCountdownContainer : ITag {  }
    public partial struct EnemyCountdown : IComponent, ITrackableChanged { public int Value; }
    
    public partial struct ExactCountdown : ITag { }
    
    public partial struct Attack : IStat { 
        [field:SerializeField] public float BaseValue
        {
            get;
            set;
        }
        [field:SerializeField] public float CurrentValue
        {
            get;
            set;
        }
    }
    
}