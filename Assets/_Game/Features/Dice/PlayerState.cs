using FFS.Libraries.StaticEcs;

namespace _Game.Features.Dice
{
    public class PlayerState : IResource
    {
        public int RerollsCount;
        public int DicePerRoll;
    }
}