using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace EasyTweens
{
    public class TweenAnimatorController : MonoBehaviour
    {
        [SerializeField] private TweenAnimation defaultAnimation;

        [SerializeField] private List<TweenAnimation> animations = new();
        
        string currentAnimationName;

        private void OnEnable()
        {
            if (defaultAnimation != null)
            {
                currentAnimationName = defaultAnimation.name;
                defaultAnimation.Play();
            }
        }

        public void Play(string animationName, bool forcePlay = false)
        {
            foreach (var anim in animations)
            {
                if (anim.name == animationName)
                {
                    if (!forcePlay && currentAnimationName == animationName)
                    {
                        return;
                    }
                    currentAnimationName = animationName;
                    
                    anim.Play();
                }
                else
                {
                    anim.Stop();
                }
            }
        }

#if ODIN_INSPECTOR && UNITY_EDITOR
        
        [Sirenix.OdinInspector.Button,  Sirenix.OdinInspector.OnInspectorGUI]
        private void DrawAnimationButtons()
        {
            if (animations == null || animations.Count == 0)
                return;

            Sirenix.Utilities.Editor.SirenixEditorGUI.Title("Animation Controls", "", TextAlignment.Left, true);

            foreach (var anim in animations)
            {
                if (anim == null || string.IsNullOrEmpty(anim.name))
                    continue;

                if (GUILayout.Button($"Play: {anim.name}"))
                {
                    foreach (var otherAnimation in animations)
                    {
                        if (otherAnimation != anim)
                        {
                            otherAnimation.Stop();
                            otherAnimation.editorSubscriptionRetainCount = 0;
                            otherAnimation.UnsubscribeFromEditorUpdates();
                        }
                    }
                    if (anim.editorSubscriptionRetainCount == 0)
                        anim.SubscribeToEditorUpdates();
                    anim.SetDelay(0.01f).Play();
                }
            }
            if (GUILayout.Button($"Stop animation"))
            {
                foreach (var otherAnimation in animations)
                {
                    otherAnimation.Stop();
                    otherAnimation.editorSubscriptionRetainCount = 0;
                    otherAnimation.UnsubscribeFromEditorUpdates();
                }
            }
        }
        [Sirenix.OdinInspector.Button, Sirenix.OdinInspector.BoxGroup("Controls")]
#endif
        public void AddNewSpriteSheetAnimation(string name)
        {
            GameObject newAnimation = new GameObject(name);
            newAnimation.transform.SetParent(transform);
            newAnimation.transform.localPosition = Vector3.zero;
            var tweenAnimation = newAnimation.AddComponent<TweenAnimation>();
            animations.Add(tweenAnimation);

            var tweenSpriteSwap = new TweenSpriteSwap();
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                tweenSpriteSwap.target = spriteRenderer;
            }
            tweenSpriteSwap.Duration = 1;
            tweenSpriteSwap.curve = AnimationCurve.Linear(0, 0, 1, 1);
            tweenAnimation.tweens.Add(tweenSpriteSwap);
            tweenAnimation.lootType = LoopType.Loop;
            tweenAnimation.enabled = false;
#if UNITY_EDITOR
            Selection.activeObject = newAnimation;
            EditorGUIUtility.PingObject(newAnimation);
#endif
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button, Sirenix.OdinInspector.BoxGroup("Controls")]
#endif
        public void AddAnimation(string name)
        {
            GameObject newAnimation = new GameObject(name);
            newAnimation.transform.SetParent(transform);
            newAnimation.transform.localPosition = Vector3.zero;
            var tweenAnimation = newAnimation.AddComponent<TweenAnimation>();
            animations.Add(tweenAnimation);

            tweenAnimation.enabled = false;
#if UNITY_EDITOR
            Selection.activeObject = newAnimation;
            EditorGUIUtility.PingObject(newAnimation);
#endif
        }
    }
}