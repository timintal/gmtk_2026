using FFS.Libraries.StaticEcs;

namespace _Game.Features.Dice
{
    public class PlayerState : IResource
    {
        public int RerollsCount = 999;
        public int CurrentLevel = 1;
        public int BaseDrawPerTurn = 3;
        
        public int NextDrawCount = 3;
    }
}