#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public class FrameData
    {
        public Sprite sprite;
        public float relativeDuration = 1;
    }
    
    [Serializable, UseCustomEditor("SpriteSwapTweenEditor"),
     StructLayout(LayoutKind.Sequential)]
    public class TweenSpriteSwap : FloatTween<SpriteRenderer>
    {
        public List<FrameData> frames = new();

        public TweenSpriteSwap()
        {
            startValue = 0;
            endValue = 1;
        }
        
        private float innerProgress;
        protected override float Property
        {
            get => innerProgress;
            set
            {
                innerProgress = value;
                float totalRelativeDuration = 0f;
                for (int i = 0; i < frames.Count; i++)
                {
                    totalRelativeDuration += frames[i].relativeDuration;
                }
                float timeToSet = Mathf.Clamp(innerProgress * totalRelativeDuration, 0f, totalRelativeDuration);
                
                float currentDuration = 0f;
                for (int i = 0; i < frames.Count; i++)
                {
                    currentDuration += frames[i].relativeDuration;
                    if (timeToSet < currentDuration)
                    {
                        target.sprite = frames[i].sprite;
                        break;
                    }
                }
                
#if UNITY_EDITOR

                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(target);
                }
#endif
            }
        }
    }
}