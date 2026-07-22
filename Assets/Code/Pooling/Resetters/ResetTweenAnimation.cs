using EasyTweens;
using UnityEngine;

public class ResetTweenAnimation : MonoBehaviour, IPoolReset
{
    [SerializeField] private TweenAnimation[] _animations;

    public void ResetForReuse()
    {
        foreach (var tweenAnimation in _animations)
        {
            tweenAnimation.PlayBackward(false);
            if (tweenAnimation.playOnAwake)
                tweenAnimation.Play();
        }
    }
}