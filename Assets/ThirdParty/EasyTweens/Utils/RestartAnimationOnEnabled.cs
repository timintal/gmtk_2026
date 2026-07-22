using UnityEngine;

namespace EasyTweens.Utils
{
    [RequireComponent(typeof(TweenAnimation))]
    public class RestartAnimationOnEnabled : MonoBehaviour
    {
        private TweenAnimation tweenAnimation;

        private void OnEnable()
        {
            if (tweenAnimation == null)
            {
                tweenAnimation = GetComponent<TweenAnimation>();
            }

            tweenAnimation.Play();
        }
    }
}
