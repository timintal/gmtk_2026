using FFS.Libraries.StaticEcs;

namespace _Game.Features.PlayerControls
{
    public partial struct Force : IComponent { public float Value; }
    public partial struct MaxSpeed : IComponent { public float Value; }
    public partial struct MaxRotationSpeed : IComponent { public float Value; }
    public partial struct BreakForce : IComponent { public float Value; }
    public partial struct HasInput : ITag{}
    public partial struct Player : ITag{}
}