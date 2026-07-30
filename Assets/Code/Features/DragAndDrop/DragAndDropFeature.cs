using _Game.Infrastructure.ECS;
using Code.Ecs;

namespace Code.Features.DragAndDrop
{
    public  class DragAndDropFeature : IFeature
    {
        public DragAndDropFeature(ISystemFactory systems)
        {
            GameSys.Add(systems.Create<ReleaseDestroyedContainerItemsSystem>(), DragAndDropSystemOrder.ReleaseDestroyed);
            GameSys.Add(systems.Create<DragInputSystem>(), DragAndDropSystemOrder.Input);
            GameSys.Add(systems.Create<DragFollowSystem>(), DragAndDropSystemOrder.Follow);
            GameSys.Add(systems.Create<DragTransferCoreValidationSystem>(), DragAndDropSystemOrder.CoreValidation);
            GameSys.Add(systems.Create<DragTransferResolveSystem>(), DragAndDropSystemOrder.Resolve);
            GameSys.Add(systems.Create<LineContainerLayoutSystem>(), DragAndDropSystemOrder.Layout);
            GameSys.Add(systems.Create<GridContainerLayoutSystem>(), DragAndDropSystemOrder.Layout);
            GameSys.Add(systems.Create<FreeContainerLayoutSystem>(), DragAndDropSystemOrder.Layout);
        }
        
        public static void RejectRequest(W.Entity requestEntity, DragTransferRejectReason reason)
        {
            requestEntity.Set(new DragTransferRejected { Reason = reason });
        }
    }
}
