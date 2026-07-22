using UnityEngine;

public class ResetAnimator : MonoBehaviour, IPoolReset
{
    [SerializeField] private Animator[] _animators;
    public void ResetForReuse()
    {
        foreach (var animator in _animators)
        {
            animator.enabled = true;
        }
    }
}