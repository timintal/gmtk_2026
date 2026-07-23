using FFS.Libraries.StaticEcs;

namespace _Game.Features.Dice.Systems
{
    public class InitRunSystem : ISystem
    {
        public void Init()
        {
            W.SetResource(new PlayerState()
            {
                RerollsCount = 3,
                DicePerRoll = 5
            });
        }
    }
}