using Code.Common;

namespace _Game.Features.Blessings
{
    public static class BlessingsFeature
    {
        public static void AddToWorld()
        {
            GameSys.Add(new RefreshBlessingsLayoutSystem(), Order.PreUpdate);
            
            GameSys.Add(new GaterBlessingTargetsSystem());
            
            GameSys.Add(new ActivateAddBlessingSystem(), Order.Update + 1);
            GameSys.Add(new ActivateMultiplyBlessingSystem(), Order.Update + 1);
            GameSys.Add(new ActivateRerollBlessingSystem(), Order.Update + 1);
            GameSys.Add(new ActivateDrawBlessingSystem(), Order.Update + 1);
            
            GameSys.Add(new AddBlessingsVisualSystem(), Order.Cleanup);
            GameSys.Add(new CleanupBlessingVisualSystem(), Order.Cleanup);
            
            GameSys.Add(new CleanupActivatedBlessing(), Order.Cleanup);
        }
    }
}