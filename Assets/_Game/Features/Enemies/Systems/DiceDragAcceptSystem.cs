using _Game.Features.Dice;
using Code.Common;
using Code.Features.DragAndDrop;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Enemies
{
    public class DiceDragAcceptSystem : ISystem
    {
        private EventReceiver<WT, DragAccepted> _dragAccepted;
        public void Init()
        {
            _dragAccepted = W.RegisterEventReceiver<DragAccepted>();
        }
        public void Update()
        {
            foreach (var draggedEvent in _dragAccepted)
            {
                var dragAccepted = draggedEvent.Value;
                if (dragAccepted.TargetContainer.TryUnpack<WT>(out var targetContainer) &&
                    targetContainer.Has<EnemyCountdown>() &&
                    dragAccepted.Draggable.TryUnpack<WT>(out var draggable) &&
                    draggable.Has<DiceValue>())
                {
                    targetContainer.Mut<EnemyCountdown>().Value -= draggable.Read<DiceValue>().Value;
                    draggable.Set<Destroyed>();
                }
            }
        }

        public void Destroy()
        {
            W.DeleteEventReceiver(ref _dragAccepted);
        }
    }
}