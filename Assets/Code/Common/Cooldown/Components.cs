using FFS.Libraries.StaticEcs;

namespace Code.Common.Cooldown
{
    public interface ICooldown : IComponent { public float Value { get; set; } }
}