namespace Code.Features.EnergyFeature
{
    public static class EnergyFeature
    {
        public static void AddToWorld()
        {
            GameSys.Add(new UpdateEnergyViewSystem());
            GameSys.Add(new CheckEnergySystem());
        }
    }
}