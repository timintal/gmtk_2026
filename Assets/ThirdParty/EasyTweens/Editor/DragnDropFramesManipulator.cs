using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTweens
{
    public class DragnDropFramesManipulator : PointerManipulator
    {
        private readonly SpriteSwapTweenEditor _targetEditor;
        private TweenAnimation _mainAnimation;
        private StyleColor _originalBackgroundColor;


        public DragnDropFramesManipulator(SpriteSwapTweenEditor targetEditor, TweenAnimation mainAnimation)
        {
            _targetEditor = targetEditor;
            _mainAnimation = mainAnimation;
            target = targetEditor.Q<VisualElement>("FramesList");
            _originalBackgroundColor = target.style.backgroundColor;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<DragEnterEvent>(DragEnter);
            target.RegisterCallback<DragLeaveEvent>(DragLeave);
            target.RegisterCallback<DragUpdatedEvent>(DragUpdated);
            target.RegisterCallback<DragPerformEvent>(DragPerform);
            target.RegisterCallback<DragExitedEvent>(DragExited);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<DragEnterEvent>(DragEnter);
            target.UnregisterCallback<DragLeaveEvent>(DragLeave);
            target.UnregisterCallback<DragUpdatedEvent>(DragUpdated);
            target.UnregisterCallback<DragPerformEvent>(DragPerform);
            target.UnregisterCallback<DragExitedEvent>(DragExited);
        }

        private void DragPerform(DragPerformEvent evt)
        {
            DragAndDrop.AcceptDrag();

            ResetColor();

            Undo.RecordObject(_mainAnimation, "Add frame");
            bool needUpdate = false;
            foreach (var reference in DragAndDrop.objectReferences)
            {
                if (reference is Sprite sprite)
                {
                    ((TweenSpriteSwap)_targetEditor.Tween).frames.Add(new FrameData
                    {
                        sprite = sprite,
                        relativeDuration = 1
                    });
                    needUpdate = true;
                }
            }
            if (needUpdate)
            {
                _targetEditor.UpdateFrameVisuals();
            }
            
        }

        private void DragUpdated(DragUpdatedEvent evt)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
        }

        private void DragLeave(DragLeaveEvent evt)
        {
            ResetColor();
        }

        private void DragExited(DragExitedEvent evt)
        {
            ResetColor();
        }

        public void ResetColor()
        {
            target.style.backgroundColor = _originalBackgroundColor;
        }

        private void DragEnter(DragEnterEvent evt)
        {
            if (DragAndDrop.objectReferences.Length == 0) return;
            foreach (var reference in DragAndDrop.objectReferences)
            {
                if (reference is Sprite)
                {
                    target.style.backgroundColor = new StyleColor(new Color(0.52f, 0.912f, 0.23f, 0.3f));
                    return;
                }
            }
        }
    }
    public class DragnDropImageFramesManipulator : PointerManipulator
    {
        private readonly ImageSpriteSwapTweenEditor _targetEditor;
        private TweenAnimation _mainAnimation;
        private StyleColor _originalBackgroundColor;


        public DragnDropImageFramesManipulator(ImageSpriteSwapTweenEditor targetEditor, TweenAnimation mainAnimation)
        {
            _targetEditor = targetEditor;
            _mainAnimation = mainAnimation;
            target = targetEditor.Q<VisualElement>("FramesList");
            _originalBackgroundColor = target.style.backgroundColor;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<DragEnterEvent>(DragEnter);
            target.RegisterCallback<DragLeaveEvent>(DragLeave);
            target.RegisterCallback<DragUpdatedEvent>(DragUpdated);
            target.RegisterCallback<DragPerformEvent>(DragPerform);
            target.RegisterCallback<DragExitedEvent>(DragExited);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<DragEnterEvent>(DragEnter);
            target.UnregisterCallback<DragLeaveEvent>(DragLeave);
            target.UnregisterCallback<DragUpdatedEvent>(DragUpdated);
            target.UnregisterCallback<DragPerformEvent>(DragPerform);
            target.UnregisterCallback<DragExitedEvent>(DragExited);
        }

        private void DragPerform(DragPerformEvent evt)
        {
            DragAndDrop.AcceptDrag();

            ResetColor();

            Undo.RecordObject(_mainAnimation, "Add frame");
            bool needUpdate = false;
            foreach (var reference in DragAndDrop.objectReferences)
            {
                if (reference is Sprite sprite)
                {
                    ((TweenSpriteSwap)_targetEditor.Tween).frames.Add(new FrameData
                    {
                        sprite = sprite,
                        relativeDuration = 1
                    });
                    needUpdate = true;
                }
            }
            if (needUpdate)
            {
                _targetEditor.UpdateFrameVisuals();
            }
            
        }

        private void DragUpdated(DragUpdatedEvent evt)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
        }

        private void DragLeave(DragLeaveEvent evt)
        {
            ResetColor();
        }

        private void DragExited(DragExitedEvent evt)
        {
            ResetColor();
        }

        public void ResetColor()
        {
            target.style.backgroundColor = _originalBackgroundColor;
        }

        private void DragEnter(DragEnterEvent evt)
        {
            if (DragAndDrop.objectReferences.Length == 0) return;
            foreach (var reference in DragAndDrop.objectReferences)
            {
                if (reference is Sprite)
                {
                    target.style.backgroundColor = new StyleColor(new Color(0.52f, 0.912f, 0.23f, 0.3f));
                    return;
                }
            }
        }
    }
}