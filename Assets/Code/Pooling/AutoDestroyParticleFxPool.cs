using UnityEngine;
using UnityEngine.Pool;

public class AutoDestroyParticleFxPool 
{
    private ParticleSystem _template;
    private readonly Transform _parent;

    private ObjectPool<ParticleSystem> _pool;

    public ObjectPool<ParticleSystem> Pool => _pool;

    public AutoDestroyParticleFxPool(ParticleSystem template, Transform parent = null)
    {
        _template = template;
        _parent = parent;
        _pool = new ObjectPool<ParticleSystem>(CreatePooledItem, OnTakeFromPool, OnReturnToPool, OnDestroyPoolObject);
    }

    private ParticleSystem CreatePooledItem()
    {
        ParticleSystem particleSystem = Object.Instantiate(_template, _parent);
        var returnParticleSystemToPool = particleSystem.gameObject.AddComponent<ReturnParticleSystemToPool>();
        returnParticleSystemToPool.Init(_pool, particleSystem);
        return particleSystem;
    }

    private void OnTakeFromPool(ParticleSystem ps)
    {
        ps.gameObject.SetActive(true);
    }

    private void OnReturnToPool(ParticleSystem ps)
    {
        ps.gameObject.SetActive(false);
    }

    private void OnDestroyPoolObject(ParticleSystem ps)
    {
        Object.Destroy(ps.gameObject);
    }
}
