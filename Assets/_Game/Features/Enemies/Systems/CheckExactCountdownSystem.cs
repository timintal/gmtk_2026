using _Game.Features.PlayerControls;
using Code.Common;
using Code.Features.EnergyFeature;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Enemies
{
    public class CheckExactCountdownSystem : ISystem
    {
        public void Update()
        {
            foreach (var e in W.Query<All<Enemy, Destroyed, ExactCountdown>>().Entities())
            {
                W.GetResource<ExactCountdownView>().ExactCountdownFeedback();

                foreach (var playerEntity in W.Query<All<Player, Energy>>().Entities())
                {
                    playerEntity.Mut<Energy>().Value += 2;
                }
            }
        }
    }
}