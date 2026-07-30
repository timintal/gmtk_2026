using _Game.Infrastructure.ECS;
using Code.Ecs;

namespace Code.Features.EnergyFeature
{
    public class EnergyFeature : IFeature
    {
        public EnergyFeature(ISystemFactory systems)
        {
            GameSys.Add(systems.Create<UpdateEnergyViewSystem>());
            GameSys.Add(systems.Create<CheckEnergySystem>());
        }
    }
}