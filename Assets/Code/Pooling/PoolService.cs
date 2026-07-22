using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using UnityEngine;

public class PoolService : IResource
{
    private Dictionary<ParticleSystem, AutoDestroyParticleFxPool> _fxPools = new();
    private Dictionary<GameObject, GameObjectPool> _objectPools = new();
    private Transform _parentTransform;
    
    public PoolService()
    {
        var poolGO = new GameObject("PooledObjects");
        _parentTransform = poolGO.transform;
    }
    
    public void Clear()
    {
        foreach (var pool in _fxPools)
        {
            pool.Value.Pool.Clear();
        }
        
        foreach (var pool in _objectPools)
        {
            pool.Value.Pool.Clear();
        }
    }

    public ParticleSystem GetParticleFx(ParticleSystem template)
    {
        if (!_fxPools.ContainsKey(template))
        {
            _fxPools.Add(template, new AutoDestroyParticleFxPool(template, _parentTransform));
        }

        return _fxPools[template].Pool.Get();
    }

    public PoolableMonoBehaviour GetGameObject(GameObject template)
    {
        if (!_objectPools.ContainsKey(template))
        {
            _objectPools.Add(template, new GameObjectPool(template, _parentTransform));
        }

        var gameObject = _objectPools[template].Pool.Get();
        return gameObject.GetComponent<PoolableMonoBehaviour>();
    }
    

    public T GetPoolable<T>(GameObject template) where T : MonoBehaviour
    {
        if (!_objectPools.ContainsKey(template))
        {
            _objectPools.Add(template, new GameObjectPool(template, _parentTransform));
        }

        var gameObject = _objectPools[template].Pool.Get();
        return gameObject.GetComponent<T>();
    }
}