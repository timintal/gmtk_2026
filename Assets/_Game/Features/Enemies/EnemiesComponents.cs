using Code.Features.Stats;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace _Game.Features.Enemies
{
    public partial struct Enemy : ITag {  }
    public partial struct EnemyCountdownContainer : ITag {  }
    public partial struct EnemyCountdown : IComponent, ITrackableChanged { public int Value; }
    
    public partial struct AcceptOnlyEven : ITag {  }
    public partial struct AcceptOnlyOdd : ITag {  }
    
    public partial struct AcceptBigger : IComponent{public int Value;}
    public partial struct AcceptSmaller : IComponent{public int Value;}
    
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