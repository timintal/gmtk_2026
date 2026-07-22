using System;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public class PlayParticles : TweenBase, ITargetSetter<ParticleSystem>
    {
        [ExposeInEditor]
        public ParticleSystem ParticleSystem;
        
        public override void UpdateTween(float time, float deltaTime)
        {
            if (deltaTime > 0 && (time - deltaTime) <= TotalDelay && time >= TotalDelay)
            {
                ParticleSystem.Play();
            }

            if (deltaTime < 0 && (time - deltaTime) >= TotalDelay && time <= TotalDelay)
            {
                ParticleSystem.Stop();
            }
        }

        public void SetTarget(ParticleSystem target)
        {
            ParticleSystem = target;
        }
    }
    
    [Serializable]
    public class StopParticles : TweenBase, ITargetSetter<ParticleSystem>
    {
        [ExposeInEditor]
        public ParticleSystem ParticleSystem;
        
        public override void UpdateTween(float time, float deltaTime)
        {
            if (deltaTime > 0 && (time - deltaTime) <= TotalDelay && time >= TotalDelay)
            {
                ParticleSystem.Stop();
            }

            if (deltaTime < 0 && (time - deltaTime) >= TotalDelay && time <= TotalDelay)
            {
                ParticleSystem.Play();
            }
        }

        public void SetTarget(ParticleSystem target)
        {
            ParticleSystem = target;
        }
    }
}