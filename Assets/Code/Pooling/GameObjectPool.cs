using UnityEngine;
using UnityEngine.Pool;

public class GameObjectPool
{
    private GameObject _template;

    private ObjectPool<PoolableMonoBehaviour> _pool;
    private readonly Transform _parent;

    public ObjectPool<PoolableMonoBehaviour> Pool => _pool;

    public GameObjectPool(GameObject template, Transform parent = null)
    {
        _template = template;
        _pool = new ObjectPool<PoolableMonoBehaviour>(CreatePooledItem, OnTakeFromPool, OnReturnToPool, OnDestroyPooledObject);
        _parent = parent;
    }

    private PoolableMonoBehaviour CreatePooledItem()
    {
        var obj = Object.Instantiate(_template, _parent);
        var poolableMonoBehaviour = obj.AddComponent<PoolableMonoBehaviour>();
        poolableMonoBehaviour.Init(_pool);
        return poolableMonoBehaviour;
    }

    private void OnTakeFromPool(PoolableMonoBehaviour obj)
    {
        obj.gameObject.SetActive(true);
        obj.ResetAll();
    }

    private void OnReturnToPool(PoolableMonoBehaviour obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(_parent);
    }

    private void OnDestroyPooledObject(PoolableMonoBehaviour obj)
    {
        Object.Destroy(obj);
    }
}