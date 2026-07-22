using UnityEngine;

public class ResetParticleSystem : MonoBehaviour, IPoolReset
{
    [SerializeField] ParticleSystem _particleSystem;
    public void ResetForReuse()
    {
        _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}