using Code.Features.EnergyFeature.View;
using FFS.Libraries.StaticEcs;

namespace Code.Features.EnergyFeature
{
    public class UpdateEnergyViewSystem : ISystem
    {
        public void Update()
        {
            foreach (var e in W.Query<All<Energy, MaxEnergy, EnergyProgressViewLink>, AllChanged<Energy>>().Entities())
            {
                var energy = e.Read<Energy>().Value;
                var maxEnergy = e.Read<MaxEnergy>().Value;
                var view = e.Read<EnergyProgressViewLink>().Value;
                view.SetEnergy(energy, maxEnergy, true, 0.5f);
            }
        }
    }
}