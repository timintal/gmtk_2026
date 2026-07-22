using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(ParticleSystem))]
public class ReturnParticleSystemToPool : MonoBehaviour
{
    ParticleSystem _system;
    
    IObjectPool<ParticleSystem> _pool;
    public void Init(IObjectPool<ParticleSystem> pool, ParticleSystem system)
    {
        _system = system;
        _pool = pool;
        var main = _system.main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    void OnParticleSystemStopped()
    {
        // Return to the pool
        _pool.Release(_system);
    }
}
