using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTweens
{
    public class DragLinkManipulator : PointerManipulator
    {
        private readonly TweenAnimationEditor tweenAnimationEditor;
        private readonly TweenEditor tweenEditor;
        private Vector3 pointerStartPosition;
        private Vector3 _pointerDelta;
        private TweenEditor _selectedEditor;
        private StyleColor _originalBackgroundColor;
        private StyleColor _originalLinkVisualElementColor;
        private float selfTopOffset;
        private float selfBottomOffset;
        public bool enabled { get; set; }


        public DragLinkManipulator(VisualElement targetElement, TweenEditor te, TweenAnimationEditor animationEditor)
        {
            target = targetElement;
            tweenEditor = te;
            tweenAnimationEditor = animationEditor;
        }


        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(PointerDown);
            target.RegisterCallback<PointerMoveEvent>(PointerMove);
            target.RegisterCallback<PointerUpEvent>(PointerUp);
            target.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(PointerDown);
            target.UnregisterCallback<PointerMoveEvent>(PointerMove);
            target.UnregisterCallback<PointerUpEvent>(PointerUp);
            target.UnregisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
        }

        private void PointerCaptureOutHandler(PointerCaptureOutEvent evt)
        {
            if (enabled)
            {
                if (_selectedEditor != null)
                {
                    _selectedEditor.style.backgroundColor = _originalBackgroundColor;

                    tweenAnimationEditor.Animation.SetTweenLink(_selectedEditor.Tween, tweenEditor.Tween);
                    tweenAnimationEditor.RefreshDelayDurationsHandles();
                }

                enabled = false;
            }

            _selectedEditor = null;
        }

        private void PointerDown(PointerDownEvent evt)
        {
            pointerStartPosition = evt.position;
            selfTopOffset = evt.position.y - tweenEditor.worldBound.y;
            selfBottomOffset = -evt.position.y + tweenEditor.worldBound.y + tweenEditor.worldBound.height;

            target.CapturePointer(evt.pointerId);
            _originalLinkVisualElementColor = target.style.backgroundColor;
            target.style.backgroundColor = new StyleColor(new Color(0.52f, 0.912f, 0.23f, 1));
            enabled = true;
        
            var linkedTween = tweenAnimationEditor.Animation.GetTweenById(tweenEditor.Tween.LinkedTweenGuid);
            if (linkedTween != null)
            {
                var linkedTweenEditor = tweenAnimationEditor.TweenEditors.Find(editor => editor.Tween == linkedTween);
                if (linkedTweenEditor != null)
                {
                    linkedTweenEditor.Q<VisualElement>("Root").AddToClassList("selected-tween-editor");
                }
            }
        }

        private void PointerUp(PointerUpEvent evt)
        {
            if (target.HasPointerCapture(evt.pointerId))
            {
                target.ReleasePointer(evt.pointerId);
            }

            target.style.backgroundColor = _originalLinkVisualElementColor;

            if ((evt.position - pointerStartPosition).magnitude < 5)
            {
                if (!string.IsNullOrEmpty(tweenEditor.Tween.LinkedTweenGuid))
                    ShowUnlinkContextMenu();
                else
                    ShowLinkHelpMessage();
            }
        
            var linkedTween = tweenAnimationEditor.Animation.GetTweenById(tweenEditor.Tween.LinkedTweenGuid);
            if (linkedTween != null)
            {
                var linkedTweenEditor = tweenAnimationEditor.TweenEditors.Find(editor => editor.Tween == linkedTween);
                if (linkedTweenEditor != null)
                {
                    linkedTweenEditor.Q<VisualElement>("Root").RemoveFromClassList("selected-tween-editor");
                }
            }
        }

        private void PointerMove(PointerMoveEvent evt)
        {
            if (enabled && target.HasPointerCapture(evt.pointerId))
            {
                _pointerDelta = evt.position - pointerStartPosition;

                var positionChange = GetCurrentPositionChange(_pointerDelta.y);
                var currIndex = tweenAnimationEditor.TweenEditors.IndexOf(tweenEditor);
                var nextIndex = currIndex + positionChange;

                if (positionChange != 0 && nextIndex >= 0 && nextIndex < tweenAnimationEditor.TweenEditors.Count)
                {
                    var newSelectedEditor = tweenAnimationEditor.TweenEditors[nextIndex];
                    if (newSelectedEditor != _selectedEditor)
                    {
                        if (_selectedEditor != null)
                            _selectedEditor.style.backgroundColor = _originalBackgroundColor;
                        ;

                        _selectedEditor = newSelectedEditor;
                        if (_selectedEditor != null)
                        {
                            _originalBackgroundColor = _selectedEditor.style.backgroundColor;
                            _selectedEditor.style.backgroundColor = new StyleColor(new Color(0.52f, 0.912f, 0.23f, 0.4f));
                        }
                    }
                }
                else
                {
                    if (_selectedEditor != null)
                    {
                        _selectedEditor.style.backgroundColor = _originalBackgroundColor;
                        _selectedEditor = null;
                    }
                }
            }
        }

        void ShowLinkHelpMessage()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddDisabledItem(new GUIContent($"Press and drag this handle to another tween to link to it. \n Tween will start after the linked tween ends."));
            menu.ShowAsContext();
        }

        private void ShowUnlinkContextMenu()
        {
            var linkedTween = tweenAnimationEditor.Animation.GetTweenById(tweenEditor.Tween.LinkedTweenGuid);
            if (linkedTween == null)
            {
                tweenEditor.Tween.LinkedTweenGuid = string.Empty;
                tweenAnimationEditor.RefreshDelayDurationsHandles();
                return;
            }

            GenericMenu menu = new GenericMenu();
            menu.AddDisabledItem(
                new GUIContent(
                    $"Linked to {linkedTween.GetType().Name} at index: {tweenAnimationEditor.Animation.tweens.IndexOf(linkedTween) + 1}"));

            menu.AddItem(new GUIContent("[x] Unlink"), false, () =>
            {
                tweenEditor.Tween.LinkedTweenGuid = string.Empty;
                tweenAnimationEditor.RefreshDelayDurationsHandles();
            });
            menu.ShowAsContext();
        }

        private int GetCurrentPositionChange(float offset)
        {
            int direction = (int) Mathf.Sign(offset);

            int positionChange = 0;

            if (direction > 0)
            {
                offset -= selfBottomOffset;
                if (offset < 0)
                    return 0;
            }
            else
            {
                offset += selfTopOffset;
                if (offset > 0)
                    return 0;
            }

            while (offset * direction > 0)
            {
                var nextHeight = HeightAtPosition(positionChange + direction);

                if (nextHeight * 0.1f < Mathf.Abs(offset))
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

        float HeightAtPosition(int positionChange)
        {
            var currIndex = tweenAnimationEditor.TweenEditors.IndexOf(tweenEditor);
            var clampedIndex = Mathf.Clamp(currIndex + positionChange, 0, tweenAnimationEditor.TweenEditors.Count - 1);
            return tweenAnimationEditor.TweenEditors[clampedIndex].layout.height;
        }
    }
}