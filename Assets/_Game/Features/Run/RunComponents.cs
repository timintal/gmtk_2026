using FFS.Libraries.StaticEcs;

namespace _Game.Features.Run
{
    public struct LevelStarted : ITag{}
    public struct ActiveTurn : ITag{}
    public struct StartNewRunRequest : ITag{}
    public struct StartNewTurnRequest : ITag{}
    public struct EndTurnRequest : ITag{}
    public struct StartNewLevelRequest : ITag { }
    
    public struct ShowRewardsRequest : ITag { }
    
    public struct DrawCardRequest : IComponent { public int Value;}

}