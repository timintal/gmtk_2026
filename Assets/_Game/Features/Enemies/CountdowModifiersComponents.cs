using FFS.Libraries.StaticEcs;

namespace _Game.Features.Enemies
{
    public enum CountdownModifierType
    {
        None,
        Even,
        Odd,
        Bigger,
        Smaller
    }

    
    public struct CountdownModifiers : ILinksType{}
    public partial struct CountdownModifier : IComponent{ public CountdownModifierType Type;}
    
    //Modifiers
    public partial struct AcceptOnlyEven : ITag {  }
    public partial struct AcceptOnlyOdd : ITag {  }
    
    public partial struct AcceptBigger : IComponent{public int Value;}
    public partial struct AcceptSmaller : IComponent{public int Value;}
}