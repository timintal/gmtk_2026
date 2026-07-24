using _Game.Features.Dice.Systems;
using Code.Common;

namespace _Game.Features.Dice
{
    public static class DiceFeature
    {
        public static void AddToWorld()
        {
            GameSys.Add(new RefreshDieViewSystem());
            GameSys.Add(new PerformRerollSystem());
        }
    }
}