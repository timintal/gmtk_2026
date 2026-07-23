namespace Code.Features.DragAndDrop
{
    public static class DragAndDropFeature
    {
        public static void AddToWorld()
        {
            GameSys.Add(new ReleaseDestroyedContainerItemsSystem(), DragAndDropSystemOrder.ReleaseDestroyed);
            GameSys.Add(new DragInputSystem(), DragAndDropSystemOrder.Input);
            GameSys.Add(new DragFollowSystem(), DragAndDropSystemOrder.Follow);
            GameSys.Add(new DragTransferCoreValidationSystem(), DragAndDropSystemOrder.CoreValidation);
            GameSys.Add(new DragTransferResolveSystem(), DragAndDropSystemOrder.Resolve);
            GameSys.Add(new LineContainerLayoutSystem(), DragAndDropSystemOrder.Layout);
            GameSys.Add(new GridContainerLayoutSystem(), DragAndDropSystemOrder.Layout);
            GameSys.Add(new FreeContainerLayoutSystem(), DragAndDropSystemOrder.Layout);
        }
        
        public static void RejectRequest(W.Entity requestEntity, DragTransferRejectReason reason)
        {
            requestEntity.Set(new DragTransferRejected { Reason = reason });
        }
    }
}
