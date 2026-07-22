using System.Collections;
using UnityEngine;

public class ResetTrail : MonoBehaviour, IPoolReset
{
    [SerializeField] private TrailRenderer[] trails;

    public void ResetForReuse()
    {
        StartCoroutine(RestartCoroutine());
    }

    private IEnumerator RestartCoroutine()
    {
        foreach (var trail in trails)
        {
            trail.emitting = false;
            trail.Clear();
        }

        yield return null;
        foreach (TrailRenderer trail in trails)
        {
            trail.emitting = true;
        }
    }
}