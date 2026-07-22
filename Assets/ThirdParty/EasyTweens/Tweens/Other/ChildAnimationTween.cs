using System;

namespace EasyTweens
{
    [Serializable]
    public class ChildAnimationTween : TweenBase
    {
        [ExposeInEditor]
        public TweenAnimation ChildAnimation;

        public override float Duration
        {
            get
            {
#if  UNITY_EDITOR
                if (ChildAnimation == null)
                    return 0;
#endif
            
                if (ChildAnimation.timeSpeedMultiplier == 0)    
                    return 0;
            
                duration = ChildAnimation.duration / ChildAnimation.timeSpeedMultiplier;
                return duration;
            }
            set
            {
                duration = value;
                ChildAnimation.duration = value;
            }
        }

        public override void SetFactor(float f)
        {
            var newTime = f * Duration;
            ChildAnimation.SetTime(newTime, newTime - ChildAnimation.currentTime);
            ChildAnimation.currentTime = newTime;
        }
    }
}