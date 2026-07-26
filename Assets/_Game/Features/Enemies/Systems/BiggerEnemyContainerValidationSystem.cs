using _Game.Features.Dice;
using Code.Features.DragAndDrop;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Enemies
{
    public class BiggerEnemyContainerValidationSystem : ISystem
    {
        public void Update()
        {
            foreach (var requestEntity in W.Query<All<DragTransferRequest>>().Entities())
            {
                if (requestEntity.Has<DragTransferRejected>())
                {
                    continue;
                }

                var request = requestEntity.Read<DragTransferRequest>();
                
                if (request.TargetContainer.TryUnpack<WT>(out var targetContainer) && 
                    targetContainer.HasModifier<AcceptBigger>())
                {
                    if (targetContainer.TryGetModifier<AcceptBigger>(out var acceptBigger))
                    {
                        var value = acceptBigger.Value;
                        if (!request.Draggable.TryUnpack<WT>(out var draggable) ||
                            !draggable.Has<DiceValue>() ||
                            draggable.Read<DiceValue>().Value <= value)
                        {
                            requestEntity.Set(new DragTransferRejected()
                            {
                                Reason = DragTransferRejectReason.Custom,
                                Message = "Only dice bigger than " + value + " can be used to this creature"
                            });
                        }
                    }
                }
            }
        }
    }
}