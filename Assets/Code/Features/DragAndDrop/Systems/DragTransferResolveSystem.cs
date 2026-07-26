using System.Collections.Generic;
using Code.Common;
using FFS.Libraries.StaticEcs;

namespace Code.Features.DragAndDrop
{
    public sealed class DragTransferResolveSystem : ISystem
    {
        private readonly List<W.Entity> _requests = new();

        public void Update()
        {
            _requests.Clear();
            foreach (var requestEntity in W.Query<All<DragTransferRequest>>().Entities())
            {
                _requests.Add(requestEntity);
            }

            for (var i = 0; i < _requests.Count; i++)
            {
                Resolve(_requests[i]);
            }
        }

        private static void Resolve(W.Entity requestEntity)
        {
            ref readonly var request = ref requestEntity.Read<DragTransferRequest>();
            var rejected = requestEntity.Has<DragTransferRejected>();

            if (request.Draggable.TryUnpack<WT>(out var draggable) && draggable.Has<Draggable>())
            {
                if (rejected)
                {
                    Rollback(request, draggable);
                }
                else
                {
                    ApplyTransfer(request, draggable);
                }

                if (draggable.Has<Dragging>())
                {
                    draggable.Delete<Dragging>();
                }
            }

            W.SendEvent(new DragEnded
            {
                Draggable = request.Draggable,
                TargetContainer = request.TargetContainer,
                Accepted = !rejected && request.HasTargetContainer,
                RejectReason = rejected ? requestEntity.Read<DragTransferRejected>().Message : string.Empty
            });

            requestEntity.Destroy();
        }

        private static void ApplyTransfer(in DragTransferRequest request, W.Entity draggable)
        {
            var source = request.SourceContainer;
            var hasSource = request.HasSourceContainer;

            if (DragContainerRelations.TryGetContainer(draggable, out var currentContainer))
            {
                source = currentContainer;
                hasSource = true;
            }

            DragContainerRelations.PlaceInContainer(draggable, request.TargetContainer);

            if (hasSource)
            {
                DragContainerRelations.MarkLayoutDirty(source);
            }

            DragContainerRelations.MarkLayoutDirty(request.TargetContainer);
        }

        private static void Rollback(in DragTransferRequest request, W.Entity draggable)
        {
            var source = request.SourceContainer;
            var hasSource = request.HasSourceContainer;

            if (draggable.Has<Dragging>())
            {
                ref readonly var dragging = ref draggable.Read<Dragging>();
                source = dragging.SourceContainer;
                hasSource = dragging.HasSourceContainer;

                if (draggable.Has<Position>())
                {
                    draggable.Ref<Position>().Value = dragging.OriginPosition;
                }
            }

            if (hasSource)
            {
                DragContainerRelations.MarkLayoutDirty(source);
            }
        }
    }
}
