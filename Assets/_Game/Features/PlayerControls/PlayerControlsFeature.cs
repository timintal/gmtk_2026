using Code.Common;

namespace _Game.Features.PlayerControls
{
    public static class PlayerControlsFeature
    {
        public static void AddToWorld()
        {
            GameSys.Add(new InputSystem(), Order.Input);

            FixedSys.Add(new ApplyForceSystem());
            FixedSys.Add(new BreakSystem());
            FixedSys.Add(new ClampSpeedSystem(), Order.Update + 1);
        }
    }
}