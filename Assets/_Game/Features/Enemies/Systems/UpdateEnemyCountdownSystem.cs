using FFS.Libraries.StaticEcs;

namespace _Game.Features.Enemies
{
    public class UpdateEnemyCountdownSystem : ISystem
    {
        public void Update()
        {
            foreach (var e in W.Query<All<EnemyCountdownViewLink, EnemyCountdown>, AllChanged<EnemyCountdown>>().Entities())
            {
                var countdown = e.Read<EnemyCountdown>();
                e.Read<EnemyCountdownViewLink>().Value.SetCountdown(countdown.Value);
            }
        }
    }
}