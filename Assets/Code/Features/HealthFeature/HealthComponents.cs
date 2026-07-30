using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticEcs.Unity;

namespace Code.Features.HealthFeature
{
    public struct Damage : IComponent
    {
        public int Amount;
        public EntityGID Source;
        public EntityGID Target;
    }

    [StaticEcsEditorGroup("Health", "00FF00")]
    public struct Health : IComponent, ITrackableChanged
    {
        public int Value;
    }
    [StaticEcsEditorGroup("Health", "00FF00")]
    
    public struct InitHealthRequest : ITag { }
    [StaticEcsEditorGroup("Health", "00FF00")]
    
    public struct MaxHealth : IComponent
    {
        public int Value;
    }
}