using FFS.Libraries.StaticEcs;

namespace _Game.Features.Run
{
    public struct StartNewRunRequest : ITag{}
    public struct StartNewTurnRequest : ITag{}
    public struct StartNewLevelRequest : ITag { }
    
    public struct DrawCardRequest : IComponent { public int Value;}

}