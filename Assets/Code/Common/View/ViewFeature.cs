namespace Code.Common.View
{
    public static class ViewFeature
    {
        public static void AddToWorld()
        {
            GameSys.Add(new SyncPositionFromTransformSystem(), Order.Init);
            
            GameSys.Add(new CreateViewSystem(), Order.LateUpdate);
            GameSys.Add(new SyncTransformFromPositionSystem(), Order.LateUpdate);
            GameSys.Add(new WorldSpaceUiFollowSystem(), Order.LateUpdate);
            GameSys.Add(new InitRigidbodySystem(), Order.LateUpdate);
            GameSys.Add(new InitializeViewSystem(), Order.LateUpdate);
            GameSys.Add(new CleanupViewSystem(), Order.Cleanup - 10);
        }
    }
}