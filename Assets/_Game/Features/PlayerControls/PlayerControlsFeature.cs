using _Game.Infrastructure.ECS;
using Code.Common;
using Code.Ecs;

namespace _Game.Features.PlayerControls
{
    public  class PlayerControlsFeature : IFeature
    {
        public PlayerControlsFeature(ISystemFactory systems)
        {
            GameSys.Add(systems.Create<InputSystem>(), Order.Input);

            FixedSys.Add(systems.Create<ApplyForceSystem>());
            FixedSys.Add(systems.Create<BreakSystem>());
            FixedSys.Add(systems.Create<ClampSpeedSystem>(), Order.Update + 1);
        }
    }
}