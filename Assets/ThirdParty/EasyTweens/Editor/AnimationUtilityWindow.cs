using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace EasyTweens
{
    public class AnimationUtilityWindow : EditorWindow
    {
        private TweenAnimationEditor currentAnimationEditor;
        public TweenAnimation tweenAnimation;
        
        public bool IsPinned { get; private set; }
        public event Action<AnimationUtilityWindow>  OnDestroyEvent;

        public static AnimationUtilityWindow ShowWindow(TweenAnimationEditor e, bool asUtility = false)
        {
            var window = GetWindow<AnimationUtilityWindow>(asUtility , "Animation Utility", true);
            
            if (asUtility && window.IsPinned)
            {
                window.OnDestroyEvent = null;
                window.Close();
                window = GetWindow<AnimationUtilityWindow>(asUtility , "Animation Utility", true);
                
            }
            window.IsPinned = !asUtility;
            window.tweenAnimation = e.Animation;
            window.CreateGUI();
            return window;
        }

        private void CreateGUI()
        {
            rootVisualElement.Clear();
            rootVisualElement.style.flexDirection = FlexDirection.Column;
            
            if (tweenAnimation != null)
            {
                ScrollView view = new ScrollView();
                rootVisualElement.Add(view);
                
                currentAnimationEditor = (TweenAnimationEditor)UnityEditor.Editor.CreateEditor(tweenAnimation, typeof(TweenAnimationEditor));
                currentAnimationEditor.IsInWindow = true;
                
                var inspectorGUI = currentAnimationEditor.CreateInspectorGUI();
                inspectorGUI.Q<Button>("OpenUtilityWindow").visible = false;
                inspectorGUI.Q<Button>("OpenUtilityWindowPinnable").visible = false;
                inspectorGUI.Q<ScrollView>("ContainerScroll").style.maxHeight = 700;
                inspectorGUI.Q<ScrollView>("ContainerScroll").style.flexShrink = 1;
                
                inspectorGUI.style.flexGrow = 1;
                inspectorGUI.style.top = 0;
                view.Add(inspectorGUI);
            }
        }

        public void Clear()
        {
            if (currentAnimationEditor != null)
                DestroyImmediate(currentAnimationEditor);

            tweenAnimation = null;
            
            rootVisualElement.Clear();

        }

        private void OnDisable()
        {
            Clear();
        }

        private void OnDestroy()
        {
            OnDestroyEvent?.Invoke(this);
        }
    }
}