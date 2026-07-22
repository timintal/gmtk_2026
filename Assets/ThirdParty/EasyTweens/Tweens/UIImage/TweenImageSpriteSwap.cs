#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace EasyTweens
{
    [Serializable, UseCustomEditor("ImageSpriteSwapTweenEditor"),
     StructLayout(LayoutKind.Sequential)]
    public class TweenImageSpriteSwap : FloatTween<Image>
    {
        public List<FrameData> frames = new();

        public TweenImageSpriteSwap()
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