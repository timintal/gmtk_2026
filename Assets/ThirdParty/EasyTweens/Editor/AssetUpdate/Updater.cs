namespace EasyTweens
{
    public static class Updater 
    {
        public static void Update(TweenAnimation target)
        {
            for (int i = target.serializationVersion; i < SerializationVersion.Version; i++)
            {
                switch (i)
                {
                    case 1:
                        UpdateToVersion2(target);
                        break;
                }
            }
        
            target.serializationVersion = SerializationVersion.Version;
        }

        static void UpdateToVersion2(TweenAnimation target)
        {
            float currentDuration = target.duration;
            float tweenMaxDuration = 0f;
            foreach (var tween in target.tweens)
            {
                if (tween.Delay + tween.Duration > tweenMaxDuration)
                {
                    tweenMaxDuration = tween.Delay + tween.Duration;
                }
            }
            if (currentDuration > tweenMaxDuration + float.Epsilon)
            {
                target.allowCustomAnimationDuration = true;
            }
        }
    }
}