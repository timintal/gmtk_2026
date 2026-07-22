using FFS.Libraries.StaticEcs;

namespace Code.Features.DragAndDrop
{
    public sealed class DragTransferCoreValidationSystem : ISystem
    {
        public void Update()
        {
            foreach (var requestEntity in W.Query<All<DragTransferRequest>>().Entities())
            {
                if (requestEntity.Has<DragTransferRejected>())
                {
                    continue;
                }

                ref readonly var request = ref requestEntity.Read<DragTransferRequest>();

                if (!request.Draggable.TryUnpack<WT>(out var draggable) || !draggable.Has<Draggable>())
                {
                    Reject(requestEntity, DragTransferRejectReason.InvalidDraggable);
                    continue;
                }

                if (draggable.Read<Draggable>().Disabled)
                {
                    Reject(requestEntity, DragTransferRejectReason.InvalidDraggable);
                    continue;
                }

                if (request.HasSourceContainer)
                {
                    if (!request.SourceContainer.TryUnpack<WT>(out var sourceContainer)
                        || !sourceContainer.Has<DragContainer>()
                        || sourceContainer.Read<DragContainer>().Disabled)
                    {
                        Reject(requestEntity, DragTransferRejectReason.InvalidSourceContainer);
                        continue;
                    }

                }

                if (!request.HasTargetContainer)
                {
                    Reject(requestEntity, DragTransferRejectReason.NoTargetContainer);
                    continue;
                }

                if (!request.TargetContainer.TryUnpack<WT>(out var targetContainer)
                    || !targetContainer.Has<DragContainer>())
                {
                    Reject(requestEntity, DragTransferRejectReason.InvalidTargetContainer);
                    continue;
                }

                if (targetContainer.Read<DragContainer>().Disabled)
                {
                    Reject(requestEntity, DragTransferRejectReason.InvalidTargetContainer);
                    continue;
                }

                if (request.HasSourceContainer && request.SourceContainer == request.TargetContainer)
                {
                    continue;
                }

                ref readonly var container = ref targetContainer.Read<DragContainer>();
                if (container.Capacity > 0 && DragContainerRelations.CountItems(request.TargetContainer, request.Draggable) >= container.Capacity)
                {
                    Reject(requestEntity, DragTransferRejectReason.ContainerFull);
                }
            }
        }

        private static void Reject(W.Entity requestEntity, DragTransferRejectReason reason)
        {
            requestEntity.Set(new DragTransferRejected { Reason = reason });
        }

    }
}
