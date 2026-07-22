using UnityEngine;
using UnityEngine.Pool;

public class PoolableMonoBehaviour : MonoBehaviour, IPoolableObject<PoolableMonoBehaviour>
{
    private IObjectPool<PoolableMonoBehaviour> _parentPool;
    
    private IPoolReset[] _resetters;
    public IObjectPool<PoolableMonoBehaviour> ParentPool => _parentPool;
    
    public void ReleaseObject()
    {
        _parentPool.Release(this);
    }
    public void Init(IObjectPool<PoolableMonoBehaviour> parentPool)
    {
        _parentPool = parentPool;
        _resetters = GetComponentsInChildren<IPoolReset>();
    }

    public void ResetAll()
    {
        foreach (var resetter in _resetters)
        {
            resetter.ResetForReuse();
        }
    }
}
