namespace Code.Common.Fx
{
    public static class FxFeature
    {
        public static void AddToWorld()
        {
            GameSys.Add(new PlayOneShotParticlesSystem(), Order.LateUpdate);
        }
        
    }
}