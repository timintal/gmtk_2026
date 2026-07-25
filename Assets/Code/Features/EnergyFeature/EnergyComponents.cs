using FFS.Libraries.StaticEcs;

namespace Code.Features.EnergyFeature
{
    public partial struct Energy : IComponent, ITrackableChanged { public int Value; }
    public partial struct MaxEnergy : IComponent { public int Value; }
}