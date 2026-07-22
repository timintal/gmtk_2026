using FFS.Libraries.StaticEcs;

namespace Code.Features.Stats
{
    public interface IStat : IComponent, ITrackableAdded, ITrackableChanged
    {
        float BaseValue { get; set; }
        float CurrentValue { get; set; }
    }
    
    public interface IStatModifier<T> : IComponent where T : IStat
    {
        float Additive { get; set; }
        float Multiplicative{ get; set; } 
    }
}