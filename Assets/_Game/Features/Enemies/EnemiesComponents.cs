using FFS.Libraries.StaticEcs;

namespace _Game.Features.Enemies
{
    public partial struct Enemy : ITag {  }
    public partial struct EnemyCountdownContainer : ITag {  }
    public partial struct EnemyCountdown : IComponent, ITrackableChanged { public int Value; }
    
}