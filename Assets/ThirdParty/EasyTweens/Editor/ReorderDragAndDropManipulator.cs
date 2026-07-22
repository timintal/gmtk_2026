using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTweens
{
    public class ReorderDragAndDropManipulator : PointerManipulator
    {
        private readonly TweenEditor tweenEditor;
        private readonly TweenAnimationEditor tweenAnimationEditor;

        private Vector2 targetStartPosition { get; set; }

        private Vector3 pointerStartPosition { get; set; }

        private bool enabled { get; set; }

        private VisualElement root { get; }

        private Vector2 boundsY;
    
        public ReorderDragAndDropManipulator(TweenEditor tweenEditor, TweenAnimationEditor tweenAnimationEditor)
        {
            this.tweenEditor = tweenEditor;
            this.tweenAnimationEditor = tweenAnimationEditor;
            target = tweenEditor.Q<VisualElement>("Dragger");
            root = tweenEditor.parent;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
            target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
            target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
            target.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);    
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
            target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
            target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
            target.UnregisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
        }
    
        private void PointerDownHandler(PointerDownEvent evt)
        {
            var translate = target.resolvedStyle.translate;
            targetStartPosition = new Vector2(translate.x, translate.y);
            pointerStartPosition = evt.position;
            target.CapturePointer(evt.pointerId);
            boundsY = new Vector2(-tweenEditor.layout.y, root.worldBound.height - tweenEditor.layout.y - tweenEditor.layout.height);
            enabled = true;
        }
    
        private void PointerMoveHandler(PointerMoveEvent evt)
        {
            if (enabled && target.HasPointerCapture(evt.pointerId))
            {
                Vector3 pointerDelta = evt.position - pointerStartPosition;
            
                tweenEditor.style.translate = new Translate(
                    targetStartPosition.x,
                    Mathf.Clamp(targetStartPosition.y + pointerDelta.y, boundsY.x, boundsY.y),
                    0);

                var positionChange = GetCurrentPositionChange();
            
                tweenAnimationEditor.UpdateDragging(tweenEditor, Mathf.RoundToInt(positionChange));
            }
        }
    
        private void PointerUpHandler(PointerUpEvent evt)
        {
            if (enabled && target.HasPointerCapture(evt.pointerId))
            {
                target.ReleasePointer(evt.pointerId);
            }
        }
    
        private void PointerCaptureOutHandler(PointerCaptureOutEvent evt)
        {
            if (enabled)
            {
                var positionChange = GetCurrentPositionChange();

                tweenAnimationEditor.StopDragging(tweenEditor, Mathf.RoundToInt(positionChange));
            
                enabled = false;
            }
        }

        private int GetCurrentPositionChange()
        {
            float offset = tweenEditor.resolvedStyle.translate.y;

            int direction = (int) Mathf.Sign(offset);

            int positionChange = 0;
        
            while (offset * direction > 0 && CanMoveTween(positionChange + direction))
            {
                var nextHeight = HeightAtPosition(positionChange + direction);

                if (nextHeight * 0.5f < Mathf.Abs(offset))
                {
                    positionChange += direction;
                    offset -= direction * nextHeight;
                }
                else
                {
                    break;
                }
            }

            return positionChange;
        }

        bool CanMoveTween(int positionChange)
        {
            var currIndex = tweenAnimationEditor.TweenEditors.IndexOf(tweenEditor);

            if (currIndex >= 0 &&
                currIndex + positionChange >= 0 &&
                currIndex + positionChange < tweenAnimationEditor.TweenEditors.Count)
                return true;

            return false;
        }

        float HeightAtPosition(int positionChange)
        {
            var currIndex = tweenAnimationEditor.TweenEditors.IndexOf(tweenEditor);
            return tweenAnimationEditor.TweenEditors[currIndex + positionChange].layout.height;
        }
    }
}