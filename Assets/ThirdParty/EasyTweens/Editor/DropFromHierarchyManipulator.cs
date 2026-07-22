using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTweens
{
    public class DropFromHierarchyManipulator : PointerManipulator
    {
        private readonly TweenEditor _targetEditor;
        private TweenAnimation _mainAnimation;
        private StyleColor _originalBackgroundColor;

    
        public DropFromHierarchyManipulator(TweenEditor targetEditor, TweenAnimation mainAnimation)
        {
            _targetEditor = targetEditor;
            _mainAnimation = mainAnimation;
            target = targetEditor.Q<VisualElement>("Root");
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

            var objectReference = DragAndDrop.objectReferences[0];

            if (objectReference is not GameObject) return;
            
            var gameObject = (GameObject) objectReference;
            if (gameObject == null) return;

            var tweenType = _targetEditor.Tween.GetType();
            var targetField = tweenType.GetField("target");

            if (targetField != null)
            {
                var component = gameObject.GetComponent(targetField.FieldType);
                if (component != null)
                {
                    Undo.RecordObject(_mainAnimation, "Set target");
                    targetField.SetValue(_targetEditor.Tween, component);
                }
            }
            else 
            {
                Type targetInterface = tweenType.GetInterfaces().FirstOrDefault(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(ITargetSetter<>));

                if (targetInterface != null)
                {
                    MethodInfo setTargetMethod = targetInterface.GetMethod("SetTarget");
                    Type targetType = targetInterface.GetGenericArguments()[0];

                    if (targetType == typeof(GameObject))
                    {
                        Undo.RecordObject(_mainAnimation, "Set target");
                        setTargetMethod.Invoke(_targetEditor.Tween, new object[] {gameObject});
                    }
                    else if (typeof(Component).IsAssignableFrom(targetType) &&
                             gameObject.GetComponent(targetType) != null)
                    {
                        Undo.RecordObject(_mainAnimation, "Set target");

                        setTargetMethod.Invoke(_targetEditor.Tween,
                            new object[] { gameObject.GetComponent(targetType) });
                    }
                }
                
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
            if (DragAndDrop.objectReferences[0] is not GameObject) return;
            target.style.backgroundColor = new StyleColor(new Color(0.52f,0.912f,0.23f, 0.3f));
        }
    }
}