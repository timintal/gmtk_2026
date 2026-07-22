using System;

namespace EasyTweens
{
    [Serializable]
    public class StartNextAnimation : TweenBase, ITargetSetter<TweenAnimation>
    {
        [ExposeInEditor] public TweenAnimation NextAnimation;

        public override void UpdateTween(float time, float deltaTime)
        {
            if (deltaTime > 0 && (time - deltaTime) <= TotalDelay && time >= TotalDelay)
            {
                NextAnimation.Play();
            }

            if (deltaTime < 0 && (time - deltaTime) >= TotalDelay && time <= TotalDelay)
            {
                NextAnimation.Stop();
            }
        }

        public void SetTarget(TweenAnimation target)
        {
            NextAnimation = target;
        }
    }
}