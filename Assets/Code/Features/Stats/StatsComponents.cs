using FFS.Libraries.StaticEcs;
using UnityEngine;

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

    public static class StatExtensions
    {
        public static int Rounded<T>(this T stat) where T : struct, IStat
            => Mathf.RoundToInt(stat.CurrentValue);
    }
}