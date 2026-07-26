using Code.Features.DragAndDrop;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Enemies
{
    public class ShowDragRejectReasonSystem : ISystem
    {
        private EventReceiver<WT, DragEnded> _ended;
        public void Init()
        {
            _ended = W.RegisterEventReceiver<DragEnded>();
        }

        public void Update()
        {
            foreach (var dragEvent in _ended)
            {
                ref readonly var ended = ref dragEvent.Value;
                if (!ended.Accepted&& !string.IsNullOrEmpty(ended.RejectReason)) 
                {
                    W.GetResource<ErrorMessage>().Show(ended.RejectReason);
                }
            }
        }

        public void Destroy()
        {
            if (W.Status != WorldStatus.Initialized)
            {
                return;
            }

            W.DeleteEventReceiver(ref _ended);
        }
    }
}