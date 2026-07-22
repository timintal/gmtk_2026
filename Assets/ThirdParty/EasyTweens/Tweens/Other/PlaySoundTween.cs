using System;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public class PlaySoundTween : TweenBase, ITargetSetter<AudioSource>
    {
        [ExposeInEditor]
        public AudioSource AudioSource;
        
        public override void UpdateTween(float time, float deltaTime)
        {
            if (deltaTime > 0 && (time - deltaTime) <= TotalDelay && time >= TotalDelay)
            {
                AudioSource.Play();
            }

            if (deltaTime < 0 && (time - deltaTime) >= TotalDelay && time <= TotalDelay)
            {
                AudioSource.Stop();
            }
        }
        
        public void SetTarget(AudioSource target)
        {
            AudioSource = target;
        }
    }
}