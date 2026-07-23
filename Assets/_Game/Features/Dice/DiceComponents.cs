using FFS.Libraries.StaticEcs;

namespace _Game.Features.Dice
{
    public partial struct Dice : ITag { }
    public partial struct RerollRequest : IComponent { 
        public int DiceCount;
        public bool ClearCurrent;
    }
    public partial struct DiceValue : IComponent, ITrackableChanged { public int Value; }
        
    public partial struct DieValueUpdated : IEvent { public int Value; }
    
    public partial struct RolledDicesContainer : IComponent {  }
        
}