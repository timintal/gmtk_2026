using System;
using UnityEngine.Events;

namespace EasyTweens
{
    [Serializable]
    public class TweenCallback : TweenBase
    {
        [ExposeInEditor]
        public UnityEvent CallbackForward;
        [ExposeInEditor]
        public UnityEvent CallbackBackward;


        public override void UpdateTween(float time, float deltaTime)
        {
            if (deltaTime > 0 && (time - deltaTime) <= TotalDelay && time >= TotalDelay)
            {
                CallbackForward?.Invoke();
            }

            if (deltaTime < 0 && (time - deltaTime) >= TotalDelay && time <= TotalDelay)
            {
                CallbackBackward?.Invoke();
            }
        }
    }
}
