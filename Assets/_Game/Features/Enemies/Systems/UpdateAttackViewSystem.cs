using FFS.Libraries.StaticEcs;

namespace _Game.Features.Enemies
{
    public class UpdateAttackViewSystem : ISystem
    {
        public void Update()
        {
            foreach (var e in W.Query<All<AttackViewLink, Attack>, AllChanged<Attack>>().Entities())
            {
                var attack = e.Read<Attack>();
                e.Read<AttackViewLink>().Value.SetAttack(attack.CurrentValue);
            }
        }
    }
}