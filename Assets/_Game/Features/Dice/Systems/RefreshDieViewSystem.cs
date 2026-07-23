using _Game.Features.Dice.View;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Dice.Systems
{
    public class RefreshDieViewSystem : ISystem
    {
        public void Update()
        {
            foreach (var e in W.Query<All<Dice, DieViewLink, DiceValue>, AllChanged<DiceValue>>().Entities())
            {
                e.Read<DieViewLink>().Value.SetValue(e.Read<DiceValue>().Value);
            }
        }
    }
}