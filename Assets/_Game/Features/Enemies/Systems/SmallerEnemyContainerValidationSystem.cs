using _Game.Features.Dice;
using Code.Features.DragAndDrop;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Enemies
{
    public class SmallerEnemyContainerValidationSystem : ISystem
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
                    targetContainer.HasModifier<AcceptSmaller>())
                {
                    if (targetContainer.TryGetModifier<AcceptSmaller>(out var acceptSmaller))
                    {
                        var value = acceptSmaller.Value;
                        if (!request.Draggable.TryUnpack<WT>(out var draggable) || 
                            !draggable.Has<DiceValue>() ||
                            draggable.Read<DiceValue>().Value >= value)
                        {
                            requestEntity.Add<DragTransferRejected>();
                        }
                    }
                }
            }
        }
    }
}