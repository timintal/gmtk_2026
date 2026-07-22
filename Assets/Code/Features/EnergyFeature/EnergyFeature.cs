using Code.Common;

namespace Code.Features.EnergyFeature
{
    public static class EnergyFeature
    {
        public static void AddToWorld()
        {
            GameSys.Add(new CheckEnergySystem(), Order.Update);
        }
    }
}